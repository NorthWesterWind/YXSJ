using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Controller.Pickups;
using Module;
using Module.Data;
using PolyNav;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace Controller
{
    /// <summary>
    /// 生产怪物
    /// </summary>
    public class FactoryController : MonoBehaviour
    {
        private static readonly HashSet<int> ActiveFactoryIds = new();
        private static int NextRuntimeFactorId = 1000;
        private const string TrialModeStopEvent = "TrialModeStop";
        public bool isSpecial;
        public bool isGoldenOnly;
        private AssetHandle _assetHandle;
        public List<GameObject> monsterList = new();
        [Header("随机生成池")]
        public bool useSpawnPool;
        public List<MonsterType> spawnMonsterPool = new();
        [Header("试炼进度")]
        public bool trialProgressMode;
        public MonsterType normalType; // 普通怪
        public MonsterType goldenType; // 金色怪
        public MonsterType giantType;  // 巨型怪（可选）
        public int maxMonsterCount = 50;
        public float spawnInterval = 3f;

        public float scatterRadius = 6f;
        public AnimationCurve scatterCurve;

        // 巨人怪周期
        private int giantCounter = 0;

        // 黄金怪周期
        private int goldenCounter = 0;
        public int factorID;
        [Header("巡逻矩形区域")]
        public Vector2 patrolAreaSize = new Vector2(5f, 5f);

        [Header("调试")]
        public bool showGizmos = true;  // 是否显示矩形辅助线

        public YuanBaoKuangDongCtr dongCtr;
        private PolyNavMap _cachedMap;
        private readonly List<MonsterType> registeredFactoryTypes = new();
        private bool isTrialShutDown;
        private bool hasLoggedMonsterCap;
        private bool hasRegisteredFactorId;
        private bool hasLoggedYuanBaoProduceBlocked;

        public Vector3 GetRandomSpawnPos()
        {
            if (TryGetValidSpawnPos(out Vector2 validPos))
            {
                return new Vector3(validPos.x, validPos.y, -1f);
            }

            return new Vector3(transform.position.x, transform.position.y, -1f);
        }


        private void OnDrawGizmos()
        {
            if (!showGizmos) return;

            Gizmos.color = Color.green;
            Vector3 center = transform.position;
            Vector3 size = new Vector3(patrolAreaSize.x, patrolAreaSize.y, 0);

            // 绘制矩形边框
            Gizmos.DrawLine(center + new Vector3(-size.x / 2, -size.y / 2, 0), center + new Vector3(size.x / 2, -size.y / 2, 0));
            Gizmos.DrawLine(center + new Vector3(size.x / 2, -size.y / 2, 0), center + new Vector3(size.x / 2, size.y / 2, 0));
            Gizmos.DrawLine(center + new Vector3(size.x / 2, size.y / 2, 0), center + new Vector3(-size.x / 2, size.y / 2, 0));
            Gizmos.DrawLine(center + new Vector3(-size.x / 2, size.y / 2, 0), center + new Vector3(-size.x / 2, -size.y / 2, 0));
        }

        private void Awake()
        {
            _assetHandle = GetComponent<AssetHandle>();
            EnsureUniqueFactorId();
        }

        private void Start()
        {
            RefreshFactoryRegistration();
        }


        private bool TryGetValidSpawnPos(out Vector2 spawnPos)
        {
            const int maxAttempts = 12;
            Vector2 center = transform.position;

            if (!TryGetPolyNavMap(out var map))
            {
                spawnPos = GetRandomPointInPatrolArea();
                return true;
            }

            for (int i = 0; i < maxAttempts; i++)
            {
                Vector2 candidate = GetRandomPointInPatrolArea();
                if (map.PointIsValid(candidate))
                {
                    spawnPos = candidate;
                    return true;
                }

                Vector2 snapped = map.GetCloserEdgePoint(candidate);
                if (IsPointInsidePatrolArea(snapped) && map.PointIsValid(snapped))
                {
                    spawnPos = snapped;
                    return true;
                }
            }

            Vector2 fallback = ClampToPatrolArea(center, map.GetCloserEdgePoint(center));
            if (map.PointIsValid(fallback))
            {
                spawnPos = fallback;
                return true;
            }

            spawnPos = center;
            return false;
        }

        private Vector2 GetRandomPointInPatrolArea()
        {
            float halfWidth = patrolAreaSize.x / 2f;
            float halfHeight = patrolAreaSize.y / 2f;

            float randomX = Random.Range(-halfWidth, halfWidth);
            float randomY = Random.Range(-halfHeight, halfHeight);

            return new Vector2(transform.position.x + randomX, transform.position.y + randomY);
        }

        private bool IsPointInsidePatrolArea(Vector2 point)
        {
            float halfWidth = patrolAreaSize.x * 0.5f;
            float halfHeight = patrolAreaSize.y * 0.5f;
            return point.x >= transform.position.x - halfWidth &&
                   point.x <= transform.position.x + halfWidth &&
                   point.y >= transform.position.y - halfHeight &&
                   point.y <= transform.position.y + halfHeight;
        }

        private Vector2 ClampToPatrolArea(Vector2 start, Vector2 target)
        {
            float halfW = patrolAreaSize.x * 0.5f;
            float halfH = patrolAreaSize.y * 0.5f;

            float minX = start.x - halfW;
            float maxX = start.x + halfW;
            float minY = start.y - halfH;
            float maxY = start.y + halfH;

            return new Vector2(
                Mathf.Clamp(target.x, minX, maxX),
                Mathf.Clamp(target.y, minY, maxY));
        }

        private bool TryGetPolyNavMap(out PolyNavMap map)
        {
            map = _cachedMap;
            if (map == null)
            {
                map = PolyNavMap.current;
            }

            if (map == null)
            {
                var mapObj = GameObject.FindWithTag("Map");
                if (mapObj != null)
                {
                    map = mapObj.GetComponent<PolyNavMap>();
                }
            }

            if (map == null)
            {
                var mapObjByName = GameObject.Find("Map");
                if (mapObjByName != null)
                {
                    map = mapObjByName.GetComponent<PolyNavMap>();
                }
            }

            if (map == null)
            {
                map = FindObjectOfType<PolyNavMap>();
            }

            if (map != null && map.nodesCount == 0)
            {
                map.GenerateMap();
            }

            _cachedMap = map;
            return map != null && map.nodesCount > 0;
        }



        private void OnEnable()
        {
            EnsureUniqueFactorId();
            AddEvent();
            RefreshFactoryRegistration();
        }

        private void OnDisable()
        {
            ReleaseFactorId();
            UnregisterFactoryRegistration();
            EventCenter.Instance.RemoveListener(EventMessages.MonsterDead, HandleMonsterDead);
            EventCenter.Instance.RemoveListener(EventMessages.MonsterBeginCreate, HandleMonsterCreate);
            EventCenter.Instance.RemoveListener(TrialModeStopEvent, HandleTrialModeStop);
        }

        private void AddEvent()
        {
            EventCenter.Instance.AddListener(EventMessages.MonsterDead, HandleMonsterDead);
            EventCenter.Instance.AddListener(EventMessages.MonsterBeginCreate, HandleMonsterCreate);
            EventCenter.Instance.AddListener(TrialModeStopEvent, HandleTrialModeStop);
        }

        private void OnDestroy()
        {
            ReleaseFactorId();
            UnregisterFactoryRegistration();
            EventCenter.Instance.RemoveListener(EventMessages.MonsterDead, HandleMonsterDead);
            EventCenter.Instance.RemoveListener(EventMessages.MonsterBeginCreate, HandleMonsterCreate);
            EventCenter.Instance.RemoveListener(TrialModeStopEvent, HandleTrialModeStop);
        }

        private void EnsureUniqueFactorId()
        {
            if (hasRegisteredFactorId && factorID > 0 && ActiveFactoryIds.Contains(factorID))
            {
                return;
            }

            if (factorID > 0 && !ActiveFactoryIds.Contains(factorID))
            {
                ActiveFactoryIds.Add(factorID);
                hasRegisteredFactorId = true;
                NextRuntimeFactorId = Mathf.Max(NextRuntimeFactorId, factorID + 1);
                return;
            }

            while (ActiveFactoryIds.Contains(NextRuntimeFactorId))
            {
                NextRuntimeFactorId++;
            }

            factorID = NextRuntimeFactorId++;
            ActiveFactoryIds.Add(factorID);
            hasRegisteredFactorId = true;
        }

        private void ReleaseFactorId()
        {
            if (!hasRegisteredFactorId)
            {
                return;
            }

            if (factorID > 0)
            {
                ActiveFactoryIds.Remove(factorID);
            }
            hasRegisteredFactorId = false;
        }

        private void RefreshFactoryRegistration()
        {
            UnregisterFactoryRegistration();

            if (GameController.Instance == null || GameController.Instance.factoryControllers == null)
            {
                return;
            }

            List<MonsterType> lookupTypes = GetFactoryLookupTypes();
            for (int i = 0; i < lookupTypes.Count; i++)
            {
                MonsterType monsterType = lookupTypes[i];
                GameController.Instance.factoryControllers[monsterType] = this;
                registeredFactoryTypes.Add(monsterType);
            }
        }

        private void UnregisterFactoryRegistration()
        {
            if (registeredFactoryTypes.Count == 0)
            {
                return;
            }

            if (GameController.Instance != null && GameController.Instance.factoryControllers != null)
            {
                for (int i = 0; i < registeredFactoryTypes.Count; i++)
                {
                    MonsterType monsterType = registeredFactoryTypes[i];
                    if (GameController.Instance.factoryControllers.TryGetValue(monsterType, out var controller) &&
                        controller == this)
                    {
                        GameController.Instance.factoryControllers.Remove(monsterType);
                    }
                }
            }

            registeredFactoryTypes.Clear();
        }

        private List<MonsterType> GetFactoryLookupTypes()
        {
            List<MonsterType> result = new();
            if (isSpecial)
            {
                AddLookupMonsterType(result, giantType);
                return result;
            }

            if (isGoldenOnly)
            {
                AddLookupMonsterType(result, MonsterType.JingYuanBao);
                return result;
            }

            if (useSpawnPool && spawnMonsterPool != null && spawnMonsterPool.Count > 0)
            {
                for (int i = 0; i < spawnMonsterPool.Count; i++)
                {
                    AddLookupMonsterType(result, spawnMonsterPool[i]);
                }
                if (result.Count > 0)
                {
                    return result;
                }
            }

            AddLookupMonsterType(result, normalType);
            AddLookupMonsterType(result, goldenType);
            AddLookupMonsterType(result, giantType);
            return result;
        }

        private static void AddLookupMonsterType(List<MonsterType> result, MonsterType monsterType)
        {
            if (monsterType == MonsterType.None || result.Contains(monsterType))
            {
                return;
            }

            result.Add(monsterType);
        }


        IEnumerator SpawnLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(spawnInterval);
                CleanupMonsterList();
                if (monsterList.Count >= maxMonsterCount)
                {
                    if (!hasLoggedMonsterCap)
                    {
                        hasLoggedMonsterCap = true;
                        Debug.Log($"[FactoryController] {name} monster count reached cap: {monsterList.Count}/{maxMonsterCount}");
                    }
                    continue;
                }
                hasLoggedMonsterCap = false;

                // 检查数量
                if (monsterList.Count < maxMonsterCount)
                {
                    if (isGoldenOnly)
                    {
                        SpawnMonster(dongCtr);
                    }
                    else
                    {
                        SpawnMonster();
                    }

                }
                else
                {
                    Debug.Log("怪物数量已达上限，暂停生产");
                }
            }
        }


        private void SpawnMonster()
        {
            TrySpawnMonster(DecideSpawnType());
        }


        private void SpawnMonster(YuanBaoKuangDongCtr limitCtr)
        {
            if (limitCtr != null && !limitCtr.CanProduce())
            {
                if (!hasLoggedYuanBaoProduceBlocked)
                {
                    int remainCount = PlayerDataModule.Instance?.data?.remainCount ?? -1;
                    string lastRefreshTime = PlayerDataModule.Instance?.data?.lastRefrashTime ?? string.Empty;
                    Debug.Log($"[FactoryController] {name} skipped JingYuanBao spawn because remainCount={remainCount}, lastRefreshTime={lastRefreshTime}");
                    hasLoggedYuanBaoProduceBlocked = true;
                }
                return;
            }

            hasLoggedYuanBaoProduceBlocked = false;

            if (TrySpawnMonster(DecideSpawnType()))
            {
                limitCtr?.ConsumeOne();
            }
        }

        private void InternalSpawnMonster()
        {
            TrySpawnMonster(DecideSpawnType());
        }

        public bool SpawnRuntimeMonster(MonsterType monsterType, Vector3 position)
        {
            return TrySpawnMonster(monsterType, position);
        }

        public bool SpawnRuntimeMonster(MonsterType monsterType)
        {
            return TrySpawnMonster(monsterType);
        }

        public List<Vector3> GetLiveMonsterPositions(MonsterType monsterType)
        {
            CleanupMonsterList();
            List<Vector3> positions = new();
            if (monsterList == null || monsterList.Count == 0)
            {
                return positions;
            }

            for (int i = 0; i < monsterList.Count; i++)
            {
                var monster = monsterList[i];
                if (monster == null) continue;
                if (!monster.activeInHierarchy) continue;
                if (!monster.TryGetComponent(out MonsterController monsterController)) continue;
                if (monsterController.monsterType != monsterType) continue;
                positions.Add(monster.transform.position);
            }

            return positions;
        }

        public void ClearLiveMonsters(MonsterType monsterType, bool createDrops = false)
        {
            CleanupMonsterList();
            if (monsterList == null || monsterList.Count == 0)
            {
                return;
            }

            List<GameObject> targets = new();
            for (int i = 0; i < monsterList.Count; i++)
            {
                var monster = monsterList[i];
                if (monster == null) continue;
                if (!monster.TryGetComponent(out MonsterController monsterController)) continue;
                if (monsterController.monsterType != monsterType) continue;
                targets.Add(monster);
            }

            for (int i = 0; i < targets.Count; i++)
            {
                RemoveMonster(targets[i], createDrops);
            }
        }

        private bool TrySpawnMonster(MonsterType toSpawnType)
        {
            return TrySpawnMonster(toSpawnType, null);
        }

        private bool TrySpawnMonster(MonsterType toSpawnType, Vector3? spawnPosition)
        {
            if (toSpawnType == MonsterType.None)
            {
                return false;
            }

            if (_assetHandle == null || DataController.Instance == null || DataController.Instance.monsterDataDic == null)
            {
                return false;
            }

            if (!DataController.Instance.monsterDataDic.TryGetValue(toSpawnType, out MonsterData data) || data == null)
            {
                Debug.LogWarning($"[FactoryController] 缺少怪物数据: {toSpawnType}");
                return false;
            }

            string resName = Extensions.GetMonsterResNameByType(toSpawnType);
            if (string.IsNullOrEmpty(resName))
            {
                Debug.LogWarning($"[FactoryController] 缺少怪物资源名: {toSpawnType}");
                return false;
            }

            GameObject prefab = _assetHandle.Get<GameObject>(resName);
            if (prefab == null)
            {
                Debug.LogWarning($"[FactoryController] 缺少怪物预制体: {resName}");
                return false;
            }

            GameObject monster = GameObject.Instantiate(prefab);
            if (monster == null)
            {
                return false;
            }

            monster.transform.position = spawnPosition ?? GetRandomSpawnPos();

            if (!monster.TryGetComponent(out MonsterController monsterController))
            {
                Debug.LogWarning($"[FactoryController] 怪物预制体缺少 MonsterController: {resName}");
                Destroy(monster);
                return false;
            }

            monsterController.Init(
                data,
                transform.position,
                ResolveSpawnBehavior(toSpawnType),
                factorID,
                patrolAreaSize);

            monsterList.Add(monster);
            return true;
        }

        private static MonsterBehavior ResolveSpawnBehavior(MonsterType monsterType)
        {
            if (monsterType == MonsterType.JingYuanBao)
            {
                return MonsterBehavior.Normal;
            }

            int value = (int)monsterType;
            if (value <= 0)
            {
                return MonsterBehavior.Normal;
            }

            int variant = (value - 1) % 3;
            if (variant == 1)
            {
                return MonsterBehavior.Golden;
            }

            if (variant == 2)
            {
                return MonsterBehavior.Giant;
            }

            return MonsterBehavior.Normal;
        }





        // 决定下一只怪物品质 
        private MonsterType DecideSpawnType()
        {
            if (isSpecial)
            {
                return giantType;
            }
            if (isGoldenOnly)
            {
                return MonsterType.JingYuanBao;
            }

            if (useSpawnPool && spawnMonsterPool != null && spawnMonsterPool.Count > 0)
            {
                MonsterType poolType = GetRandomSpawnTypeFromPool();
                if (poolType != MonsterType.None)
                {
                    return poolType;
                }
            }

            giantCounter++;
            goldenCounter++;
            if (giantType != MonsterType.None && giantCounter >= 25)
            {
                giantCounter = 0;
                return giantType;
            }

            if (goldenCounter >= 40)
            {
                goldenCounter = 0;
                return goldenType;
            }

            return normalType;
        }

        private MonsterType GetRandomSpawnTypeFromPool()
        {
            int validCount = 0;
            for (int i = 0; i < spawnMonsterPool.Count; i++)
            {
                if (spawnMonsterPool[i] != MonsterType.None)
                {
                    validCount++;
                }
            }

            if (validCount == 0)
            {
                return MonsterType.None;
            }

            int targetIndex = Random.Range(0, validCount);
            for (int i = 0; i < spawnMonsterPool.Count; i++)
            {
                MonsterType monsterType = spawnMonsterPool[i];
                if (monsterType == MonsterType.None)
                {
                    continue;
                }

                if (targetIndex == 0)
                {
                    return monsterType;
                }

                targetIndex--;
            }

            return MonsterType.None;
        }



        // 当怪物死亡时记得从列表移除
        private void RemoveMonster(GameObject monster, bool createDrops = true)
        {
            // 如果怪物不在列表中，说明已经处理过了，直接返回
            if (monster == null || !monsterList.Contains(monster))
            {
                return;
            }

            monsterList.Remove(monster);

            if (!monster.TryGetComponent(out MonsterController monsterController))
            {
                Destroy(monster);
                return;
            }

            if (createDrops)
            {
                GetDropType(monsterController.monsterType);
            }
            Vector3 bornPos = new Vector3(monster.transform.position.x, monster.transform.position.y,
                monster.transform.position.z);
            Destroy(monster);
            if (createDrops)
            {
                StartCoroutine(ScatterDrops(bornPos));
            }
        }


        IEnumerator ScatterDrops(Vector3 bornPos)
        {
            foreach (var kv in dropDict.ToList())
            {
                int dropCount = kv.Value;
                DropItemType dropType = kv.Key;

                for (int i = 0; i < dropCount; i++)
                {

                    GameObject drop = GameObject.Instantiate(_assetHandle.Get<GameObject>("DropObj"));
                    drop.GetComponent<DropController>().Init(dropType);
                    drop.transform.position = bornPos;
                    StartCoroutine(FlyDrop(drop, bornPos));
                }
            }

            yield break;
        }

        IEnumerator FlyDrop(GameObject drop, Vector3 bornPos)
        {
            Vector2 start = bornPos;

            Vector2 randomOffset = Random.insideUnitCircle.normalized * scatterRadius;
            Vector2 rawTarget = start + randomOffset;

            Vector2 target = ClampToPatrolAreaAlongRay(start, rawTarget);

            Vector2 control = Vector2.Lerp(start, target, 0.5f) + Vector2.up * 1.5f;

            float timer = 0f;
            float duration = 0.3f;

            while (timer < duration)
            {
                float t = scatterCurve.Evaluate(timer / duration);

                Vector2 pos =
                    (1 - t) * (1 - t) * start +
                    2 * (1 - t) * t * control +
                    t * t * target;

                drop.transform.position = pos;
                timer += Time.deltaTime;
                yield return null;
            }

            drop.transform.position = target;
            drop.GetComponent<DropController>().canPickup = true;
        }


        Vector2 ClampToPatrolAreaAlongRay(Vector2 start, Vector2 target)
        {
            float halfW = patrolAreaSize.x * 0.5f;
            float halfH = patrolAreaSize.y * 0.5f;

            float minX = transform.position.x - halfW;
            float maxX = transform.position.x + halfW;
            float minY = transform.position.y - halfH;
            float maxY = transform.position.y + halfH;

            // 直接将目标点限制在巡逻区域内
            float clampedX = Mathf.Clamp(target.x, minX, maxX);
            float clampedY = Mathf.Clamp(target.y, minY, maxY);

            return new Vector2(clampedX, clampedY);
        }



        private void HandleMonsterDead(params object[] args)
        {
            if (args == null || args.Length < 3)
            {
                return;
            }
            var type = (MonsterType)args[0];
            var target = (GameObject)args[1];
            var id = (int)args[2];
            if (id != factorID)
                return;
            if (!IsSpawnedMonsterType(type))
            {
                return;
            }
            if (trialProgressMode)
            {
                RemoveMonster(target, false);
                EventCenter.Instance.TriggerEvent("TrialMonsterDead", factorID, type, 1f);
                return;
            }

            RemoveMonster(target);
        }

        private Coroutine spawnCoroutine;

        private void HandleMonsterCreate(params object[] args)
        {
            if (trialProgressMode && isTrialShutDown)
            {
                return;
            }

            RefreshFactoryRegistration();
            if (spawnCoroutine != null) return;
            spawnCoroutine = StartCoroutine(SpawnLoop());
        }

        private void HandleTrialModeStop(params object[] args)
        {
            if (!trialProgressMode)
            {
                return;
            }

            StopSpawnAndClearMonsters();
        }

        public void StopSpawnAndClearMonsters()
        {
            if (trialProgressMode)
            {
                isTrialShutDown = true;
            }

            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }

            CleanupMonsterList();
            for (int i = 0; i < monsterList.Count; i++)
            {
                GameObject monster = monsterList[i];
                if (monster != null)
                {
                    Destroy(monster);
                }
            }

            monsterList.Clear();
            giantCounter = 0;
            goldenCounter = 0;
            hasLoggedMonsterCap = false;
            dropDict.Clear();
        }

        private bool IsSpawnedMonsterType(MonsterType monsterType)
        {
            if (isSpecial)
            {
                return monsterType == giantType;
            }

            if (isGoldenOnly)
            {
                return monsterType == MonsterType.JingYuanBao;
            }

            if (useSpawnPool && spawnMonsterPool != null && spawnMonsterPool.Count > 0)
            {
                bool hasValidPoolType = false;
                for (int i = 0; i < spawnMonsterPool.Count; i++)
                {
                    MonsterType poolType = spawnMonsterPool[i];
                    if (poolType == MonsterType.None)
                    {
                        continue;
                    }

                    hasValidPoolType = true;
                    if (poolType == monsterType)
                    {
                        return true;
                    }
                }

                if (!hasValidPoolType)
                {
                    return monsterType == normalType || monsterType == giantType || monsterType == goldenType;
                }

                return false;
            }

            return monsterType == normalType || monsterType == giantType || monsterType == goldenType;
        }

        private void CleanupMonsterList()
        {
            if (monsterList == null || monsterList.Count == 0)
            {
                return;
            }

            monsterList.RemoveAll(monster => monster == null);
        }

        Dictionary<DropItemType, int> dropDict = new();

        private void GetDropType(MonsterType monsterType)
        {
            dropDict.Clear();
            switch (monsterType)
            {
                case MonsterType.ShuangYunZhi:
                    dropDict[DropItemType.ShuangYunZhiFragment] = 1;
                    break;
                case MonsterType.ShuangYunZhiGolden:
                    dropDict[DropItemType.ShuangYunZhiFragment] = 5;
                    dropDict[DropItemType.YingQian] = 5;
                    break;
                case MonsterType.ShuangYunZhiBig:
                    dropDict[DropItemType.ShuangYunZhiFragment] = 3;
                    break;
                case MonsterType.YueLuCao:
                    dropDict[DropItemType.YueLuCaoFragment] = 1;
                    break;
                case MonsterType.YueLuCaoGolden:
                    dropDict[DropItemType.YueLuCaoFragment] = 5;
                    dropDict[DropItemType.YingQian] = 5;
                    break;
                case MonsterType.YueLuCaoBig:
                    dropDict[DropItemType.YueLuCaoFragment] = 3;
                    break;
                case MonsterType.ZiXinHua:
                    dropDict[DropItemType.ZiXinHuaFragment] = 1;
                    break;
                case MonsterType.ZiXinHuaGolden:
                    dropDict[DropItemType.ZiXinHuaFragment] = 10;
                    dropDict[DropItemType.YingQian] = 5;
                    break;
                case MonsterType.ZiXinHuaBig:
                    dropDict[DropItemType.ZiXinHuaFragment] = 3;
                    break;
                case MonsterType.YuHuiHe:
                    dropDict[DropItemType.YuHuiHeFragment] = 1;
                    break;
                case MonsterType.YuHuiHeGolden:
                    dropDict[DropItemType.YuHuiHeFragment] = 5;
                    dropDict[DropItemType.YingQian] = 5;
                    break;
                case MonsterType.YuHuiHeBig:
                    dropDict[DropItemType.YuHuiHeFragment] = 3;
                    break;
                case MonsterType.XingWenGuo:
                    dropDict[DropItemType.XingWenGuoFragment] = 1;
                    break;
                case MonsterType.XingWenGuoGolden:
                    dropDict[DropItemType.XingWenGuoFragment] = 5;
                    dropDict[DropItemType.YingQian] = 5;
                    break;
                case MonsterType.XingWenGuoBig:
                    dropDict[DropItemType.XingWenGuoFragment] = 3;
                    break;
                case MonsterType.WuRongJun:
                    dropDict[DropItemType.WuRongJunFragment] = 1;
                    break;
                case MonsterType.WuRongJunBig:
                    dropDict[DropItemType.WuRongJunFragment] = 3;
                    break;
                case MonsterType.WuRongJunGolden:
                    dropDict[DropItemType.WuRongJunFragment] = 5;
                    dropDict[DropItemType.YingQian] = 5;
                    break;
                case MonsterType.LingXuSheng:
                    dropDict[DropItemType.LingXuShengFragment] = 1;
                    break;
                case MonsterType.LingXuShengGolden:
                    dropDict[DropItemType.LingXuShengFragment] = 5;
                    dropDict[DropItemType.YingQian] = 5;
                    break;
                case MonsterType.LingXuShengBig:
                    dropDict[DropItemType.LingXuShengFragment] = 3;
                    break;
                case MonsterType.XueBanHua:
                    dropDict[DropItemType.XueBanHuaFragment] = 1;
                    break;
                case MonsterType.XueBanHuaGolden:
                    dropDict[DropItemType.XueBanHuaFragment] = 5;
                    dropDict[DropItemType.YingQian] = 5;
                    break;
                case MonsterType.XueBanHuaBig:
                    dropDict[DropItemType.XueBanHuaFragment] = 3;
                    break;
                case MonsterType.MuLingYa:
                    dropDict[DropItemType.MuLingYaFragment] = 1;
                    break;
                case MonsterType.MuLingYaGolden:
                    dropDict[DropItemType.MuLingYaFragment] = 5;
                    dropDict[DropItemType.YingQian] = 5;
                    break;
                case MonsterType.MuLingYaBig:
                    dropDict[DropItemType.MuLingYaFragment] = 3;
                    break;
                case MonsterType.JingRuiCao:
                    dropDict[DropItemType.JingRuiCaoFragment] = 1;
                    break;
                case MonsterType.JingRuiCaoGolden:
                    dropDict[DropItemType.JingRuiCaoFragment] = 5;
                    dropDict[DropItemType.YingQian] = 5;
                    break;
                case MonsterType.JingRuiCaoBig:
                    dropDict[DropItemType.JingRuiCaoFragment] = 3;
                    break;
                case MonsterType.TieKuangShi:
                    dropDict[DropItemType.TieKuangShiFragment] = 1;
                    break;
                case MonsterType.TieKuangShiGolden:
                    dropDict[DropItemType.TieKuangShiFragment] = 5;
                    dropDict[DropItemType.YingQian] = 5;
                    break;
                case MonsterType.TieKuangShiBig:
                    dropDict[DropItemType.TieKuangShiFragment] = 3;
                    break;
                case MonsterType.YinKuangShi:
                    dropDict[DropItemType.YinKuangShiFragment] = 1;
                    break;
                case MonsterType.YinKuangShiGolden:
                    dropDict[DropItemType.YinKuangShiFragment] = 5;
                    dropDict[DropItemType.YingQian] = 5;
                    break;
                case MonsterType.YinKuangShiBig:
                    dropDict[DropItemType.YinKuangShiFragment] = 3;
                    break;
                case MonsterType.TongKuangShi:
                    dropDict[DropItemType.TongKuangShiFragment] = 1;
                    break;
                case MonsterType.TongKuangShiGolden:
                    dropDict[DropItemType.TongKuangShiFragment] = 5;
                    dropDict[DropItemType.YingQian] = 5;
                    break;
                case MonsterType.TongKuangShiBig:
                    dropDict[DropItemType.TongKuangShiFragment] = 3;
                    break;
                case MonsterType.ZiJingShi:
                    dropDict[DropItemType.ZiJingShiFragment] = 1;
                    break;
                case MonsterType.ZiJingShiGolden:
                    dropDict[DropItemType.ZiJingShiFragment] = 5;
                    dropDict[DropItemType.YingQian] = 5;
                    break;
                case MonsterType.ZiJingShiBig:
                    dropDict[DropItemType.ZiJingShiFragment] = 3;
                    break;
                case MonsterType.YueJingShi:
                    dropDict[DropItemType.YueJingShiFragment] = 1;
                    break;
                case MonsterType.YueJingShiGolden:
                    dropDict[DropItemType.YueJingShiFragment] = 5;
                    dropDict[DropItemType.YingQian] = 5;
                    break;
                case MonsterType.YueJingShiBig:
                    dropDict[DropItemType.YueJingShiFragment] = 3;
                    break;
                case MonsterType.JingYuanBao:
                    dropDict[DropItemType.JingYuanBao] = 2;
                    break;
            }
        }
    }
}
