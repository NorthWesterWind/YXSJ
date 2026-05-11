using System;
using System.Collections;
using System.Collections.Generic;
using Controller.Pickups;
using Controller.Structure;
using Module;
using Module.Data;
using PolyNav;
using Sirenix.OdinInspector;
using Spine.Unity;
using UnityEngine;
using Utils;

namespace Controller
{
    public class FreightClerkController : SerializedMonoBehaviour
    {
        public SkeletonAnimation skeletonAnimation;
        public MeshRenderer renderer;
        public SpriteRenderer shadow;
        public PolyNavAgent _agent;
        public int currentCapacity;
        private AssetHandle _assetHandle;


        public List<Transform> points = new List<Transform>();
        public List<ProductionStation> productionStationList = new List<ProductionStation>();
        public List<SalesStall> salesStallList = new List<SalesStall>();
        public Transform normalPos;
        public List<Production> productList = new List<Production>();
        private Vector2 idleTargetPosition;
        private int idleSlotIndex = -1;
        private ProductionStation idleStation;
        private readonly List<Vector2> idleRouteWaypoints = new List<Vector2>();
        private const float IdleAnchorOffsetDistance = 0.3f;
        private const float IdleSpreadDistance = 0.55f;
        private const int IdleSlotsPerRing = 6;
        private static int NextIdleSlotIndex;

        private ProductionStation targetStation; // 当前目标生产台
        private SalesStall targetStall;          // 对应销售摊位
        private bool isWorking;
        private bool needDestory;  // 是否需要销毁
        private bool mapWarningShown;

        /// <summary>
        /// 静态字典：记录每个生产台当前有多少搬运工在“前往 / 服务”
        /// 用来尽量避免多个搬运工一起跑向同一个生产台
        /// </summary>
        private static readonly Dictionary<ProductionStation, int> StationWorkingClerkCount =
            new Dictionary<ProductionStation, int>();

        private static readonly HashSet<Production> ReservedProductsByFreight =
            new HashSet<Production>();

        public static void MarkProductReservedByFreight(Production production)
        {
            if (production == null)
            {
                return;
            }

            ReservedProductsByFreight.Add(production);
        }

        public static void UnmarkProductReservedByFreight(Production production)
        {
            if (production == null)
            {
                return;
            }

            ReservedProductsByFreight.Remove(production);
        }

        public static bool IsProductReservedByFreight(Production production)
        {
            if (production == null)
            {
                return false;
            }

            return ReservedProductsByFreight.Contains(production);
        }

        public static bool IsStationClaimedByFreight(ProductionStation station)
        {
            if (station == null)
            {
                return false;
            }

            return StationWorkingClerkCount.TryGetValue(station, out var count) && count > 0;
        }

        public static void ResetStationReservations()
        {
            StationWorkingClerkCount.Clear();
            ReservedProductsByFreight.Clear();
            NextIdleSlotIndex = 0;
        }

        public void CleanupBeforeDestroy()
        {
            ReleaseStationReservation();
        }

        public bool HasCarriedProducts()
        {
            return productList != null && productList.Count > 0;
        }

        private void UpdateSpeed(params object[] args)
        {
            _agent.maxSpeed = WorldData.speedLevelDic[PlayerDataModule.Instance.data.deliverData.speedLevel];
        }
        public void Init()
        {
            // 缺失组件保护
            if (_agent == null)
            {
                _agent = GetComponent<PolyNavAgent>();
            }

            if (_assetHandle == null)
            {
                _assetHandle = GetComponent<AssetHandle>();
            }

            // 搬运能力：从玩家云递阁数据中读取，并限制在挂点数量范围内
            var deliverData = PlayerDataModule.Instance.data.deliverData;
            currentCapacity = 1;
            var cardUpProgresses = PlayerDataModule.Instance.data.cardUpProgressesList;
            var cardUpProgress = cardUpProgresses.Find(x => x.developType == CardDevelopType.UpgradeYunDiGe);
            if (cardUpProgress != null)
            {
                currentCapacity = cardUpProgress.level;
            }

            productionStationList = GameController.Instance.productionStationList;
            salesStallList = GameController.Instance.salesStallList;
            if (GameController.Instance != null)
            {
                GameController.Instance.RefreshStructureCaches();
                productionStationList = GameController.Instance.productionStationList;
                salesStallList = GameController.Instance.salesStallList;
            }
            // 初始化导航
            EnsureMap();
            SnapToValidPosition();
            ResolveIdlePosition();
            if (normalPos != null)
            {
                if (EnsureMap())
                {
                    SetInitialIdleDestination();
                }
            }
            _agent.maxSpeed = WorldData.speedLevelDic[deliverData.speedLevel];
            StartCoroutine(WorkerLoop());
        }

        private void ResolveIdlePosition()
        {
            EnsureIdleSlotIndex();
            normalPos = null;
            idleStation = null;
            idleRouteWaypoints.Clear();
            idleTargetPosition = transform.position;

            var availableStations = new List<ProductionStation>();
            if (productionStationList != null)
            {
                for (int i = 0; i < productionStationList.Count; i++)
                {
                    var station = productionStationList[i];
                    if (station == null) continue;
                    if (station.isLock || station.isCanUnlockState) continue;
                    availableStations.Add(station);
                }
            }

            if (availableStations.Count > 0)
            {
                int stationIndex = Mathf.Abs(idleSlotIndex) % availableStations.Count;
                int overlapIndex = Mathf.Abs(idleSlotIndex) / availableStations.Count;
                var station = availableStations[stationIndex];
                idleStation = station;
                normalPos = station.transferPoint != null ? station.transferPoint : station.transform;
                Vector2 anchorPosition = normalPos != null
                    ? (Vector2)normalPos.position
                    : (Vector2)station.transform.position;
                idleTargetPosition = BuildIdleTargetPosition(anchorPosition, station.transform, overlapIndex, stationIndex);
                RefreshIdleRoute();
                return;
            }

            var fallbackStation = FindObjectOfType<ProductionStation>();
            if (fallbackStation != null)
            {
                if (fallbackStation.isLock || fallbackStation.isCanUnlockState)
                {
                    return;
                }

                normalPos = fallbackStation.transferPoint != null
                    ? fallbackStation.transferPoint
                    : fallbackStation.transform;
                idleStation = fallbackStation;
                Vector2 anchorPosition = normalPos != null
                    ? (Vector2)normalPos.position
                    : (Vector2)fallbackStation.transform.position;
                idleTargetPosition = BuildIdleTargetPosition(anchorPosition, fallbackStation.transform, 0, 0);
                RefreshIdleRoute();
            }
        }

        private void RefreshIdleRoute()
        {
            idleRouteWaypoints.Clear();
            if (idleStation == null || GameController.Instance == null)
            {
                return;
            }

            if (GameController.Instance.TryBuildFreightClerkRoute(idleStation.buildingType, out var routeWaypoints))
            {
                idleRouteWaypoints.AddRange(routeWaypoints);
            }
        }

        private void EnsureIdleSlotIndex()
        {
            if (idleSlotIndex >= 0)
            {
                return;
            }

            idleSlotIndex = NextIdleSlotIndex++;
        }

        private Vector2 BuildIdleTargetPosition(Vector2 anchorPosition, Transform stationTransform, int overlapIndex, int stationSeed)
        {
            Vector2 baseOffset = GetIdleAnchorOffset(anchorPosition, stationTransform, stationSeed);
            if (overlapIndex <= 0)
            {
                return anchorPosition + baseOffset;
            }

            int ring = ((overlapIndex - 1) / IdleSlotsPerRing) + 1;
            int slotInRing = (overlapIndex - 1) % IdleSlotsPerRing;
            float angle = ((slotInRing + (stationSeed * 0.5f)) / IdleSlotsPerRing) * Mathf.PI * 2f;
            Vector2 ringOffset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (IdleSpreadDistance * ring);
            return anchorPosition + baseOffset + ringOffset;
        }

        private Vector2 GetIdleAnchorOffset(Vector2 anchorPosition, Transform stationTransform, int stationSeed)
        {
            if (stationTransform != null)
            {
                Vector2 awayFromStation = anchorPosition - (Vector2)stationTransform.position;
                if (awayFromStation.sqrMagnitude > 0.0001f)
                {
                    return awayFromStation.normalized * IdleAnchorOffsetDistance;
                }
            }

            float fallbackAngle = stationSeed * 0.9f;
            return new Vector2(Mathf.Cos(fallbackAngle), Mathf.Sin(fallbackAngle)) * IdleAnchorOffsetDistance;
        }

        private void RefreshWorkTargets()
        {
            if (GameController.Instance != null)
            {
                GameController.Instance.RefreshStructureCaches();
                productionStationList = GameController.Instance.productionStationList;
                salesStallList = GameController.Instance.salesStallList;
            }

            if (productionStationList == null)
            {
                productionStationList = new List<ProductionStation>();
            }
            else
            {
                productionStationList.RemoveAll(x => x == null);
            }

            if (salesStallList == null)
            {
                salesStallList = new List<SalesStall>();
            }
            else
            {
                salesStallList.RemoveAll(x => x == null);
            }

            ResolveIdlePosition();
        }

        public void StopWorking()
        {
            // 标记为需要销毁，由工作循环在合适的时机处理，确保释放生产台占用等状态
            needDestory = true;
        }



        private IEnumerator WorkerLoop()
        {
            yield return null;

            while (true)
            {
                RefreshWorkTargets();
                // 如果被要求停止工作，并且身上已经没有货物，安全销毁自己
                if (needDestory && productList.Count == 0)
                {
                    ReleaseStationReservation();
                    Destroy(gameObject);
                    yield break;
                }

                // 查找目标生产台
                targetStation = FindValidProductionStation();
                if (targetStation == null)
                {
                    // 没有任何生产台有产品，让搬运工回到待命点 normalPos 原地待命
                    if (normalPos != null)
                    {
                        yield return MoveToIdleTarget();
                    }
                    // 适当等待再重新尝试
                    yield return new WaitForSeconds(2f);
                    continue;
                }

                // 去取货
                yield return MoveTo(targetStation.transferPoint.position);

                // 抵达后再次检查是否有货
                if (targetStation.productionList.Count < 1)
                {
                    // 该生产台在路上被别人取空了，释放占用，并回待命点
                    ReleaseStationReservation();
                    if (normalPos != null)
                    {
                        yield return MoveToIdleTarget();
                    }
                    yield return new WaitForSeconds(2f);
                    continue;
                }

                // 拿货（玩家优先，所以可能拿到空列表或少量产品）
                productList = targetStation.TakeProduct(this);

                // 如果取货失败或被玩家抢走了，释放占用并继续
                if (productList == null || productList.Count == 0)
                {
                    ReleaseStationReservation();
                    if (normalPos != null)
                    {
                        yield return MoveToIdleTarget();
                    }
                    yield return new WaitForSeconds(2f);
                    continue;
                }
                foreach (var product in productList)
                {
                    product.spriteRenderer.sortingOrder = renderer.sortingOrder + 1;
                }

                yield return new WaitForSeconds(0.5f + productList.Count);

                // 找到对应该商品的销售摊位
                targetStall = FindSalesStall(targetStation.goodsType);

                if (targetStall == null)
                {
                    Debug.LogError("没有找到对应商品的摊位！");
                    ReleaseStationReservation();
                    continue;
                }

                // 送货
                yield return MoveTo(targetStall.transferPoint.position);

                // 放下商品
                targetStall.ReceiveProduct(this);

                // 完成本次生产台的任务，释放占用
                ReleaseStationReservation();


                yield return new WaitForSeconds(1f);

            }
        }

        private void SetInitialIdleDestination()
        {
            Vector2 initialTarget = idleTargetPosition;
            if (TryGetIdleRouteStartDestination(out var routeStart))
            {
                initialTarget = routeStart;
            }

            _agent.SetDestination(ClampToNav(initialTarget));
        }

        private IEnumerator MoveToIdleTarget()
        {
            if (TryBuildIdleRouteTraversal(out var routeTraversal))
            {
                for (int i = 0; i < routeTraversal.Count; i++)
                {
                    yield return MoveTo(routeTraversal[i]);
                }
            }

            yield return MoveTo(idleTargetPosition);
        }

        private bool TryGetIdleRouteStartDestination(out Vector2 routeStart)
        {
            routeStart = idleTargetPosition;
            if (idleRouteWaypoints == null || idleRouteWaypoints.Count == 0)
            {
                return false;
            }

            routeStart = idleRouteWaypoints[0];
            return true;
        }

        private bool TryBuildIdleRouteTraversal(out List<Vector2> traversal)
        {
            traversal = null;
            if (idleRouteWaypoints == null || idleRouteWaypoints.Count == 0)
            {
                return false;
            }

            Vector2 currentPosition = transform.position;
            float directDistance = Vector2.Distance(currentPosition, idleTargetPosition);
            float firstWaypointDistance = Vector2.Distance(currentPosition, idleRouteWaypoints[0]);
            if (directDistance <= firstWaypointDistance)
            {
                return false;
            }

            int startIndex = 0;
            float nearestDistance = float.MaxValue;
            for (int i = 0; i < idleRouteWaypoints.Count; i++)
            {
                float distance = Vector2.Distance(currentPosition, idleRouteWaypoints[i]);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    startIndex = i;
                }
            }

            if (nearestDistance > 2f)
            {
                startIndex = 0;
            }

            traversal = new List<Vector2>();
            for (int i = startIndex; i < idleRouteWaypoints.Count; i++)
            {
                if (Vector2.Distance(idleRouteWaypoints[i], idleTargetPosition) <= 0.1f)
                {
                    continue;
                }

                traversal.Add(idleRouteWaypoints[i]);
            }

            return traversal.Count > 0;
        }

        private IEnumerator MoveTo(Vector2 target)
        {
            if (!EnsureMap())
            {
                yield break;
            }

            Vector2 clamped = ClampToNav(target);
            const int maxAttempts = 3;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                bool finished = false;
                bool success = false;
                _agent.SetDestination(clamped, ok =>
                {
                    finished = true;
                    success = ok;
                });

                while (!finished)
                {
                    if (_agent == null)
                    {
                        yield break;
                    }

                    float distance = Vector2.Distance(transform.position, clamped);
                    if (distance <= 0.6f)
                    {
                        success = true;
                        finished = true;
                        break;
                    }

                    if (!_agent.pathPending && distance <= 1f &&
                        (!_agent.hasPath || _agent.remainingDistance <= 0.15f))
                    {
                        success = true;
                        finished = true;
                        break;
                    }
                    yield return null;
                }

                if (success || Vector2.Distance(transform.position, clamped) <= 1f)
                {
                    yield break;
                }

                if (EnsureMap())
                {
                    SnapToValidPosition();
                    clamped = ClampToNav(target);
                }

                yield return null;
            }
        }

        private Vector2 ClampToNav(Vector2 pos)
        {
            if (!EnsureMap())
            {
                return pos;
            }
            var map = _agent.map;
            return map.PointIsValid(pos) ? pos : map.GetCloserEdgePoint(pos);
        }

        private void SnapToValidPosition()
        {
            if (!EnsureMap())
            {
                return;
            }
            var map = _agent.map;
            Vector2 pos = transform.position;
            if (!map.PointIsValid(pos))
            {
                Vector2 fixedPos = map.GetCloserEdgePoint(pos);
                transform.position = new Vector3(fixedPos.x, fixedPos.y, transform.position.z);
            }
        }

        private bool EnsureMap()
        {
            if (_agent == null)
            {
                return false;
            }

            if (_agent.map != null)
            {
                return true;
            }

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

            _agent.map = map;
            if (_agent.map != null && _agent.map.nodesCount == 0)
            {
                _agent.map.GenerateMap();
            }

            if ((_agent.map == null || _agent.map.nodesCount == 0) && !mapWarningShown)
            {
                mapWarningShown = true;
                Debug.LogWarning("[FreightClerk] PolyNavMap not ready, movement disabled.");
            }
            return _agent.map != null && _agent.map.nodesCount > 0;
        }

        private int CountPickableProducts(ProductionStation station)
        {
            int count = 0;
            for (int i = 0; i < station.productionList.Count; i++)
            {
                var production = station.productionList[i];
                if (production == null) continue;
                if (production.isTaken) continue;
                if (!production.canPickup) continue;
                if (production.state != ItemState.OnWorkbench) continue;
                count++;
            }

            return count;
        }


        /// 查找有产品的生产台
        private ProductionStation FindValidProductionStation()
        {
            ProductionStation bestStation = null;
            int bestScore = int.MinValue;

            foreach (var ps in productionStationList)
            {
                if (ps == null)
                    continue;

                if (ps.isLock || ps.isCanUnlockState)
                    continue;

                if (ps.transferPoint == null)
                    continue;

                var stall = FindSalesStall(ps.goodsType);
                if (stall == null || stall.isLock || stall.isCanUnlockState)
                    continue;

                if (stall.transferPoint == null)
                    continue;

                int pickableCount = CountPickableProducts(ps);
                if (pickableCount <= 0)
                    continue;

                int workingCount = 0;
                StationWorkingClerkCount.TryGetValue(ps, out workingCount);

                // 根据可搬运商品数限制同站分配，避免过度预占。
                int capacityPerTrip = Mathf.Max(1, currentCapacity);
                int neededClerkCount = Mathf.Max(1, Mathf.CeilToInt(pickableCount * 1f / capacityPerTrip));
                if (workingCount >= neededClerkCount)
                {
                    continue;
                }

                // 已经有搬运工在服务的生产台优先级更高。
                int activeBonus = workingCount > 0 ? 1000 : 0;
                int quantityScore = pickableCount * 100;
                int distancePenalty = Mathf.RoundToInt(Vector2.Distance(transform.position, ps.transferPoint.position) * 5f);
                int workerPenalty = workingCount * 10;
                int score = activeBonus + quantityScore - distancePenalty - workerPenalty;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestStation = ps;
                }
            }

            if (bestStation != null)
            {
                int count = 0;
                StationWorkingClerkCount.TryGetValue(bestStation, out count);
                StationWorkingClerkCount[bestStation] = count + 1;
            }

            return bestStation;
        }

        /// <summary>
        /// 释放当前占用的生产台计数
        /// </summary>
        private void ReleaseStationReservation()
        {
            if (targetStation == null)
                return;

            int count;
            if (StationWorkingClerkCount.TryGetValue(targetStation, out count))
            {
                count--;
                if (count <= 0)
                {
                    StationWorkingClerkCount.Remove(targetStation);
                }
                else
                {
                    StationWorkingClerkCount[targetStation] = count;
                }
            }

            targetStation = null;
        }


        /// 根据商品类型找到摊位
        private SalesStall FindSalesStall(GoodsType type)
        {
            foreach (var stall in salesStallList)
            {
                if (stall == null)
                    continue;

                if (stall.currentGoodsType == type)
                    return stall;
            }
            return null;
        }

        private void Update()
        {
            if (_agent.hasPath)
            {
                PlayAnimationIfNotPlaying("walk");
            }
            else
            {
                PlayAnimationIfNotPlaying("idle");
            }
            SetLayer();

            Vector2 dir = _agent.movingDirection;
            if (dir.x < 0)
            {
                skeletonAnimation.skeleton.SetAttachment("衣服", "10");
                skeletonAnimation.transform.localScale = new Vector3(-0.6f, 0.6f, 0.6f);
            }
            else
            {
                skeletonAnimation.skeleton.SetAttachment("衣服", "10_2");
                skeletonAnimation.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            }
        }

        private void PlayAnimationIfNotPlaying(string animationName)
        {
            var state = skeletonAnimation.AnimationState;
            var current = state.GetCurrent(0);

            if (current == null || current.Animation == null || current.Animation.Name != animationName)
            {
                state.SetAnimation(0, animationName, true);
            }
        }
        public void SetLayer()
        {
            int newOrder = 30000 - Mathf.FloorToInt(transform.localPosition.y * 100);
            renderer.sortingOrder = newOrder;
            shadow.sortingOrder = newOrder - 1;
            foreach (var product in productList)
            {
                product.spriteRenderer.sortingOrder = renderer.sortingOrder + 1;
            }
        }

        void OnEnable()
        {
            _agent.OnDestinationReached += OnReachDestination;
            EventCenter.Instance.AddListener(EventMessages.UpdateYunDiZheSpeed, UpdateSpeed);
        }

        void OnDisable()
        {
            ReleaseStationReservation();
            _agent.OnDestinationReached -= OnReachDestination;
            EventCenter.Instance.RemoveListener(EventMessages.UpdateYunDiZheSpeed, UpdateSpeed);
        }
        void OnReachDestination()
        {

        }
    }
}
