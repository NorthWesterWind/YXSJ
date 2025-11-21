using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Controller.Pickups;
using Module.Data;
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
        public bool isSpecial;
        private AssetHandle _assetHandle;
        public List<GameObject> monsterList = new();
        public MonsterType normalType; // 普通怪
        public MonsterType goldenType; // 金色怪
        public MonsterType giantType;  // 巨型怪（可选）
        public int maxMonsterCount = 50;
        private float spawnInterval = 3f;

        public float scatterRadius = 6f;
        public AnimationCurve scatterCurve;
        private float scatterDuration = 0.5f;
        
        private int spawnCounter = 0;
        // 巨人怪周期
        private int giantCounter = 0;
        // 黄金怪周期
        private int goldenCounter = 0;
        public int factorID ;
        private void Awake()
        {
            _assetHandle = GetComponent<AssetHandle>();
        }

        void Start()
        {
            AddEvent();
            if (isSpecial)
            {
                ObjectPoolManager.Instance.WarmPool(Extensions.GetMonsterResNameByType(giantType) , _assetHandle.Get<GameObject>(Extensions.GetMonsterResNameByType(giantType)) ,10);
            }
            else
            {
                ObjectPoolManager.Instance.WarmPool(Extensions.GetMonsterResNameByType(normalType) , _assetHandle.Get<GameObject>(Extensions.GetMonsterResNameByType(normalType)) ,40);
                ObjectPoolManager.Instance.WarmPool(Extensions.GetMonsterResNameByType(giantType) , _assetHandle.Get<GameObject>(Extensions.GetMonsterResNameByType(giantType)) ,10);
                ObjectPoolManager.Instance.WarmPool(Extensions.GetMonsterResNameByType(goldenType) , _assetHandle.Get<GameObject>(Extensions.GetMonsterResNameByType(goldenType)) ,5);

            }
        }

        void Update()
        {
        }

        private void AddEvent()
        {
            EventCenter.Instance.AddListener(EventMessages.MonsterDead, HandleMonsterDead);
            EventCenter.Instance.AddListener(EventMessages.MonsterBeginCreate, HandleMonsterCreate);
        }

        private void OnDestroy()
        {
            EventCenter.Instance.RemoveListener(EventMessages.MonsterDead, HandleMonsterDead);
            EventCenter.Instance.RemoveListener(EventMessages.MonsterBeginCreate, HandleMonsterCreate);
        }


        IEnumerator SpawnLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(spawnInterval);

                // 检查数量
                if (monsterList.Count < maxMonsterCount)
                {
                    SpawnMonster();
                }
                else
                {
                    Debug.Log("怪物数量已达上限，暂停生产");
                }
            }
        }
        
        private void SpawnMonster()
        {
            MonsterType toSpawnType = DecideSpawnType();  
            GameObject monster = ObjectPoolManager.Instance.GetObject(Extensions.GetMonsterResNameByType(toSpawnType));
            MonsterData data = DataController.Instance.monsterDataDic[toSpawnType];
            monster.transform.position = GetRandomSpawnPos();
           MonsterBehavior behavior = MonsterBehavior.Normal;
            if (toSpawnType == giantType)
            {
                behavior = MonsterBehavior.Giant;
            }
            else if (toSpawnType == goldenType)
            {
                behavior =  MonsterBehavior.Golden;
            }
            monster.GetComponent<MonsterController>().Init(
                data,
                transform.position,
                behavior , factorID);

            monsterList.Add(monster);
            
        }

        // 决定下一只怪物品质 
        private MonsterType DecideSpawnType()
        {
            if (isSpecial)
            {
                return giantType;
            }
            giantCounter++;
            goldenCounter++;
            if (giantType != MonsterType.None &&  giantCounter >= 30)
            {

                giantCounter = 0;
                return giantType;
            }
            
            if (goldenCounter >= 60)
            {
                goldenCounter = 0;
                return goldenType;
            }
            
            return normalType;
        }
        
        Vector3 GetRandomSpawnPos()
        {
            // 可根据需求换成地图内随机点
            Vector3 bornPos = new Vector3(Random.Range(-5f, 5f) + transform.position.x,
                Random.Range(-5f, 5f) + transform.position.y, -1);
            return bornPos;
        }

        // 当怪物死亡时记得从列表移除
        private void RemoveMonster(GameObject monster)
        {
            if (monsterList.Contains(monster))
            {
                monsterList.Remove(monster);
            }

            GetDropType(monster.GetComponent<MonsterController>().monsterType);
            Vector3 bornPos = new Vector3(monster.transform.position.x, monster.transform.position.y,
                monster.transform.position.z);
            ObjectPoolManager.Instance.ReturnObject(Extensions.GetMonsterResNameByType(monster.GetComponent<MonsterController>().monsterType), monster);
            StartCoroutine(ScatterDrops(bornPos));
        }

        IEnumerator ScatterDrops(Vector3 bornPos)
        {
            foreach (var value in dropDict.ToList()) // 复制一个列表，安全遍历
            {
                int dropCount = value.Value;
                for (int i = 0; i < dropCount; i++)
                {
                    GameObject drop = ObjectPoolManager.Instance.GetObject("DropObj");
                    drop.GetComponent<DropController>().Init(value.Key);
                    drop.transform.position = bornPos;
                    Vector2 target = (Vector2)bornPos + Random.insideUnitCircle.normalized * scatterRadius;
                    Vector2 start = bornPos;
                    Vector2 control = Vector2.Lerp(start, target, 0.5f) + Vector2.up * 1.5f;

                    float timer = 0;
                    while (timer < scatterDuration)
                    {
                        float t = scatterCurve.Evaluate(timer / scatterDuration);
                        Vector2 pos = (1 - t) * (1 - t) * start + 2 * (1 - t) * t * control + t * t * target;
                        drop.transform.position = pos;
                        timer += Time.deltaTime;
                        yield return null;
                    }
                    drop.transform.position = target;
                    drop.GetComponent<DropController>().isAttracted = false;
                }
            }
            
        }

        private void HandleMonsterDead(params object[] args)
        {
            var type = (MonsterType)args[0];
            var target = (GameObject)args[1];
            if (type != normalType && type != giantType && type != goldenType)
            {
                return;
            }
            RemoveMonster(target);
        }

        private void HandleMonsterCreate(params object[] args)
        {
            StartCoroutine(SpawnLoop());
        }
        
        Dictionary<DropItemType,int> dropDict = new ();
        private void GetDropType(MonsterType monsterType)
        {
            dropDict.Clear();
                switch (monsterType)
                {
                     case MonsterType.ShuangYunZhi: 
                         dropDict[DropItemType.ShuangYunZhiFragment] = 1;
                        break;
                    case MonsterType.ShuangYunZhiGolden:
                        dropDict[DropItemType.ShuangYunZhiFragment] = 10;
                        dropDict[DropItemType.YingQian] = 10;
                        break;
                    case MonsterType.ShuangYunZhiBig:
                        dropDict[DropItemType.ShuangYunZhiFragment] = 4;
                        break;
                    case MonsterType.YueLuCao:
                        dropDict[DropItemType.YueLuCaoFragment] = 1;
                        break;
                    case MonsterType.YueLuCaoGolden:
                        dropDict[DropItemType.YueLuCaoFragment] = 10;
                        dropDict[DropItemType.YingQian] = 10;
                        break;
                    case MonsterType.YueLuCaoBig:
                        dropDict[DropItemType.YueLuCaoFragment] = 4;
                        break;
                    case MonsterType.ZiXinHua:
                        dropDict[DropItemType.ZiXinHuaFragment] = 1;
                        break;
                    case MonsterType.ZiXinHuaGolden:
                        dropDict[DropItemType.ZiXinHuaFragment] = 10;
                        dropDict[DropItemType.YingQian] = 10;
                        break;
                    case MonsterType.ZiXinHuaBig:
                        dropDict[DropItemType.ZiXinHuaFragment] = 4;
                        break;
                    case MonsterType.YuHuiHe:
                        dropDict[DropItemType.YuHuiHeFragment] = 1;
                        break;
                    case MonsterType.YuHuiHeGolden:
                        dropDict[DropItemType.YuHuiHeFragment] = 10;
                        dropDict[DropItemType.YingQian] = 10;
                        break;
                    case MonsterType.YuHuiHeBig:
                        dropDict[DropItemType.YuHuiHeFragment] = 4;
                        break;
                    case MonsterType.XingWenGuo:
                        dropDict[DropItemType.XingWenGuoFragment] = 1;
                        break;
                    case MonsterType.XingWenGuoGolden:
                        dropDict[DropItemType.XingWenGuoFragment] = 10;
                        dropDict[DropItemType.YingQian] = 10;
                        break;
                    case MonsterType.XingWenGuoBig:
                        dropDict[DropItemType.XingWenGuoFragment] = 4;
                        break;
                    case MonsterType.WuRongJun:
                        dropDict[DropItemType.WuRongJunFragment] = 1;
                        break;
                    case MonsterType.WuRongJunBig:
                        dropDict[DropItemType.WuRongJunFragment] = 4;
                        break;
                    case MonsterType.WuRongJunGolden:
                        dropDict[DropItemType.WuRongJunFragment] = 10;
                        dropDict[DropItemType.YingQian] = 10;
                        break;
                    case MonsterType.LingXuSheng:
                        dropDict[DropItemType.LingXuShengFragment] = 1;
                        break;
                    case MonsterType.LingXuShengGolden:
                        dropDict[DropItemType.LingXuShengFragment] = 10;
                        dropDict[DropItemType.YingQian] = 10;
                        break;
                    case MonsterType.LingXuShengBig:
                        dropDict[DropItemType.LingXuShengFragment] = 4;
                        break;
                    case MonsterType.XueBanHua:
                        dropDict[DropItemType.XueBanHuaFragment] = 1;
                        break;
                    case MonsterType.XueBanHuaGolden:
                        dropDict[DropItemType.XueBanHuaFragment] = 10;
                        dropDict[DropItemType.YingQian] = 10;
                        break;
                    case MonsterType.XueBanHuaBig:
                        dropDict[DropItemType.XueBanHuaFragment] = 4;
                        break;
                    case MonsterType.MuLingYa:
                        dropDict[DropItemType.MuLingYaFragment] = 1;
                        break;
                    case MonsterType.MuLingYaGolden:
                        dropDict[DropItemType.MuLingYaFragment] = 10;
                        dropDict[DropItemType.YingQian] = 10;
                        break;
                    case MonsterType.MuLingYaBig:
                        dropDict[DropItemType.MuLingYaFragment] = 4;
                        break;
                    case MonsterType.JingRuiCao:
                        dropDict[DropItemType.JingRuiCaoFragment] = 1;
                        break;
                    case MonsterType.JingRuiCaoGolden:
                        dropDict[DropItemType.JingRuiCaoFragment] = 10;
                        dropDict[DropItemType.YingQian] = 10;
                        break;
                    case MonsterType.JingRuiCaoBig:
                        dropDict[DropItemType.JingRuiCaoFragment] = 4;
                        break;
                    case MonsterType.TieKuangShi:
                        dropDict[DropItemType.TieKuangShiFragment] = 1;
                        break;
                    case MonsterType.TieKuangShiGolden:
                        dropDict[DropItemType.TieKuangShiFragment] = 10;
                        dropDict[DropItemType.YingQian] = 10;
                        break;
                    case MonsterType.TieKuangShiBig:
                        dropDict[DropItemType.TieKuangShiFragment] = 4;
                        break;
                    case MonsterType.YinKuangShi:
                        dropDict[DropItemType.YinKuangShiFragment] = 1;
                        break;
                    case MonsterType.YinKuangShiGolden:
                        dropDict[DropItemType.YinKuangShiFragment] = 10;
                        dropDict[DropItemType.YingQian] = 10;
                        break;
                    case MonsterType.YinKuangShiBig:
                        dropDict[DropItemType.YinKuangShiFragment] = 4;
                        break;
                    case MonsterType.TongKuangShi:
                        dropDict[DropItemType.TongKuangShiFragment] = 1;
                        break;
                    case MonsterType.TongKuangShiGolden:
                        dropDict[DropItemType.TongKuangShiFragment] = 10;
                        dropDict[DropItemType.YingQian] = 10;
                        break;
                    case MonsterType.TongKuangShiBig:
                        dropDict[DropItemType.TongKuangShiFragment] = 4;
                        break;
                    case MonsterType.ZiJingShi:
                        dropDict[DropItemType.ZiJingShiFragment] = 1;
                        break;
                    case MonsterType.ZiJingShiGolden:
                        dropDict[DropItemType.ZiJingShiFragment] = 10;
                        dropDict[DropItemType.YingQian] = 10;
                        break;
                    case MonsterType.ZiJingShiBig:
                        dropDict[DropItemType.ZiJingShiFragment] = 4;
                        break;
                    case MonsterType.YueJingShi:
                        dropDict[DropItemType.YueJingShiFragment] = 1;
                        break;
                    case MonsterType.YueJingShiGolden:
                        dropDict[DropItemType.YueJingShiFragment] = 10;
                        dropDict[DropItemType.YingQian] = 10;
                        break;
                    case MonsterType.YueJingShiBig:
                        dropDict[DropItemType.YueJingShiFragment] = 4;
                        break;
                }
            
        }
    }
}