using System.Collections;
using System.Collections.Generic;
using Controller.Pickups;
using Module;
using Module.Data;
using PolyNav;
using Spine.Unity;
using UnityEngine;
using Utils;

namespace Controller
{
    public enum CollectorState
    {
        Idle,
        FindResource,
        GoToResource,
        Fight,
        ReturnToDepot,
        Unloading,
        WaitDepotSpace,
        Wait
    }

    public class CollectorController : MonoBehaviour
    {
        #region Fields

        // 閰嶇疆鍙傛暟
        public float detectRadius = 6f;       // monster detect radius
        public float collectRadius = 5f;      // 鐗╁搧鍚稿紩鍗婂緞
        public float collectorPickupDelay = 0.8f; // drop spawn delay before collector can pick
        public float unloadInterval = 0.12f; // interval per unload batch
        public int unloadPerBatch = 1;
        public float depotArriveDistance = 0.8f;
        public LayerMask monsterLayer;        // monster layer mask
        public float waitTime = 2f;           // 鍖哄煙鏃犳€墿鏃剁殑绛夊緟鏃堕棿

        // 缁勪欢寮曠敤
        public float attackStopDistance = 1.2f;
        public PolyNavAgent agent;
        public GameObject weapon;
        public Transform weaponRoot;
        private float weaponSpinSpeed = 540f;
        public CollectorInventory inventory = new CollectorInventory();
        public LingChuGeController depot;
        public Transform receiveTransform;
        public SkeletonAnimation skeletonAnimation;
        public MeshRenderer meshRenderer;
        public SpriteRenderer shadowRenderer;
        public SpriteRenderer weaponRenderer;
        public CollectorInfo collectorInfo;

        // 鏁版嵁
        public Collector collectorData;
        public MonsterType monsterType;
        public DropItemType targetType;       // 閲囬泦鐩爣绫诲瀷

        // 鐘讹拷?
        private CollectorState currentState;
        private FactoryController currentTarget;
        private bool hasMonsterNearby;

        // 灞烇拷?
        public float currentHp;
        public float maxHp;
        public int currentCarryNum;
        public int maxCarryNum;
        private bool isDead;
        private bool invincible;
        private const float InvincibleTime = 0.2f;
        private Transform playerTransform;
        private float ignorePickupUntil;
        private float nextUnloadTime;

        // 鍥炶鐩稿叧
        private float lastDamageTime = -999f;
        private bool isRegenerating = false;
        private Coroutine regenCoroutine;
        private const float RegenDelay = 3f;

        // 鍔ㄧ敾甯搁噺
        private const string AnimIdle = "idle";
        private const string AnimWalk = "walk";
        private const string AnimAttack = "gongji";
        private const string AnimWalkAttack = "zoulugongji";
        private Vector3 lastWorldPos;
        private Vector3 baseSkeletonScale = Vector3.one;
        private bool hasBaseSkeletonScale;

        public Canvas canvas;

        public WeaponController weaponController;

        #endregion

        #region Unity Lifecycle
        void Awake()
        {
            if(canvas == null)
            {
                canvas = GetComponentInChildren<Canvas>();
            }
        }

        private void Start()
        {
            agent = GetComponent<PolyNavAgent>();

            if (agent != null)
            {
                var mapObj = GameObject.FindWithTag("Map");
                if (mapObj != null)
                {
                    agent.map = mapObj.transform.GetComponent<PolyNavMap>();
                }
            }

            if (monsterLayer.value == 0)
            {
                int monsterLayerId = LayerMask.NameToLayer("Monster");
                if (monsterLayerId >= 0)
                {
                    monsterLayer = 1 << monsterLayerId;
                }
            }

            if (inventory == null)
            {
                inventory = new CollectorInventory();
            }
            if (receiveTransform == null)
            {
                receiveTransform = transform;
            }

            if (weaponRoot == null)
            {
                if (weapon != null && weapon.transform.parent != null)
                {
                    weaponRoot = weapon.transform.parent;
                }
                else
                {
                    var root = transform.Find("Character/weaponroot");
                    if (root != null)
                    {
                        weaponRoot = root;
                    }
                }
            }

            if (collectorInfo != null && maxHp > 0f)
            {
                collectorInfo.Bind(this);
            }
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }

            CacheSkeletonScale();
            RefreshCarryInfo();
            lastWorldPos = transform.position;
            ChangeState(CollectorState.Idle);
        }

        private void Update()
        {
            CheckMonster();
            UpdateWeaponSpin();
            UpdateFacing();
            SetLayer();
            UpdateAnimation();

            if (inventory.IsFull() && !IsInDepotWorkflow())
            {
                ChangeState(CollectorState.ReturnToDepot);
                return;
            }
            UpdateState();
            DoCollect();
        }

        private void CacheSkeletonScale()
        {
            if (skeletonAnimation == null || hasBaseSkeletonScale)
            {
                return;
            }

            baseSkeletonScale = skeletonAnimation.transform.localScale;
            hasBaseSkeletonScale = true;
        }

        private void UpdateFacing()
        {
            CacheSkeletonScale();
            if (skeletonAnimation == null || !hasBaseSkeletonScale)
            {
                return;
            }

            float dx = transform.position.x - lastWorldPos.x;
            if (Mathf.Abs(dx) > 0.0005f)
            {
                SetFacingByDirection(dx);
            }

            lastWorldPos = transform.position;
        }

        private void SetFacingByDirection(float dirX)
        {
            if (skeletonAnimation == null || !hasBaseSkeletonScale || Mathf.Abs(dirX) <= 0.0001f)
            {
                return;
            }

            var scale = baseSkeletonScale;
            scale.x = Mathf.Abs(baseSkeletonScale.x) * (dirX >= 0 ? 1f : -1f);
            skeletonAnimation.transform.localScale = scale;
        }

        public void RefreshCarryInfo()
        {
            if (inventory == null)
            {
                currentCarryNum = 0;
            }
            else
            {
                currentCarryNum = inventory.GetTotalCount();
                maxCarryNum = inventory.max;
            }
            collectorInfo?.UpdateTxt();
        }

        private void UpdateWeaponSpin()
        {
            if (weaponRoot == null)
            {
                return;
            }

            if (weapon != null)
            {
                bool shouldActive = currentState == CollectorState.Fight;
                if (weapon.activeSelf != shouldActive)
                {
                    weapon.SetActive(shouldActive);
                }
            }

            if (weapon != null && weapon.activeSelf)
            {
                weaponRoot.Rotate(0f, 0f, -weaponSpinSpeed * Time.deltaTime);
            }
            else
            {
                weaponRoot.localRotation = Quaternion.identity;
            }
        }

        #endregion

        #region Initialization

        public void Init(Collector c, LingChuGeController structure)
        {
            collectorData = c;
            (MonsterType, DropItemType) v = Extensions.ExchangeFamilyType(collectorData.monsterType);
            monsterType = v.Item1;
            targetType = v.Item2;
            depot = structure;

            inventory.max = (int)c.bagCapacity;
            if (agent != null)
            {
                agent.maxSpeed = 4;
            }
            maxHp = collectorData.maxHp;
            var cardprogress = PlayerDataModule.Instance.data.cardUpProgressesList.Find(x => x.developType == CardDevelopType.UpgradeCharacterWithXuanCaiTuHp);
            if (cardprogress != null)
            {
                maxHp += cardprogress.level * 30;
            }
            currentHp = maxHp;

            currentCarryNum = 0;
            maxCarryNum = (int)c.bagCapacity;
            if (collectorInfo != null)
            {
                collectorInfo.Bind(this);
                collectorInfo.UpdateFill(1f);
            }
            RefreshCarryInfo();
            weaponController.warehouseCategoryType = structure.warehouseCategory.warehouseCategoryType;
            EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerValueInfo);
        }

        #endregion

        #region State Machine

        private void ChangeState(CollectorState newState)
        {
            if (currentState == newState) return;

            ExitState(currentState);
            currentState = newState;
            EnterState(newState);
        }

        private void ExitState(CollectorState state)
        {
            switch (state)
            {
                case CollectorState.Wait:
                    CancelInvoke(nameof(BackToIdle));
                    break;

                case CollectorState.Fight:
                    if (agent != null)
                    {
                        agent.Stop();
                    }
                    break;
            }
        }

        private void EnterState(CollectorState state)
        {
            switch (state)
            {
                case CollectorState.Idle:
                    break;

                case CollectorState.FindResource:
                    if (TryGetFactoryController(out var targetFactory))
                    {
                        currentTarget = targetFactory;
                        ChangeState(CollectorState.GoToResource);
                    }
                    else
                    {
                        currentTarget = null;
                        ChangeState(CollectorState.Wait);
                    }
                    break;

                case CollectorState.GoToResource:
                    if (agent != null && currentTarget != null)
                    {
                        agent.SetDestination(currentTarget.transform.position);
                    }
                    break;

                case CollectorState.ReturnToDepot:
                    if (agent != null && depot != null && depot.collectorTransform != null)
                    {
                        agent.SetDestination(depot.collectorTransform.position);
                    }
                    break;

                case CollectorState.Unloading:
                    if (agent != null)
                    {
                        agent.Stop();
                    }
                    nextUnloadTime = Time.time;
                    break;

                case CollectorState.WaitDepotSpace:
                    if (agent != null)
                    {
                        agent.Stop();
                    }
                    break;

                case CollectorState.Wait:
                    Invoke(nameof(BackToIdle), waitTime);
                    break;
            }
        }

        private void BackToIdle()
        {
            ChangeState(CollectorState.Idle);
        }

        private void UpdateState()
        {
            switch (currentState)
            {
                case CollectorState.Idle:
                    if (inventory.IsFull())
                    {
                        ChangeState(CollectorState.ReturnToDepot);
                    }
                    else
                    {
                        ChangeState(CollectorState.FindResource);
                    }

                    break;

                case CollectorState.GoToResource:
                    if (currentTarget == null)
                    {
                        ChangeState(CollectorState.FindResource);
                        break;
                    }

                    float targetDist = Vector2.Distance(transform.position, currentTarget.transform.position);
                    if (targetDist <= detectRadius * 0.8f || (agent != null && agent.hasPath && agent.remainingDistance < 0.1f))
                    {
                        ChangeState(CollectorState.Fight);
                    }
                    else if (agent != null && !agent.hasPath)
                    {
                        agent.SetDestination(currentTarget.transform.position);
                    }
                    break;

                case CollectorState.Fight:
                    DoFight();
                    break;

                case CollectorState.ReturnToDepot:
                    if (depot == null || depot.collectorTransform == null)
                    {
                        ChangeState(CollectorState.Idle);
                        break;
                    }

                    float depotDist = Vector2.Distance(transform.position, depot.collectorTransform.position);
                    if (depotDist <= Mathf.Max(0.2f, depotArriveDistance))
                    {
                        ChangeState(CollectorState.Unloading);
                    }
                    else if (agent != null && !agent.hasPath)
                    {
                        agent.SetDestination(depot.collectorTransform.position);
                    }
                    break;

                case CollectorState.Unloading:
                    if (depot == null)
                    {
                        ChangeState(CollectorState.Idle);
                        break;
                    }

                    if (inventory.IsEmpty())
                    {
                        ChangeState(CollectorState.Idle);
                        break;
                    }

                    if (!depot.HasFreeCapacity())
                    {
                        ChangeState(CollectorState.WaitDepotSpace);
                        break;
                    }

                    if (Time.time >= nextUnloadTime)
                    {
                        int unloadCount = Mathf.Max(1, unloadPerBatch);
                        int moved = depot.Store(this, inventory, unloadCount);
                        nextUnloadTime = Time.time + Mathf.Max(0.01f, unloadInterval);

                        if (moved <= 0)
                        {
                            ChangeState(CollectorState.WaitDepotSpace);
                        }
                    }
                    break;

                case CollectorState.WaitDepotSpace:
                    if (depot == null)
                    {
                        ChangeState(CollectorState.Idle);
                        break;
                    }

                    if (inventory.IsEmpty())
                    {
                        ChangeState(CollectorState.Idle);
                        break;
                    }

                    if (depot.HasFreeCapacity())
                    {
                        ChangeState(CollectorState.Unloading);
                    }
                    break;
            }
        }

        #endregion

        #region Combat

        public void CheckMonster()
        {
            Collider2D[] hits = monsterLayer.value != 0
                ? Physics2D.OverlapCircleAll(transform.position, detectRadius, monsterLayer)
                : Physics2D.OverlapCircleAll(transform.position, detectRadius);

            hasMonsterNearby = false;
            foreach (var hit in hits)
            {
                if (hit != null && hit.CompareTag("Monster"))
                {
                    hasMonsterNearby = true;
                    break;
                }
            }

            if (!hasMonsterNearby)
            {
                // 鑷姩鍥炶妫€锟?
                if (currentHp < maxHp && !isRegenerating)
                {
                    if (Time.time - lastDamageTime >= RegenDelay)
                    {
                        regenCoroutine = StartCoroutine(RegenerateHealth());
                    }
                }
            }
        }

        private void DoFight()
        {
            if (!TryGetFactoryController(out var targetFactory) || targetFactory.monsterList == null)
            {
                ChangeState(CollectorState.Wait);
                return;
            }

            var list = targetFactory.monsterList;

            if (list.Count == 0)
            {
                // 娌℃€墿浜嗭紝杩涘叆绛夊緟鐘讹拷?
                ChangeState(CollectorState.Wait);
                return;
            }

            // 鎵惧嚭鏈€杩戠殑鎬墿
            float minDist = float.MaxValue;
            Transform nearest = null;

            foreach (var monster in list)
            {
                if (monster == null) continue;

                float dist = Vector2.Distance(transform.position, monster.transform.position);

                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = monster.transform;
                }
            }

            // 涓囦竴鍏ㄩ儴 monster 閮借娓呮帀
            if (nearest == null)
            {
                ChangeState(CollectorState.FindResource);
                return;
            }
            SetFacingByDirection(nearest.position.x - transform.position.x);

            // 鏍规嵁涓庢渶杩戞€墿鐨勮窛绂诲喅锟?闈犺繎"杩樻槸"鍘熷湴鎸ユ锟?
            float stopDistance = Mathf.Max(attackStopDistance, 0.8f);

            if (minDist > stopDistance)
            {
                // 杩樻病鍒版敾鍑昏窛绂伙紝缁х画寰€鎬墿浣嶇疆绉诲姩
                if (agent != null)
                {
                    agent.SetDestination(nearest.position);
                }
            }
            else
            {
                // 宸插埌鏀诲嚮璺濈闄勮繎锛屽仠涓嬭剼姝ワ紝璁╂鍣ㄨЕ鍙戝櫒鍘诲仛浼ゅ妫€锟?
                if (agent != null)
                {
                    agent.Stop();
                }
            }
        }

        #endregion

        #region Collection

        public void AddDropItem(DropItemType itemType)
        {
            if (isDead || Time.time < ignorePickupUntil)
            {
                return;
            }

            inventory.Add(itemType);
            RefreshCarryInfo();
            if (inventory.IsFull())
            {
                ChangeState(CollectorState.ReturnToDepot);
            }
        }

        private void DoCollect()
        {
            if (isDead || Time.time < ignorePickupUntil)
            {
                return;
            }
            if (IsInDepotWorkflow())
            {
                return;
            }

            if (inventory.IsFull())
            {
                ChangeState(CollectorState.ReturnToDepot);
                return;
            }
            if (receiveTransform == null)
            {
                return;
            }

            if (ScenePickupController.Instance == null)
            {
                return;
            }

            var materials = ScenePickupController.Instance.materials.ToArray();
            foreach (var item in materials)
            {
                if (item == null) continue;
                if (!item.gameObject.activeInHierarchy) continue;
                var drop = item as DropController;
                if (drop == null) continue;
                if (drop.itemType != targetType) continue;
                if (item.isTaken) continue;
                if (!item.canPickup) continue;
                if (!drop.CanBePickedByCollector(collectorPickupDelay)) continue;
                if (IsDropNearPlayer(item.transform)) continue;

                float dist = Vector2.Distance(transform.position, item.transform.position);
                if (dist <= collectRadius && !inventory.IsFull())
                {
                    item.StartAttract(this.transform, receiveTransform);
                }
            }
        }

        private bool IsDropNearPlayer(Transform dropTransform)
        {
            if (dropTransform == null)
            {
                return false;
            }

            if (playerTransform == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    playerTransform = player.transform;
                }
            }

            if (playerTransform == null)
            {
                return false;
            }

            return Vector2.Distance(playerTransform.position, dropTransform.position) <= collectRadius;
        }

        #endregion

        #region Health

        public void TakeDamage(float damage)
        {
            if (isDead || invincible)
            {
                return;
            }

            StartCoroutine(InvincibleFrame());
            lastDamageTime = Time.time;
            currentHp -= damage;
            currentHp = Mathf.Max(currentHp, 0f);

            if (collectorInfo != null)
            {
                collectorInfo.ShowHpInfo();
                collectorInfo.UpdateFill(currentHp / Mathf.Max(maxHp, 0.001f));
            }

            if (currentHp <= 0f)
            {
                DoDie();
            }
        }

        private void DoDie()
        {
            isDead = true;

            if (regenCoroutine != null)
            {
                StopCoroutine(regenCoroutine);
                regenCoroutine = null;
            }
            isRegenerating = false;

            inventory.Clear();
            RefreshCarryInfo();

            if (weapon != null)
            {
                weapon.SetActive(false);
            }

            if (agent != null)
            {
                agent.Stop();
            }

            if (depot != null && depot.collectorTransform != null)
            {
                Transform respawnPoint = depot.bornTransform != null ? depot.bornTransform : depot.collectorTransform;
                transform.position = respawnPoint.position;
            }

            lastWorldPos = transform.position;
            ignorePickupUntil = Time.time + 1f;
            currentHp = maxHp;
            if (collectorInfo != null)
            {
                collectorInfo.ShowHpInfo();
                collectorInfo.UpdateFill(1f);
            }

            ChangeState(CollectorState.Idle);
            isDead = false;
        }

        private IEnumerator InvincibleFrame()
        {
            invincible = true;
            yield return new WaitForSeconds(InvincibleTime);
            invincible = false;
        }

        private IEnumerator RegenerateHealth()
        {
            isRegenerating = true;

            while (currentHp < maxHp)
            {
                currentHp += 5 * Time.deltaTime;
                currentHp = Mathf.Min(currentHp, maxHp);
                if (collectorInfo != null)
                {
                    collectorInfo.UpdateFill(currentHp / maxHp);
                }
                yield return null;

                if (Time.time - lastDamageTime < RegenDelay)
                {
                    isRegenerating = false;
                    yield break;
                }
            }

            isRegenerating = false;
        }

        #endregion

        #region Utility

        public void SetLayer()
        {
            int baseOrder = 30000 - Mathf.RoundToInt(transform.position.y * 100f);
             canvas.sortingOrder =baseOrder + 1;
            if (meshRenderer != null)
            {
                meshRenderer.sortingOrder = baseOrder;
            }

            if (shadowRenderer != null)
            {
                shadowRenderer.sortingOrder = baseOrder - 1;
            }

            if (weaponRenderer != null)
            {
                int offset = 1;
                if (weaponRoot != null && weapon != null && weapon.activeSelf)
                {
                    float z = weaponRoot.localEulerAngles.z;
                    if (z > 180f) z -= 360f;
                    offset = Mathf.Abs(z) <= 90f ? 1 : -1;
                }
                weaponRenderer.sortingOrder = baseOrder + offset;
            }
        }

        private void UpdateAnimation()
        {
            if (skeletonAnimation == null || agent == null)
            {
                return;
            }

            var state = skeletonAnimation.AnimationState;
            var current = state.GetCurrent(0);

            bool moving = agent.hasPath && agent.remainingDistance > 1f;
            bool fighting = currentState == CollectorState.Fight;

            string anim = fighting
                ? (moving ? AnimWalkAttack : AnimAttack)
                : (moving ? AnimWalk : AnimIdle);

            if (current == null || current.Animation.Name != anim)
            {
                state.SetAnimation(0, anim, true);
            }
        }

        private bool TryGetFactoryController(out FactoryController factory)
        {
            factory = null;
            if (GameController.Instance == null || GameController.Instance.factoryControllers == null)
            {
                return false;
            }

            if (!GameController.Instance.factoryControllers.TryGetValue(monsterType, out factory))
            {
                return false;
            }

            return factory != null;
        }

        private bool IsInDepotWorkflow()
        {
            return currentState == CollectorState.ReturnToDepot
                   || currentState == CollectorState.Unloading
                   || currentState == CollectorState.WaitDepotSpace;
        }

        #endregion
    }

    public class CollectorInventory
    {
        public int max = 20;
        public Dictionary<DropItemType, int> dic = new();

        public bool IsFull()
        {
            int sum = 0;
            foreach (var v in dic.Values) sum += v;
            return sum >= max;
        }

        public int GetTotalCount()
        {
            int sum = 0;
            foreach (var v in dic.Values) sum += v;
            return sum;
        }

        public void Add(DropItemType t)
        {
            if (!dic.ContainsKey(t)) dic[t] = 0;
            dic[t]++;
        }

        public void Clear()
        {
            dic.Clear();
        }
        public void Remove(DropItemType t, int count)
        {
            if (!dic.ContainsKey(t)) return;

            dic[t] -= count;
            if (dic[t] <= 0)
                dic.Remove(t);
        }
        public bool IsEmpty()
        {
            return dic.Count == 0;
        }


    }
}


