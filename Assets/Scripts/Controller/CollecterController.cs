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

    public enum CollectorRouteMovePhase
    {
        None,
        EnterRouteForward,
        MoveToResource,
        MoveToRouteTail,
        ReturnAlongRoute,
        MoveToDepot
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
        private CollectorRouteMovePhase routeMovePhase;
        private FactoryController currentTarget;
        private bool hasMonsterNearby;
        private readonly List<Vector2> routeWaypoints = new();
        private int routeWaypointIndex = -1;
        private const float RouteArriveDistance = 0.35f;

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
        private int pendingPickupCount;

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
        private float nextMonsterCheckTime;
        private const float MonsterCheckInterval = 0.2f;
        private readonly Collider2D[] monsterDetectResults = new Collider2D[32];
        private int lastLayerBaseOrder = int.MinValue;
        private int lastWeaponOrderOffset = int.MinValue;

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

            if (ShouldReturnToDepot() && !IsInDepotWorkflow())
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
            currentTarget = null;
            ConfigureRoute();
            ResetRouteMovement();
            if (agent != null)
            {
                agent.Stop();
            }
            currentState = CollectorState.Idle;

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
            pendingPickupCount = 0;
            if (collectorInfo != null)
            {
                collectorInfo.Bind(this);
                collectorInfo.UpdateFill(1f);
            }
            RefreshCarryInfo();
            if (weaponController != null)
            {
                weaponController.warehouseCategoryType = structure.warehouseCategory.warehouseCategoryType;
                weaponController.playMonsterHitSfx = false;
            }
            EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerValueInfo);
        }

        private void ConfigureRoute()
        {
            routeWaypoints.Clear();
            routeWaypointIndex = -1;
            routeMovePhase = CollectorRouteMovePhase.None;

            if (depot == null || collectorData == null)
            {
                return;
            }

            if (GameController.Instance == null ||
                depot == null ||
                !GameController.Instance.TryBuildCollectorRoute(
                    depot.categoryType,
                    collectorData.monsterType,
                    out var waypoints))
            {
                return;
            }

            routeWaypoints.AddRange(waypoints);
        }

        private void ResetRouteMovement()
        {
            routeMovePhase = CollectorRouteMovePhase.None;
            routeWaypointIndex = -1;
        }

        private bool HasRouteWaypoints()
        {
            return routeWaypoints != null && routeWaypoints.Count > 0;
        }

        private bool HasReachedRoutePoint(Vector2 target)
        {
            float distance = Vector2.Distance(transform.position, target);
            if (distance <= RouteArriveDistance)
            {
                return true;
            }

            if (agent == null)
            {
                return false;
            }

            if (agent.pathPending)
            {
                return false;
            }

            if (distance <= 0.8f && (!agent.hasPath || agent.remainingDistance <= 0.15f))
            {
                return true;
            }

            return false;
        }

        private void SetAgentDestination(Vector2 target)
        {
            if (agent == null)
            {
                return;
            }

            agent.SetDestination(target);
        }

        private void SetDepotDestination()
        {
            Transform depotTarget = GetDepotTargetTransform();
            if (depotTarget == null)
            {
                return;
            }

            SetAgentDestination(depotTarget.position);
        }

        private void BeginMoveToResource()
        {
            if (currentTarget == null)
            {
                ResetRouteMovement();
                return;
            }

            if (!HasRouteWaypoints())
            {
                routeMovePhase = CollectorRouteMovePhase.MoveToResource;
                routeWaypointIndex = -1;
                SetAgentDestination(currentTarget.transform.position);
                return;
            }

            routeMovePhase = CollectorRouteMovePhase.EnterRouteForward;
            routeWaypointIndex = 0;
            SetAgentDestination(routeWaypoints[routeWaypointIndex]);
        }

        private bool UpdateGoToResourceRoute()
        {
            if (routeMovePhase != CollectorRouteMovePhase.EnterRouteForward)
            {
                return false;
            }

            if (!HasRouteWaypoints() || routeWaypointIndex < 0 || routeWaypointIndex >= routeWaypoints.Count)
            {
                routeMovePhase = CollectorRouteMovePhase.MoveToResource;
                routeWaypointIndex = -1;
                if (currentTarget != null)
                {
                    SetAgentDestination(currentTarget.transform.position);
                }
                return false;
            }

            Vector2 currentWaypoint = routeWaypoints[routeWaypointIndex];
            if (HasReachedRoutePoint(currentWaypoint))
            {
                if (routeWaypointIndex < routeWaypoints.Count - 1)
                {
                    routeWaypointIndex++;
                    SetAgentDestination(routeWaypoints[routeWaypointIndex]);
                    return true;
                }

                // 到达路线终点后，直接切回原有寻怪/战斗逻辑，
                // 避免继续强依赖 FactoryController 中心点导致停在终点不动。
                ResetRouteMovement();
                ChangeState(CollectorState.Fight);
                return true;
            }

            if (agent != null && !agent.hasPath)
            {
                SetAgentDestination(currentWaypoint);
            }

            return true;
        }

        private void BeginReturnToDepot()
        {
            if (GetDepotTargetTransform() == null)
            {
                ResetRouteMovement();
                return;
            }

            if (!HasRouteWaypoints())
            {
                routeMovePhase = CollectorRouteMovePhase.MoveToDepot;
                routeWaypointIndex = -1;
                SetDepotDestination();
                return;
            }

            int tailIndex = routeWaypoints.Count - 1;
            Vector2 tailPoint = routeWaypoints[tailIndex];
            if (HasReachedRoutePoint(tailPoint))
            {
                if (tailIndex <= 0)
                {
                    routeMovePhase = CollectorRouteMovePhase.MoveToDepot;
                    routeWaypointIndex = -1;
                    SetDepotDestination();
                    return;
                }

                routeMovePhase = CollectorRouteMovePhase.ReturnAlongRoute;
                routeWaypointIndex = tailIndex - 1;
                SetAgentDestination(routeWaypoints[routeWaypointIndex]);
                return;
            }

            routeMovePhase = CollectorRouteMovePhase.MoveToRouteTail;
            routeWaypointIndex = tailIndex;
            SetAgentDestination(tailPoint);
        }

        private bool UpdateReturnToDepotRoute()
        {
            switch (routeMovePhase)
            {
                case CollectorRouteMovePhase.MoveToRouteTail:
                    if (!HasRouteWaypoints() || routeWaypointIndex < 0 || routeWaypointIndex >= routeWaypoints.Count)
                    {
                        routeMovePhase = CollectorRouteMovePhase.MoveToDepot;
                        routeWaypointIndex = -1;
                        SetDepotDestination();
                        return false;
                    }

                    Vector2 tailPoint = routeWaypoints[routeWaypointIndex];
                    if (HasReachedRoutePoint(tailPoint))
                    {
                        if (routeWaypointIndex <= 0)
                        {
                            routeMovePhase = CollectorRouteMovePhase.MoveToDepot;
                            routeWaypointIndex = -1;
                            SetDepotDestination();
                            return false;
                        }

                        routeMovePhase = CollectorRouteMovePhase.ReturnAlongRoute;
                        routeWaypointIndex--;
                        SetAgentDestination(routeWaypoints[routeWaypointIndex]);
                        return true;
                    }

                    if (agent != null && !agent.hasPath)
                    {
                        SetAgentDestination(tailPoint);
                    }

                    return true;

                case CollectorRouteMovePhase.ReturnAlongRoute:
                    if (routeWaypointIndex < 0 || routeWaypointIndex >= routeWaypoints.Count)
                    {
                        routeMovePhase = CollectorRouteMovePhase.MoveToDepot;
                        routeWaypointIndex = -1;
                        SetDepotDestination();
                        return false;
                    }

                    Vector2 currentWaypoint = routeWaypoints[routeWaypointIndex];
                    if (HasReachedRoutePoint(currentWaypoint))
                    {
                        routeWaypointIndex--;
                        if (routeWaypointIndex >= 0)
                        {
                            SetAgentDestination(routeWaypoints[routeWaypointIndex]);
                            return true;
                        }

                        routeMovePhase = CollectorRouteMovePhase.MoveToDepot;
                        routeWaypointIndex = -1;
                        SetDepotDestination();
                        return false;
                    }

                    if (agent != null && !agent.hasPath)
                    {
                        SetAgentDestination(currentWaypoint);
                    }

                    return true;
            }

            return false;
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
                    ResetRouteMovement();
                    break;

                case CollectorState.FindResource:
                    ResetRouteMovement();
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
                    BeginMoveToResource();
                    break;

                case CollectorState.ReturnToDepot:
                    BeginReturnToDepot();
                    break;

                case CollectorState.Unloading:
                    ResetRouteMovement();
                    if (agent != null)
                    {
                        agent.Stop();
                    }
                    nextUnloadTime = Time.time;
                    break;

                case CollectorState.WaitDepotSpace:
                    ResetRouteMovement();
                    if (agent != null)
                    {
                        agent.Stop();
                    }
                    break;

                case CollectorState.Wait:
                    ResetRouteMovement();
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
                    if (ShouldReturnToDepot())
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

                    if (!HasAliveMonsters(currentTarget))
                    {
                        ChangeState(CollectorState.Wait);
                        break;
                    }

                    if (UpdateGoToResourceRoute())
                    {
                        break;
                    }

                    float targetDistSqr = ((Vector2)(currentTarget.transform.position - transform.position)).sqrMagnitude;
                    float targetEnterFightDistance = detectRadius * 0.8f;
                    if (targetDistSqr <= targetEnterFightDistance * targetEnterFightDistance ||
                        (agent != null && agent.hasPath && agent.remainingDistance < 0.1f))
                    {
                        ChangeState(CollectorState.Fight);
                    }
                    else if (agent != null && !agent.hasPath)
                    {
                        routeMovePhase = CollectorRouteMovePhase.MoveToResource;
                        SetAgentDestination(currentTarget.transform.position);
                    }
                    break;

                case CollectorState.Fight:
                    DoFight();
                    break;

                case CollectorState.ReturnToDepot:
                    if (GetDepotTargetTransform() == null)
                    {
                        ChangeState(CollectorState.Idle);
                        break;
                    }

                    if (UpdateReturnToDepotRoute())
                    {
                        break;
                    }

                    Transform depotTarget = GetDepotTargetTransform();
                    float depotDistSqr = depotTarget == null
                        ? float.MaxValue
                        : ((Vector2)(depotTarget.position - transform.position)).sqrMagnitude;
                    float depotArriveThreshold = Mathf.Max(0.2f, depotArriveDistance);
                    if (depotDistSqr <= depotArriveThreshold * depotArriveThreshold)
                    {
                        ChangeState(CollectorState.Unloading);
                    }
                    else if (agent != null && !agent.hasPath)
                    {
                        routeMovePhase = CollectorRouteMovePhase.MoveToDepot;
                        SetDepotDestination();
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

                case CollectorState.Wait:
                    if (ShouldReturnToDepot())
                    {
                        ChangeState(CollectorState.ReturnToDepot);
                        break;
                    }

                    if (TryGetActiveFactoryController(out var activeFactory))
                    {
                        currentTarget = activeFactory;
                        if (TryGetNearestAliveMonster(activeFactory, out var nearestMonster, out var nearestMonsterDistSqr) &&
                            nearestMonsterDistSqr <= detectRadius * detectRadius)
                        {
                            ChangeState(CollectorState.Fight);
                        }
                        else
                        {
                            ChangeState(CollectorState.GoToResource);
                        }
                        break;
                    }

                    break;
            }
        }

        #endregion

        #region Combat

        public void CheckMonster()
        {
            if (Time.time < nextMonsterCheckTime)
            {
                return;
            }

            nextMonsterCheckTime = Time.time + MonsterCheckInterval;

            int hitCount = monsterLayer.value != 0
                ? Physics2D.OverlapCircleNonAlloc(transform.position, detectRadius, monsterDetectResults, monsterLayer)
                : Physics2D.OverlapCircleNonAlloc(transform.position, detectRadius, monsterDetectResults);

            hasMonsterNearby = false;
            for (int i = 0; i < hitCount; i++)
            {
                var hit = monsterDetectResults[i];
                if (hit != null && hit.CompareTag("Monster"))
                {
                    hasMonsterNearby = true;
                    break;
                }
            }

            for (int i = 0; i < hitCount; i++)
            {
                monsterDetectResults[i] = null;
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
            if (!TryGetFactoryController(out var targetFactory))
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
                if (!IsAliveFactoryMonster(monster)) continue;

                float dist = ((Vector2)(monster.transform.position - transform.position)).sqrMagnitude;

                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = monster.transform;
                }
            }

            // 涓囦竴鍏ㄩ儴 monster 閮借娓呮帀
            if (nearest == null)
            {
                ChangeState(CollectorState.Wait);
                return;
            }
            SetFacingByDirection(nearest.position.x - transform.position.x);

            // 鏍规嵁涓庢渶杩戞€墿鐨勮窛绂诲喅锟?闈犺繎"杩樻槸"鍘熷湴鎸ユ锟?
            float stopDistance = Mathf.Max(attackStopDistance, 0.8f);
            float stopDistanceSqr = stopDistance * stopDistance;

            if (minDist > stopDistanceSqr)
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
            ReleasePendingPickupSlot();

            if (isDead || Time.time < ignorePickupUntil)
            {
                return;
            }

            if (inventory.GetTotalCount() >= inventory.max)
            {
                RefreshCarryInfo();
                ChangeState(CollectorState.ReturnToDepot);
                return;
            }

            inventory.Add(itemType);
            RefreshCarryInfo();
            if (GetReservedCarryNum() >= maxCarryNum)
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
                if (dist > collectRadius)
                {
                    continue;
                }

                if (!HasCarryCapacityForOne())
                {
                    ChangeState(CollectorState.ReturnToDepot);
                    return;
                }

                ReservePendingPickupSlot();
                item.StartAttract(this.transform, receiveTransform, ReleasePendingPickupSlot);
                if (!item.isTaken)
                {
                    ReleasePendingPickupSlot();
                    continue;
                }

                if (GetReservedCarryNum() >= maxCarryNum)
                {
                    ChangeState(CollectorState.ReturnToDepot);
                    return;
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
            pendingPickupCount = 0;
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
            int weaponOffset = 1;

            if (weaponRoot != null && weapon != null && weapon.activeSelf)
            {
                float z = weaponRoot.localEulerAngles.z;
                if (z > 180f) z -= 360f;
                weaponOffset = Mathf.Abs(z) <= 90f ? 1 : -1;
            }

            if (baseOrder == lastLayerBaseOrder && weaponOffset == lastWeaponOrderOffset)
            {
                return;
            }

            lastLayerBaseOrder = baseOrder;
            lastWeaponOrderOffset = weaponOffset;

            if (canvas != null)
            {
                canvas.sortingOrder = baseOrder + 1;
            }

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
                weaponRenderer.sortingOrder = baseOrder + weaponOffset;
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

        private bool TryGetActiveFactoryController(out FactoryController factory)
        {
            factory = null;
            if (!TryGetFactoryController(out var candidate))
            {
                return false;
            }

            if (!HasAliveMonsters(candidate))
            {
                return false;
            }

            factory = candidate;
            return true;
        }

        private bool HasAliveMonsters(FactoryController factory)
        {
            if (factory == null || factory.monsterList == null)
            {
                return false;
            }

            for (int i = 0; i < factory.monsterList.Count; i++)
            {
                if (IsAliveFactoryMonster(factory.monsterList[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private bool TryGetNearestAliveMonster(FactoryController factory, out Transform nearest, out float minDist)
        {
            nearest = null;
            minDist = float.MaxValue;

            if (factory == null || factory.monsterList == null)
            {
                return false;
            }

            for (int i = 0; i < factory.monsterList.Count; i++)
            {
                var monster = factory.monsterList[i];
                if (!IsAliveFactoryMonster(monster))
                {
                    continue;
                }

                float dist = ((Vector2)(monster.transform.position - transform.position)).sqrMagnitude;
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = monster.transform;
                }
            }

            return nearest != null;
        }

        private bool IsAliveFactoryMonster(GameObject monster)
        {
            if (monster == null || !monster.activeInHierarchy)
            {
                return false;
            }

            var monsterController = monster.GetComponent<MonsterController>();
            if (monsterController == null)
            {
                return false;
            }

            return monsterController.currentHp > 0f;
        }

        private bool IsInDepotWorkflow()
        {
            return currentState == CollectorState.ReturnToDepot
                   || currentState == CollectorState.Unloading
                   || currentState == CollectorState.WaitDepotSpace;
        }

        private bool ShouldReturnToDepot()
        {
            return GetReservedCarryNum() >= maxCarryNum;
        }

        private Transform GetDepotTargetTransform()
        {
            if (depot == null)
            {
                return null;
            }

            if (depot.collectorTransform != null)
            {
                return depot.collectorTransform;
            }

            if (depot.bornTransform != null)
            {
                return depot.bornTransform;
            }

            return depot.transform;
        }

        private int GetReservedCarryNum()
        {
            return inventory.GetTotalCount() + pendingPickupCount;
        }

        private bool HasCarryCapacityForOne()
        {
            return GetReservedCarryNum() < maxCarryNum;
        }

        private void ReservePendingPickupSlot()
        {
            pendingPickupCount++;
        }

        private void ReleasePendingPickupSlot()
        {
            pendingPickupCount = Mathf.Max(0, pendingPickupCount - 1);
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
