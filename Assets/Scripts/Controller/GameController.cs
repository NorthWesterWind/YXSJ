using System;
using System.Collections.Generic;
using Controller.Structure;
using Module.Data;
using UnityEngine;
using Utils;

namespace Controller
{
    [Serializable]
    public class CollectorRouteConfig
    {
        public string routeName;
        public WarehouseCategoryType warehouseCategoryType;
        public MonsterFamily targetMonsterType;
        public List<Transform> waypoints = new();
    }

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

        [Header("Collector Routes")]
        public List<CollectorRouteConfig> collectorRoutes = new();

        public List<BuildingType> unlockedBuildingTypes = new();
        

        public override void Awake()
        {
            base.Awake();
            _assetHandle = GetComponent<AssetHandle>();
            RefreshStructureCaches();
            foreach (var temp in goodBuild.Values)
            {
                SalesStall  sale = temp as SalesStall;
                if (sale != null && sale.parchaseTransform != null)
                {
                    RegisterQueue(temp, new Vector2(sale.parchaseTransform.position.x, sale.parchaseTransform.position.y));
                }
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

        public void RefreshStructureCaches()
        {
            if (buildings == null) buildings = new Dictionary<BuildingType, StructureBase>();
            if (goodBuild == null) goodBuild = new Dictionary<GoodsType, StructureBase>();
            if (productionStationList == null) productionStationList = new List<ProductionStation>();
            if (salesStallList == null) salesStallList = new List<SalesStall>();

            buildings.Clear();
            goodBuild.Clear();
            productionStationList.Clear();
            salesStallList.Clear();

            var structures = FindObjectsOfType<StructureBase>(true);
            foreach (var structure in structures)
            {
                if (structure == null) continue;

                BuildingType buildingKey = structure.structureType;
                if (structure is SalesStall stall)
                {
                    if (stall.buildingType != BuildingType.None)
                    {
                        buildingKey = stall.buildingType;
                    }
                    if (!salesStallList.Contains(stall))
                    {
                        salesStallList.Add(stall);
                    }
                    if (stall.currentGoodsType != GoodsType.None &&
                        !goodBuild.ContainsKey(stall.currentGoodsType))
                    {
                        goodBuild.Add(stall.currentGoodsType, stall);
                    }
                }
                else if (structure is ProductionStation station)
                {
                    if (station.buildingType != BuildingType.None)
                    {
                        buildingKey = station.buildingType;
                    }
                    if (!productionStationList.Contains(station))
                    {
                        productionStationList.Add(station);
                    }
                }
                else if (structure is YunDiGeController yundi)
                {
                    if (yundi.buildingType != BuildingType.None)
                    {
                        buildingKey = yundi.buildingType;
                    }
                }

                if (buildingKey != BuildingType.None && !buildings.ContainsKey(buildingKey))
                {
                    buildings.Add(buildingKey, structure);
                }
            }
        }

        public bool TryBuildCollectorRoute(WarehouseCategoryType warehouseCategoryType, MonsterFamily targetMonsterType, out List<Vector2> routeWaypoints)
        {
            routeWaypoints = new List<Vector2>();
            if (targetMonsterType == MonsterFamily.None || collectorRoutes == null || collectorRoutes.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < collectorRoutes.Count; i++)
            {
                var route = collectorRoutes[i];
                if (route == null ||
                    route.warehouseCategoryType != warehouseCategoryType ||
                    route.targetMonsterType != targetMonsterType)
                {
                    continue;
                }

                if (route.waypoints == null)
                {
                    return false;
                }

                for (int j = 0; j < route.waypoints.Count; j++)
                {
                    var waypoint = route.waypoints[j];
                    if (waypoint == null)
                    {
                        continue;
                    }

                    routeWaypoints.Add(waypoint.position);
                }

                return routeWaypoints.Count > 0;
            }

            return false;
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
