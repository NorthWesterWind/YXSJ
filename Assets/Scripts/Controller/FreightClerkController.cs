using System;
using System.Collections;
using System.Collections.Generic;
using Controller.Pickups;
using Controller.Structure;
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
        public int currentMove;
        public int pickUpRange;
        private AssetHandle _assetHandle;
    
        
        public List<Transform> points = new List<Transform>();
        public List<ProductionStation> productionStationList = new List<ProductionStation>();
        public List<SalesStall> salesStallList = new List<SalesStall>();
        public Transform normalPos;
        public List<Production>  productList = new List<Production>();
        
        private ProductionStation targetStation; // 当前目标生产台
        private SalesStall targetStall;          // 对应销售摊位
        private bool isWorking;
        
        public void Init()
        {
            if (_assetHandle == null)
            {
                _assetHandle = GetComponent<AssetHandle>();
            }

            productionStationList = GameController.Instance.productionStationList;
            salesStallList = GameController.Instance.salesStallList;
            _agent.map = GameObject.Find("Map").transform.GetComponent<PolyNavMap>();
            normalPos = productionStationList[0].transferPoint;
            _agent.SetDestination(normalPos.position);
            _agent.maxSpeed = currentMove;
            StartCoroutine(WorkerLoop());
        }
        
        
        
        private IEnumerator WorkerLoop()
        {
            yield return null;

            while (true)
            {
                // 查找目标生产台
                targetStation = FindValidProductionStation();
                if (targetStation == null)
                {
                    // 没有任何生产台有产品，等一会再查找
                    yield return new WaitForSeconds(1f);
                    continue;
                }

                // 去取货
                yield return MoveTo(targetStation.transferPoint.position);

                // 抵达后再次检查是否有货
                if (targetStation.productionList.Count <1)
                    continue;

                // 拿货
                productList = targetStation.TakeProduct(this);
                    
                yield return new WaitForSeconds(0.5f + productList.Count);
                
                // 找到对应该商品的销售摊位
                targetStall = FindSalesStall(targetStation.goodsType);

                if (targetStall == null)
                {
                    Debug.LogError("没有找到对应商品的摊位！");
                    continue;
                }

                // 送货
                yield return MoveTo(targetStall.transferPoint.position);

                // 放下商品
                targetStall.ReceiveProduct(this);
                
                currentCapacity = 0;
                
            }
        }
        private IEnumerator MoveTo(Vector2 target)
        {
            _agent.SetDestination(target);
            while (_agent.hasPath  && _agent.remainingDistance > 1f)
                yield return null;
        }
        
        
        /// 查找有产品的生产台
        private ProductionStation FindValidProductionStation()
        {
            foreach (var ps in productionStationList)
            {
                if (ps.productionList.Count>0)
                    return ps;
            }
            return null;
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
               var state = skeletonAnimation.AnimationState;
                var current = state.GetCurrent(0);

                if (current == null || current.Animation.Name != "walk")
                {
                    
                    state.SetAnimation(0, "walk", true);
                }
              
            }
            else
            {
                 var state = skeletonAnimation.AnimationState;
                var current = state.GetCurrent(0);

                if (current == null || current.Animation.Name != "idle")
                {
                    
                    state.SetAnimation(0, "walk", true);
                }
            }
            SetLayer();
        }
        public void SetLayer()
        {
            int newOrder = 3000 - Mathf.FloorToInt(transform.localPosition.y);
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
