using System.Collections;
using System.Collections.Generic;
using Controller.Pickups;
using Controller.Structure;
using Module.Data;
using PolyNav;
using Spine.Unity;
using UnityEngine;
using Utils;

namespace Controller
{
    public enum NpcState
    {
        None,
        QianWangGouMai,
        WaitGouMaiWanCheng,
        QianWangShouYinTai,
        JieZhangChengGong,
        Angry,
    }

    public class CustomerController : MonoBehaviour
    {
        private enum RouteMovePhase
        {
            None,
            EnterRouteForward,
            MoveToStateTarget,
            MoveToRouteTail,
            ReturnAlongRoute,
        }

        public PolyNavAgent agent;
        public CustomerData data;
        public NpcState state;
        public Vector2 bornPosition;
        public Vector2 nextPosition;
        public Vector2 purchasePosition;
        public GoodsType goodsType;
        public SkeletonAnimation skeletonAnimation;
        public SpriteRenderer shadow;
        public int currentIndex = 0;
        public SalesStall salesStall;
        public Transform receiveTransform;
        public List<Production> purchaseList = new();
        public MeshRenderer _meshRenderer;
        public GameObject fillBg;
        public GameObject fill;
        public bool severing = false;

        public int SelectedRouteIndex => routeIndex;
        public int SavedRouteMovePhase => (int)routeMovePhase;
        public int SavedRouteWaypointIndex => routeWaypointIndex;

        private Vector2 spawnOrigin;
        private float travelRepathTimer = 0f;
        private const float TravelRepathInterval = 0.5f;
        private bool mapWarningShown;
        private Coroutine ensureMoveCoroutine;
        private Coroutine invalidRecoverCoroutine;
        private Coroutine purchaseRoutineCoroutine;
        private float purchaseIdleTimer = 0f;
        private int spawnRelocateAttempts = 0;
        private const float PurchaseIdleRelocateDelay = 0.6f;
        private NpcState lastState;
        private CustomerFactory customerFactory;
        private int routeIndex = -1;
        private readonly List<Vector2> routeWaypoints = new();
        private RouteMovePhase routeMovePhase = RouteMovePhase.None;
        private int routeWaypointIndex = -1;
        private Vector2 stateTargetPosition;
        private int lastSortingOrder = int.MinValue;
        private int lastPurchaseCount = -1;
        private PolyNavMap cachedMap;
        private float nextEnsureMapRetryTime;
        private const float EnsureMapRetryInterval = 1f;
        private const float StateArrivalTolerance = 0.75f;
        private const float MovementCheckInterval = 0.1f;
        private float movementCheckTimer;
        private string currentLoopAnimation;
        private int currentFacingSign = 1;

        void Start()
        {
        }

        void Update()
        {
            if (state != lastState)
            {
                lastState = state;
                purchaseIdleTimer = 0f;
                spawnRelocateAttempts = 0;
                currentLoopAnimation = null;
            }

            SetLayer();
            UpdateMovementAnimation();

            movementCheckTimer -= Time.deltaTime;
            if (movementCheckTimer > 0f)
            {
                return;
            }

            movementCheckTimer = MovementCheckInterval;
            if (agent == null || agent.map == null)
            {
                EnsureMap();
            }
            EnsureTravelMovement();
            EnsurePurchaseMovement();
        }

        public void SetLayer()
        {
            if (_meshRenderer == null)
            {
                _meshRenderer = skeletonAnimation.GetComponent<MeshRenderer>();
            }

            int order = 30000 - Mathf.RoundToInt(transform.position.y * 100);
            if (order == lastSortingOrder && purchaseList.Count == lastPurchaseCount)
            {
                return;
            }

            lastSortingOrder = order;
            lastPurchaseCount = purchaseList.Count;

            _meshRenderer.sortingOrder = order;
            shadow.sortingOrder = order - 5;
            for (int i = 0; i < purchaseList.Count; i++)
            {
                var obj = purchaseList[i];
                var mesh = obj != null ? obj.spriteRenderer : null;
                if (mesh != null)
                {
                    mesh.sortingOrder = order + i + 1;
                }
            }
        }

        public void Init(
            CustomerData outdata,
            GoodsType type,
            StructureBase structureBase,
            Vector2 routeStartPosition,
            CustomerFactory factory = null,
            int selectedRouteIndex = -1,
            bool startMovement = true)
        {
            severing = false;
            goodsType = type;
            data = outdata;
            state = NpcState.QianWangGouMai;
            lastState = state;
            purchaseIdleTimer = 0f;
            spawnRelocateAttempts = 0;
            movementCheckTimer = 0f;
            currentLoopAnimation = null;
            currentFacingSign = 1;
            customerFactory = factory;
            routeIndex = selectedRouteIndex;
            bornPosition = routeStartPosition;
            spawnOrigin = routeStartPosition;
            routeMovePhase = RouteMovePhase.None;
            routeWaypointIndex = -1;
            salesStall = structureBase as SalesStall;
            agent = GetComponent<PolyNavAgent>();
            EnsureMap();
            ConfigureRoute();
            transform.position = new Vector3(bornPosition.x, bornPosition.y, transform.position.z);

            fillBg.gameObject.SetActive(false);
            fill.gameObject.transform.localScale = new Vector3(0, 1, 1);

            if (startMovement)
            {
                RefreshMovementByState();
            }
        }

        public void RestoreRuntimeState(NpcState savedState, int savedRoutePhase, int savedRouteWaypointIndex)
        {
            state = savedState;
            lastState = state;
            purchaseIdleTimer = 0f;
            spawnRelocateAttempts = 0;
            movementCheckTimer = 0f;
            currentLoopAnimation = null;
            SetNextPosition();

            routeMovePhase = ParseRouteMovePhase(savedRoutePhase);
            routeWaypointIndex = savedRouteWaypointIndex;

            if (!ResumeSavedMovement())
            {
                RefreshMovementByState();
            }
        }

        public void WaitPurchase()
        {
            routeMovePhase = RouteMovePhase.None;
            routeWaypointIndex = -1;
            state = NpcState.WaitGouMaiWanCheng;
            if (purchaseRoutineCoroutine != null)
            {
                StopCoroutine(purchaseRoutineCoroutine);
            }

            purchaseRoutineCoroutine = StartCoroutine(PurchaseRoutine());
        }

        public void SetNextPosition()
        {
            if (state == NpcState.QianWangGouMai)
            {
                stateTargetPosition = salesStall != null ? salesStall.GetPurchasePosition() : (Vector2)transform.position;
            }
            else if (state == NpcState.QianWangShouYinTai)
            {
                if (GameController.Instance != null &&
                    GameController.Instance.buildings.TryGetValue(BuildingType.LingZhangTai, out var cashierBase))
                {
                    var cashier = cashierBase as CashierCounter;
                    if (cashier != null && cashier.parchaseTransform != null)
                    {
                        purchasePosition = cashier.parchaseTransform.position;
                        stateTargetPosition = purchasePosition;
                    }
                    else
                    {
                        stateTargetPosition = (Vector2)transform.position;
                    }
                }
                else
                {
                    stateTargetPosition = (Vector2)transform.position;
                }
            }
            else if (state is NpcState.JieZhangChengGong or NpcState.Angry)
            {
                stateTargetPosition = bornPosition;
            }
            else
            {
                stateTargetPosition = (Vector2)transform.position;
            }

            stateTargetPosition = GetValidMapPoint(stateTargetPosition);
            nextPosition = stateTargetPosition;
        }

        public void RefreshMovementByState()
        {
            SetNextPosition();

            switch (state)
            {
                case NpcState.QianWangGouMai:
                    if (HasRouteWaypoints)
                    {
                        routeMovePhase = RouteMovePhase.EnterRouteForward;
                        routeWaypointIndex = 0;
                        TrySetDestination(routeWaypoints[routeWaypointIndex]);
                    }
                    else
                    {
                        routeMovePhase = RouteMovePhase.MoveToStateTarget;
                        routeWaypointIndex = -1;
                        TrySetDestination(stateTargetPosition);
                    }
                    break;
                case NpcState.QianWangShouYinTai:
                    routeMovePhase = RouteMovePhase.MoveToStateTarget;
                    routeWaypointIndex = -1;
                    TrySetDestination(stateTargetPosition);
                    break;
                case NpcState.JieZhangChengGong:
                case NpcState.Angry:
                    BeginReturnMovement();
                    break;
                default:
                    routeMovePhase = RouteMovePhase.None;
                    routeWaypointIndex = -1;
                    nextPosition = (Vector2)transform.position;
                    break;
            }

            RestartEnsureMoveStarted();
        }

        void OnEnable()
        {
            if (agent == null)
            {
                agent = GetComponent<PolyNavAgent>();
            }

            if (agent != null)
            {
                agent.OnDestinationReached += OnReachDestination;
                agent.OnDestinationInvalid += OnDestinationInvalid;
            }

            movementCheckTimer = 0f;
        }

        void OnDisable()
        {
            if (agent != null)
            {
                agent.OnDestinationReached -= OnReachDestination;
                agent.OnDestinationInvalid -= OnDestinationInvalid;
            }
        }

        void OnReachDestination()
        {
            if (Vector2.Distance((Vector2)transform.position, bornPosition) < 1f &&
                (state == NpcState.Angry || state == NpcState.JieZhangChengGong))
            {
                Destroy(gameObject);
                return;
            }

            if (HandleRouteArrival())
            {
                return;
            }

            if (state == NpcState.QianWangGouMai)
            {
                if (!HasReachedStateTarget())
                {
                    TrySetDestination(stateTargetPosition);
                    RestartEnsureMoveStarted();
                    return;
                }

                WaitPurchase();
                EventCenter.Instance.TriggerEvent(EventMessages.CustomerArrivedSell, this, salesStall);
                return;
            }

            if (state == NpcState.QianWangShouYinTai)
            {
                if (!HasReachedStateTarget())
                {
                    TrySetDestination(stateTargetPosition);
                    RestartEnsureMoveStarted();
                    return;
                }

                EventCenter.Instance.TriggerEvent(EventMessages.CustomerArrived, this, salesStall);
            }
        }

        private bool HasReachedStateTarget()
        {
            return Vector2.Distance((Vector2)transform.position, stateTargetPosition) <= StateArrivalTolerance;
        }

        private void ConfigureRoute()
        {
            routeWaypoints.Clear();
            if (customerFactory == null)
            {
                return;
            }

            if (!customerFactory.TryBuildRoute(routeIndex, out var routeStart, out var waypoints))
            {
                return;
            }

            bornPosition = routeStart;
            spawnOrigin = bornPosition;

            for (int i = 0; i < waypoints.Count; i++)
            {
                routeWaypoints.Add(waypoints[i]);
            }
        }

        private bool ResumeSavedMovement()
        {
            switch (state)
            {
                case NpcState.WaitGouMaiWanCheng:
                case NpcState.None:
                    routeMovePhase = RouteMovePhase.None;
                    routeWaypointIndex = -1;
                    return true;
            }

            switch (routeMovePhase)
            {
                case RouteMovePhase.EnterRouteForward:
                    if (!HasRouteWaypoints || routeWaypointIndex < 0 || routeWaypointIndex >= routeWaypoints.Count)
                    {
                        return false;
                    }

                    return TrySetDestination(routeWaypoints[routeWaypointIndex]);
                case RouteMovePhase.MoveToStateTarget:
                    routeWaypointIndex = -1;
                    return TrySetDestination(stateTargetPosition);
                case RouteMovePhase.MoveToRouteTail:
                    if (!HasRouteWaypoints)
                    {
                        return false;
                    }

                    routeWaypointIndex = routeWaypoints.Count - 1;
                    return TrySetDestination(routeWaypoints[routeWaypointIndex]);
                case RouteMovePhase.ReturnAlongRoute:
                    if (routeWaypointIndex >= 0)
                    {
                        if (!HasRouteWaypoints || routeWaypointIndex >= routeWaypoints.Count)
                        {
                            return false;
                        }

                        return TrySetDestination(routeWaypoints[routeWaypointIndex]);
                    }

                    return TrySetDestination(bornPosition);
                default:
                    return false;
            }
        }

        private void BeginReturnMovement()
        {
            if (!HasRouteWaypoints)
            {
                routeMovePhase = RouteMovePhase.ReturnAlongRoute;
                routeWaypointIndex = -1;
                TrySetDestination(bornPosition);
                return;
            }

            int tailIndex = routeWaypoints.Count - 1;
            Vector2 tailPoint = routeWaypoints[tailIndex];
            if (Vector2.Distance((Vector2)transform.position, tailPoint) <= 0.5f)
            {
                routeMovePhase = RouteMovePhase.ReturnAlongRoute;
                routeWaypointIndex = tailIndex - 1;
                TrySetDestination(routeWaypointIndex >= 0 ? routeWaypoints[routeWaypointIndex] : bornPosition);
                return;
            }

            routeMovePhase = RouteMovePhase.MoveToRouteTail;
            routeWaypointIndex = tailIndex;
            TrySetDestination(tailPoint);
        }

        private bool HandleRouteArrival()
        {
            switch (routeMovePhase)
            {
                case RouteMovePhase.EnterRouteForward:
                    if (routeWaypointIndex < routeWaypoints.Count - 1)
                    {
                        routeWaypointIndex++;
                        TrySetDestination(routeWaypoints[routeWaypointIndex]);
                    }
                    else
                    {
                        routeMovePhase = RouteMovePhase.MoveToStateTarget;
                        routeWaypointIndex = -1;
                        TrySetDestination(stateTargetPosition);
                    }

                    RestartEnsureMoveStarted();
                    return true;
                case RouteMovePhase.MoveToRouteTail:
                    routeMovePhase = RouteMovePhase.ReturnAlongRoute;
                    routeWaypointIndex = routeWaypoints.Count - 2;
                    TrySetDestination(routeWaypointIndex >= 0 ? routeWaypoints[routeWaypointIndex] : bornPosition);
                    RestartEnsureMoveStarted();
                    return true;
                case RouteMovePhase.ReturnAlongRoute:
                    if (routeWaypointIndex > 0)
                    {
                        routeWaypointIndex--;
                        TrySetDestination(routeWaypoints[routeWaypointIndex]);
                        RestartEnsureMoveStarted();
                        return true;
                    }

                    if (routeWaypointIndex == 0)
                    {
                        routeWaypointIndex = -1;
                        TrySetDestination(bornPosition);
                        RestartEnsureMoveStarted();
                        return true;
                    }

                    routeMovePhase = RouteMovePhase.None;
                    return false;
                default:
                    return false;
            }
        }

        private void RestartEnsureMoveStarted()
        {
            if (ensureMoveCoroutine != null)
            {
                StopCoroutine(ensureMoveCoroutine);
            }

            ensureMoveCoroutine = StartCoroutine(EnsureMoveStarted());
        }

        private void EnsurePurchaseMovement()
        {
            if (state != NpcState.QianWangGouMai)
            {
                purchaseIdleTimer = 0f;
                return;
            }

            if (agent == null)
            {
                return;
            }

            if (agent.pathPending || agent.hasPath || agent.remainingDistance > 0.1f)
            {
                purchaseIdleTimer = 0f;
                return;
            }

            purchaseIdleTimer += Time.deltaTime;
            if (purchaseIdleTimer < PurchaseIdleRelocateDelay)
            {
                return;
            }
            purchaseIdleTimer = 0f;

            if (routeIndex >= 0)
            {
                TrySetDestination(nextPosition);
                RestartEnsureMoveStarted();
                return;
            }
        }

        private void EnsureTravelMovement()
        {
            if (routeMovePhase == RouteMovePhase.None)
            {
                travelRepathTimer = 0f;
                return;
            }

            if (agent == null)
            {
                return;
            }

            if (agent.map == null)
            {
                EnsureMap();
                if (agent.map == null)
                {
                    return;
                }
            }

            float distSqr = (nextPosition - (Vector2)transform.position).sqrMagnitude;
            if (distSqr <= 0.25f)
            {
                travelRepathTimer = 0f;
                return;
            }

            bool needsRepath = !agent.hasPath || agent.remainingDistance < 0.1f;
            if (!needsRepath && agent.movingDirection == Vector2.zero)
            {
                needsRepath = true;
            }

            if (needsRepath)
            {
                travelRepathTimer += Time.deltaTime;
                if (travelRepathTimer >= TravelRepathInterval)
                {
                    TrySetDestination(nextPosition);
                    travelRepathTimer = 0f;
                }
            }
            else
            {
                travelRepathTimer = 0f;
            }
        }

        private bool EnsureMap()
        {
            if (agent == null)
            {
                return false;
            }

            if (agent.map != null)
            {
                cachedMap = agent.map;
                return true;
            }

            if (cachedMap != null)
            {
                agent.map = cachedMap;
                return agent.map.nodesCount > 0;
            }

            if (Time.time < nextEnsureMapRetryTime)
            {
                return false;
            }

            nextEnsureMapRetryTime = Time.time + EnsureMapRetryInterval;

            PolyNavMap map = null;
            var mapObj = GameObject.FindWithTag("Map");
            if (mapObj != null)
            {
                map = mapObj.GetComponent<PolyNavMap>();
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

            agent.map = map;
            if (agent.map != null && agent.map.nodesCount == 0)
            {
                agent.map.GenerateMap();
            }

            if (agent.map != null && agent.map.nodesCount > 0)
            {
                cachedMap = agent.map;
            }

            if ((agent.map == null || agent.map.nodesCount == 0) && !mapWarningShown)
            {
                mapWarningShown = true;
                Debug.LogWarning("[Customer] PolyNavMap not ready, movement disabled.");
            }
            return agent.map != null && agent.map.nodesCount > 0;
        }

        private Vector2 GetValidMapPoint(Vector2 point)
        {
            if (!EnsureMap())
            {
                return point;
            }

            if (!agent.map.PointIsValid(point))
            {
                point = agent.map.GetCloserEdgePoint(point);
            }

            return point;
        }

        private IEnumerator PurchaseRoutine()
        {
            float timer = 0f;
            while (timer < data.waitTime)
            {
                if (state != NpcState.WaitGouMaiWanCheng)
                {
                    purchaseRoutineCoroutine = null;
                    yield break;
                }

                if (purchaseList.Count >= data.carryNum)
                {
                    Purchase();
                    yield break;
                }

                if (severing)
                {
                    yield return null;
                    continue;
                }

                timer += Time.deltaTime;
                yield return null;
            }

            if (state != NpcState.WaitGouMaiWanCheng)
            {
                purchaseRoutineCoroutine = null;
                yield break;
            }

            if (purchaseList.Count > 0)
            {
                Purchase();
                yield break;
            }

            if (severing)
            {
                float graceTimer = 0.2f;
                while (graceTimer > 0f && state == NpcState.WaitGouMaiWanCheng && severing && purchaseList.Count == 0)
                {
                    graceTimer -= Time.deltaTime;
                    yield return null;
                }

                if (state != NpcState.WaitGouMaiWanCheng)
                {
                    purchaseRoutineCoroutine = null;
                    yield break;
                }

                if (purchaseList.Count > 0)
                {
                    Purchase();
                    yield break;
                }
            }

            if (purchaseList.Count < data.carryNum)
            {
                currentLoopAnimation = null;
                skeletonAnimation.AnimationState.SetAnimation(0, "angry", false);
                yield return new WaitForSeconds(1f);

                if (state != NpcState.WaitGouMaiWanCheng)
                {
                    purchaseRoutineCoroutine = null;
                    yield break;
                }

                if (purchaseList.Count > 0)
                {
                    Purchase();
                    yield break;
                }

                OnPurchaseTimeout();
            }

            purchaseRoutineCoroutine = null;
        }

        private void Purchase()
        {
            if (state != NpcState.WaitGouMaiWanCheng)
            {
                purchaseRoutineCoroutine = null;
                return;
            }

            for (int i = 0; i < purchaseList.Count; i++)
            {
                var obj = purchaseList[i];
                if (obj == null)
                {
                    continue;
                }

                obj.SetState(ItemState.HeldByCustomer);
                obj.transform.SetParent(transform, false);
                obj.transform.position = receiveTransform.position;
            }

            severing = false;
            purchaseRoutineCoroutine = null;
            state = NpcState.QianWangShouYinTai;
            RefreshMovementByState();
        }

        private void OnPurchaseTimeout()
        {
            if (state != NpcState.WaitGouMaiWanCheng)
            {
                purchaseRoutineCoroutine = null;
                return;
            }

            purchaseRoutineCoroutine = null;
            severing = false;
            ReturnPurchasedProductsToStall();
            state = NpcState.Angry;
            RefreshMovementByState();
            EventCenter.Instance.TriggerEvent(EventMessages.CustomerLeave, salesStall, this);
        }

        private void ReturnPurchasedProductsToStall()
        {
            if (purchaseList.Count == 0)
            {
                return;
            }

            for (int i = purchaseList.Count - 1; i >= 0; i--)
            {
                Production product = purchaseList[i];
                purchaseList.RemoveAt(i);
                if (product == null)
                {
                    continue;
                }

                salesStall?.PlaceProduct(product);
            }
        }

        public bool CanReceivePurchasedProduct()
        {
            return this != null &&
                   gameObject != null &&
                   gameObject.activeInHierarchy &&
                   state == NpcState.WaitGouMaiWanCheng;
        }

        public void ReceivePurchasedProduct(Production product)
        {
            if (product == null)
            {
                return;
            }

            if (!CanReceivePurchasedProduct())
            {
                salesStall?.PlaceProduct(product);
                return;
            }

            if (!purchaseList.Contains(product))
            {
                purchaseList.Add(product);
            }

            product.SetState(ItemState.HeldByCustomer);
            product.transform.SetParent(transform, false);
            product.transform.position = receiveTransform.position;
        }

        private bool TrySetDestination(Vector2 target)
        {
            if (agent == null)
            {
                return false;
            }
            if (!EnsureMap())
            {
                return false;
            }

            var map = agent.map;
            if (map == null)
            {
                return false;
            }

            if (!map.PointIsValid(target))
            {
                target = map.GetCloserEdgePoint(target);
            }

            nextPosition = target;
            return agent.SetDestination(target);
        }

        private void UpdateMovementAnimation()
        {
            if (skeletonAnimation == null || skeletonAnimation.AnimationState == null)
            {
                return;
            }

            bool isWalking = agent != null && (agent.hasPath || agent.remainingDistance > 1f);
            SetLoopAnimation(isWalking ? "walk" : "idle");

            Vector2 dir = agent != null ? agent.movingDirection : Vector2.zero;
            if (dir == Vector2.zero || Mathf.Abs(dir.x) <= 0.01f || skeletonAnimation.skeleton == null)
            {
                return;
            }

            int facingSign = dir.x < 0 ? -1 : 1;
            if (facingSign == currentFacingSign)
            {
                return;
            }

            currentFacingSign = facingSign;
            skeletonAnimation.skeleton.ScaleX = currentFacingSign;
        }

        private void SetLoopAnimation(string animationName)
        {
            if (currentLoopAnimation == animationName)
            {
                return;
            }

            skeletonAnimation.AnimationState.SetAnimation(0, animationName, true);
            currentLoopAnimation = animationName;
        }

        private IEnumerator EnsureMoveStarted()
        {
            const int maxAttempts = 3;
            int attempts = 0;
            while (attempts < maxAttempts)
            {
                yield return null;
                if (agent == null)
                {
                    yield break;
                }

                if (agent.pathPending)
                {
                    continue;
                }

                if (agent.hasPath || agent.remainingDistance > 0.1f)
                {
                    yield break;
                }

                attempts++;
                if (EnsureMap())
                {
                    TrySetDestination(nextPosition);
                }
            }

            if (EnsureMap())
            {
                TrySetDestination(nextPosition);
            }
        }

        private void OnDestinationInvalid()
        {
            if (invalidRecoverCoroutine != null)
            {
                return;
            }

            invalidRecoverCoroutine = StartCoroutine(RecoverFromInvalidPath());
        }

        private IEnumerator RecoverFromInvalidPath()
        {
            yield return null;
            const int maxAttempts = 3;
            int attempts = 0;
            while (attempts < maxAttempts)
            {
                attempts++;
                if (!EnsureMap())
                {
                    yield return null;
                    continue;
                }

                TrySetDestination(nextPosition);

                float wait = 0f;
                while (agent != null && agent.pathPending && wait < 0.5f)
                {
                    wait += Time.deltaTime;
                    yield return null;
                }

                if (agent != null && (agent.hasPath || agent.remainingDistance > 0.1f))
                {
                    invalidRecoverCoroutine = null;
                    yield break;
                }

                yield return null;
            }

            if (EnsureMap())
            {
                TrySetDestination(nextPosition);
            }

            invalidRecoverCoroutine = null;
        }

        private RouteMovePhase ParseRouteMovePhase(int savedValue)
        {
            if (savedValue < (int)RouteMovePhase.None || savedValue > (int)RouteMovePhase.ReturnAlongRoute)
            {
                return RouteMovePhase.None;
            }

            return (RouteMovePhase)savedValue;
        }

        private bool HasRouteWaypoints => routeWaypoints.Count > 0;
    }
}
