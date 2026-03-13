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
            if ((productionStationList == null || productionStationList.Count == 0) &&
                GameController.Instance != null)
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
                    _agent.SetDestination(ClampToNav(normalPos.position));
                }
            }
            _agent.maxSpeed = WorldData.speedLevelDic[deliverData.speedLevel];
            StartCoroutine(WorkerLoop());
        }

        private void ResolveIdlePosition()
        {
            if (normalPos != null)
            {
                return;
            }

            if (productionStationList != null)
            {
                for (int i = 0; i < productionStationList.Count; i++)
                {
                    var station = productionStationList[i];
                    if (station == null) continue;
                    if (station.transferPoint != null)
                    {
                        normalPos = station.transferPoint;
                        return;
                    }
                    normalPos = station.transform;
                    return;
                }
            }

            var fallbackStation = FindObjectOfType<ProductionStation>();
            if (fallbackStation != null)
            {
                normalPos = fallbackStation.transferPoint != null
                    ? fallbackStation.transferPoint
                    : fallbackStation.transform;
            }
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
                    if ((productionStationList == null || productionStationList.Count == 0 ||
                         salesStallList == null || salesStallList.Count == 0) &&
                        GameController.Instance != null)
                    {
                        GameController.Instance.RefreshStructureCaches();
                        productionStationList = GameController.Instance.productionStationList;
                        salesStallList = GameController.Instance.salesStallList;
                    }
                    // 没有任何生产台有产品，让搬运工回到待命点 normalPos 原地待命
                    if (normalPos != null)
                    {
                        yield return MoveTo(normalPos.position);
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
                        yield return MoveTo(normalPos.position);
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
                        yield return MoveTo(normalPos.position);
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
                if (ps.isLock || ps.isCanUnlockState)
                    continue;

                var stall = FindSalesStall(ps.goodsType);
                if (stall == null || stall.isLock || stall.isCanUnlockState)
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
            if (dir == Vector2.zero) return;
            if (Mathf.Abs(dir.x) > 0.01f)
            {
                skeletonAnimation.skeleton.ScaleX = dir.x < 0 ? -1 : 1;
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
            _agent.OnDestinationReached -= OnReachDestination;
            EventCenter.Instance.RemoveListener(EventMessages.UpdateYunDiZheSpeed, UpdateSpeed);
        }
        void OnReachDestination()
        {

        }
    }
}
