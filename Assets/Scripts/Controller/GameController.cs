using System.Collections.Generic;
using Controller.Structure;
using Module.Data;
using UnityEngine;
using Utils;

namespace Controller
{
    public class GameController : MonoUtil<GameController>
    {
        [Header("每种怪物的出生点")]
        public Dictionary<MonsterType, Transform> monsterBornPositions = new ();

        [Header("地图中建筑信息")]
        public Dictionary<BuildingType, StructureBase> buildings = new ();

        [Header("商品类型对应的售卖摊位")]
        public Dictionary<GoodsType, StructureBase> goodBuild = new ();

        [Header("每个摊位的排队系统")]
        public Dictionary<StructureBase, QueueGroup> queues = new ();
        public Dictionary< MonsterType , FactoryController> factoryControllers = new();

        public Dictionary< MonsterType , MapLock> mapLockDic = new ();

        public int currentMapID = 1;

        private AssetHandle _assetHandle;
    
        public Transform RespawnPoint;
        
        /// <summary>
        /// 场景中的所有生产设施
        /// </summary>
        public List<ProductionStation> productionStationList = new();
        
        /// <summary>
        /// 场景中的所有售卖摊位
        /// </summary>
        public List<SalesStall> salesStallList = new();

        public List<BuildingType> unlockedBuildingTypes = new();
        

        public override void Awake()
        {
            base.Awake();
            _assetHandle = GetComponent<AssetHandle>();
            foreach (var temp in goodBuild.Values)
            {
                SalesStall  sale = temp as SalesStall;
                RegisterQueue(temp , new Vector2(sale.parchaseTransform.position.x, sale.parchaseTransform.position.y)  );
            }
        }

        private void Start()
        {
          //  var position = ((CashierCounter)buildings[BuildingType.LingZhangTai]).parchaseTransform.position;
           // RegisterQueue(buildings[BuildingType.LingZhangTai] , new Vector2(position.x, position.y));
        }

        /// <summary>
        /// 注册一个摊位（创建队伍系统）
        /// </summary>
        public void RegisterQueue(StructureBase building, Vector2 queueOrigin)
        {
            if (!queues.ContainsKey(building))
                queues.Add(building, new QueueGroup(queueOrigin));
        }

        
    }
    
    
    public class QueueGroup
    {
        public List<CustomerController> customers = new();
        private List<Vector2> queuePoints = new();

        private Vector2 origin; // 队伍起点（摊位前方的位置）

        private float vertical = 0.3f;     // 前后距离
        private float spreadFactor = 0.3f; // 左右扩散

        public QueueGroup(Vector2 origin)
        {
            this.origin = origin;
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