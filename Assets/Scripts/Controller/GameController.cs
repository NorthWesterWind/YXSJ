using System.Collections.Generic;
using Controller.Structure;
using Module.Data;
using UnityEngine;
using Utils;

namespace Controller
{
    public class GameController : MonoSingleton<GameController>
    {
        [Header("每个地图的出生点")]
        public Dictionary<MonsterType, Transform> monsterBornPositions = new ();

        [Header("地图中建筑信息")]
        public Dictionary<BuildingType, StructureBase> buildings = new ();

        [Header("商品类型对应的售卖摊位")]
        public Dictionary<GoodsType, StructureBase> goodBuild = new ();

        [Header("每个摊位的排队系统")]
        public Dictionary<StructureBase, QueueGroup> queues = new ();
        public Dictionary< MonsterType , FactoryController> factoryControllers = new();

        public int currentMapID = 1;

        private AssetHandle _assetHandle;
        public GameObject obj;
        public MonsterData monsterData;
        
        /// <summary>
        /// 场景中的所有生产设施
        /// </summary>
        public List<ProductionStation> productionStationList = new();
        
        /// <summary>
        /// 场景中的所有售卖摊位
        /// </summary>
        public List<SalesStall> salesStallList = new();

        public override void Awake()
        {
            base.Awake();
            monsterBornPositions = new ();
            buildings = new ();
            goodBuild = new ();
            queues = new ();
        }

        private void Start()
        {
            _assetHandle = GetComponent<AssetHandle>();
            foreach (var temp in goodBuild.Values)
            {
                SalesStall  sale = temp as SalesStall;
                RegisterQueue(temp , new Vector2(sale.parchaseTransform.position.x, sale.parchaseTransform.position.y)  );
            }
            var position = ((CashierCounter)buildings[BuildingType.LingZhangTai]).parchaseTransform.position;
            RegisterQueue(buildings[BuildingType.LingZhangTai] , new Vector2(position.x, position.y));
        }

        /// <summary>
        /// 注册一个摊位（创建队伍系统）
        /// </summary>
        public void RegisterQueue(StructureBase building, Vector2 queueOrigin)
        {
            if (!queues.ContainsKey(building))
                queues.Add(building, new QueueGroup(queueOrigin));
        }

        /// <summary>
        /// 顾客加入队伍，返回其排队点位
        /// </summary>
        public Vector2 AddCustomerToQueue(StructureBase building, CustomerController customer)
        {
            return queues[building].AddCustomer(customer);
        }

        /// <summary>
        /// 顾客离开队伍
        /// </summary>
        public void RemoveCustomerFromQueue(StructureBase building, CustomerController customer)
        {
            queues[building].RemoveCustomer(customer);
        }
    }
    
    
    public class QueueGroup
    {
        private List<CustomerController> customers = new();
        private List<Vector2> queuePoints = new();

        private Vector2 origin; // 队伍起点（摊位前方的位置）

        private float vertical = 1.2f;     // 前后距离
        private float spreadFactor = 0.2f; // 左右扩散

        public QueueGroup(Vector2 origin)
        {
            this.origin = origin;
        }

        /// 顾客加入队伍
        public Vector2 AddCustomer(CustomerController customer)
        {
            customers.Add(customer);
            RebuildPoints();
            return queuePoints[customers.Count - 1];
        }

        /// 顾客离开队伍
        public void RemoveCustomer(CustomerController customer)
        { 
            // 1. 空保护
            if (customer == null) 
            {
                Debug.LogWarning("RemoveCustomer: customer is null");
                return;
            }

            // 2. 队列为空保护
            if (customers.Count == 0)
            {
                Debug.LogWarning("RemoveCustomer: customers is empty");
                return;
            }

            // 3. 不在队列中保护
            int index = customers.IndexOf(customer);
            if (index < 0)
            {
                Debug.LogWarning("RemoveCustomer: customer not in list");
                return;
            }

            // 4. 删除
            customers.RemoveAt(index);

            // 5. 重新构建队列点
            RebuildPoints();

            // 6. 队列点数量不足保护
            if (queuePoints.Count < customers.Count)
            {
                Debug.LogError($"queuePoints 不够！queuePoints: {queuePoints.Count} customers: {customers.Count}");
                return;
            }

            // 7. 安全更新所有顾客目标点
            for (int i = 0; i < customers.Count; i++)
            {
                if (customers[i] == null)
                {
                    Debug.LogWarning($"Customer {i} is null in customers list.");
                    continue;
                }

                customers[i].UpdateQueueTarget(queuePoints[i]);
            }
        }

        /// 重建队伍点位（核心）
        private void RebuildPoints()
        {
            queuePoints.Clear();

            for (int i = 0; i < customers.Count; i++)
            {
                float y = origin.y - i * vertical;
                float xOff = i * spreadFactor;
                float x = origin.x + ((i % 2 == 0) ? xOff : -xOff);

                queuePoints.Add(new Vector2(x, y));
            }
        }
    }
    
}