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
        public float detectRadius = 6f;      
        public float collectRadius = 5f;      
        public float collectorPickupDelay = 0.8f; 
        public float collectScanInterval = 0.08f; 
        public float unloadInterval = 0.12f; 
        public int unloadPerBatch = 1;
        public float depotArriveDistance = 0.8f;
        public LayerMask monsterLayer;      
        public float waitTime = 2f;     
        public float attackStopDistance = 1.2f;
        public float fightRepathInterval = 0.2f; 
        public float fightTargetRefreshInterval = 0.12f; 
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
        public Collector collectorData;
        public MonsterType monsterType;
        public DropItemType targetType;  
        private CollectorState currentState;
        private CollectorRouteMovePhase routeMovePhase;
        private FactoryController currentTarget;
        private bool hasMonsterNearby;
        private readonly List<Vector2> routeWaypoints = new();
        private int routeWaypointIndex = -1;
        private const float RouteArriveDistance = 0.35f;
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
        private float nextCollectScanTime;
        private float nextFightRepathTime;
        private float nextFightTargetRefreshTime;
        private Vector2 lastFightDestination;
        private bool hasFightDestination;
        private Transform cachedFightTarget;
        private int pendingPickupCount;
        private float lastDamageTime = -999f;
        private bool isRegenerating = false;
        private Coroutine regenCoroutine;
        private const float RegenDelay = 3f;
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
            TryDoCollect();
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
            ResetFightChaseCache();
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
            Vector2 offset = (Vector2)transform.position - target;
            float distanceSqr = offset.sqrMagnitude;
            if (distanceSqr <= RouteArriveDistance * RouteArriveDistance)
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
            if (distanceSqr <= 0.64f && (!agent.hasPath || agent.remainingDistance <= 0.15f))
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
        #region State Machie
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
                    ResetFightChaseCache();
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
                case CollectorState.Fight:
                    ResetFightChaseCache();
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
            if (list == null || list.Count == 0)
            {
                ChangeState(CollectorState.Wait);
                return;
            }
            if (!TryGetFightTarget(targetFactory, out var nearest, out var minDist))
            {
                ChangeState(CollectorState.Wait);
                return;
            }
            SetFacingByDirection(nearest.position.x - transform.position.x);
            float stopDistance = Mathf.Max(attackStopDistance, 0.8f);
            float stopDistanceSqr = stopDistance * stopDistance;
            if (minDist > stopDistanceSqr)
            {
                if (agent != null)
                {
                    if (Time.time >= nextFightRepathTime)
                    {
                        Vector2 destination = nearest.position;
                        bool needRepath = !agent.hasPath
                                          || !hasFightDestination
                                          || (destination - lastFightDestination).sqrMagnitude > 0.04f;
                        if (needRepath)
                        {
                            agent.SetDestination(destination);
                            lastFightDestination = destination;
                            hasFightDestination = true;
                        }
                        nextFightRepathTime = Time.time + Mathf.Max(0.05f, fightRepathInterval);
                    }
                }
            }
            else
            {
                if (agent != null)
                {
                    if (agent.hasPath)
                    {
                        agent.Stop();
                    }
                }
                ResetFightChaseCache();
            }
        }
        private bool TryGetFightTarget(FactoryController factory, out Transform nearest, out float minDist)
        {
            nearest = null;
            minDist = float.MaxValue;
            if (factory == null || factory.monsterList == null)
            {
                ResetFightChaseCache();
                return false;
            }
            bool needRefresh = Time.time >= nextFightTargetRefreshTime
                               || cachedFightTarget == null
                               || !IsAliveFactoryMonster(cachedFightTarget.gameObject);
            if (needRefresh)
            {
                if (!TryGetNearestAliveMonster(factory, out cachedFightTarget, out minDist))
                {
                    cachedFightTarget = null;
                    nextFightTargetRefreshTime = Time.time + Mathf.Max(0.05f, fightTargetRefreshInterval);
                    return false;
                }
                nextFightTargetRefreshTime = Time.time + Mathf.Max(0.05f, fightTargetRefreshInterval);
            }
            else
            {
                minDist = ((Vector2)(cachedFightTarget.position - transform.position)).sqrMagnitude;
            }
            nearest = cachedFightTarget;
            return nearest != null;
        }
        private void ResetFightChaseCache()
        {
            hasFightDestination = false;
            nextFightRepathTime = 0f;
            cachedFightTarget = null;
            nextFightTargetRefreshTime = 0f;
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
        private void TryDoCollect()
        {
            if (Time.time < nextCollectScanTime)
            {
                return;
            }
            nextCollectScanTime = Time.time + Mathf.Max(0.02f, collectScanInterval);
            DoCollect();
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
            var scenePickup = ScenePickupController.Instance;
            if (scenePickup == null || scenePickup.materials == null || scenePickup.materials.Count == 0)
            {
                return;
            }
            if (playerTransform == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    playerTransform = player.transform;
                }
            }
            Vector2 selfPosition = transform.position;
            float collectRadiusSqr = collectRadius * collectRadius;
            bool hasPlayer = playerTransform != null;
            Vector2 playerPosition = hasPlayer ? (Vector2)playerTransform.position : Vector2.zero;
            var materials = scenePickup.materials;
            for (int i = materials.Count - 1; i >= 0; i--)
            {
                var item = materials[i];
                if (item == null) continue;
                if (!item.gameObject.activeInHierarchy) continue;
                var drop = item as DropController;
                if (drop == null) continue;
                if (drop.itemType != targetType) continue;
                if (item.isTaken) continue;
                if (!item.canPickup) continue;
                if (!drop.CanBePickedByCollector(collectorPickupDelay)) continue;
                Vector2 itemPosition = item.transform.position;
                if (hasPlayer && (playerPosition - itemPosition).sqrMagnitude <= collectRadiusSqr) continue;
                if ((selfPosition - itemPosition).sqrMagnitude > collectRadiusSqr)
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
            ResetFightChaseCache();
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
            if (!monster.TryGetComponent(out MonsterController monsterController))
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
