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
        private PolyNavAgent _agent;
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

        /// <summary>
        /// 静态字典：记录每个生产台当前有多少搬运工在“前往 / 服务”
        /// 用来尽量避免多个搬运工一起跑向同一个生产台
        /// </summary>
        private static readonly Dictionary<ProductionStation, int> StationWorkingClerkCount =
            new Dictionary<ProductionStation, int>();

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
            // 初始化导航
            _agent.map = GameObject.Find("Map").transform.GetComponent<PolyNavMap>();
            normalPos = productionStationList[0].transferPoint;
            _agent.SetDestination(normalPos.position);
            _agent.maxSpeed = PlayerDataModule.Instance.data.deliverData.currentMoveSpeed;
            StartCoroutine(WorkerLoop());
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

                // 拿货
                productList = targetStation.TakeProduct(this);

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


                yield return new WaitForSeconds(1f );

            }
        }
        private IEnumerator MoveTo(Vector2 target)
        {
            _agent.SetDestination(target);
            while (_agent.hasPath && _agent.remainingDistance > 1f)
                yield return null;
        }


        /// 查找有产品的生产台
        private ProductionStation FindValidProductionStation()
        {
            ProductionStation bestStation = null;
            int bestWorkerCount = int.MaxValue;

            foreach (var ps in productionStationList)
            {
                if (ps.isLock || ps.isCanUnlockState)
                    continue;

                var stall = FindSalesStall(ps.goodsType);
                if (stall == null || stall.isLock || stall.isCanUnlockState)
                    continue;

                if (ps.productionList.Count <= 0)
                    continue;

                // 当前已有多少搬运工在服务这个生产台
                int workingCount = 0;
                StationWorkingClerkCount.TryGetValue(ps, out workingCount);

                // 选择“已有搬运工最少”的生产台，尽量分散
                if (workingCount < bestWorkerCount)
                {
                    bestWorkerCount = workingCount;
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
        }

        void OnEnable()
        {
            _agent.OnDestinationReached += OnReachDestination;
        }

        void OnDisable()
        {
            _agent.OnDestinationReached -= OnReachDestination;
        }
        void OnReachDestination()
        {

        }
    }
}
