using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Controller.Structure;
using Module;
using Module.Data;
using UnityEngine;
using Utils;

namespace Controller
{
    [System.Serializable]
    public class CustomerRoute
    {
        public string routeName;
        public Transform spawnPoint;
        public List<Transform> waypoints = new();
    }

    public class CustomerFactory : MonoBehaviour
    {
        private AssetHandle _assetHandle;
        public MapData mapData;
        public List<int> customerTypeList = new();
        public float spawnTime;
        public List<CustomerRoute> routes = new();

        private const int MaxCustomerPerPlace = 5;
        private Coroutine createCustomerCoroutine;

        private void OnEnable()
        {
            Debug.Log($"CustomerFactory OnEnable: {GetInstanceID()}");
            EventCenter.Instance.AddListener(EventMessages.MapDataPrepared, HandleCustomerCreat);
        }

        private void OnDisable()
        {
            EventCenter.Instance.RemoveListener(EventMessages.MapDataPrepared, HandleCustomerCreat);
            if (createCustomerCoroutine != null)
            {
                StopCoroutine(createCustomerCoroutine);
                createCustomerCoroutine = null;
            }
        }

        private void Start()
        {
            _assetHandle = GetComponent<AssetHandle>();
        }


        public void HandleCustomerCreat(params object[] args)
        {
            if (GameController.Instance != null)
            {
                GameController.Instance.RefreshStructureCaches();
            }
            customerTypeList.Clear();
            mapData = DataController.Instance.mapDataDic[
               PlayerDataModule.Instance.data.currentMapID];

            customerTypeList = new List<int>(mapData.customerTypeList);
            if (createCustomerCoroutine != null)
            {
                StopCoroutine(createCustomerCoroutine);
                createCustomerCoroutine = null;
            }

            createCustomerCoroutine = StartCoroutine(CreatCustomer());
        }

        private BuildingType GetStallBuildingType(SalesStall stall)
        {
            if (stall == null) return BuildingType.None;
            return stall.buildingType != BuildingType.None ? stall.buildingType : stall.structureType;
        }

        bool IsStructureLocked(BuildingType buildingType)
        {
            if (buildingType == BuildingType.None)
            {
                return false;
            }
            var playerData = PlayerDataModule.Instance.data;
            bool unlockedByData = playerData.structUnLockDataDic[playerData.currentMapID].Contains(buildingType);
            bool unlockedByRuntime = GameController.Instance != null &&
                                     GameController.Instance.unlockedBuildingTypes.Contains(buildingType);
            return !(unlockedByData || unlockedByRuntime);
        }

        public int GetBestRouteIndex(StructureBase targetStructure)
        {
            Vector2 targetPosition = GetTargetPosition(targetStructure);
            int bestRouteIndex = -1;
            float bestDistanceSqr = float.MaxValue;

            for (int i = 0; i < routes.Count; i++)
            {
                if (!HasRouteData(routes[i]))
                {
                    continue;
                }

                Vector2 routeAnchor = GetRouteAnchorPosition(routes[i]);
                float distanceSqr = (routeAnchor - targetPosition).sqrMagnitude;
                if (distanceSqr < bestDistanceSqr)
                {
                    bestDistanceSqr = distanceSqr;
                    bestRouteIndex = i;
                }
            }

            return bestRouteIndex;
        }

        public Vector3 GetSpawnPositionForRoute(int routeIndex)
        {
            if (TryGetRoute(routeIndex, out var route))
            {
                if (route.spawnPoint != null)
                {
                    return route.spawnPoint.position;
                }

                int firstWaypointIndex = GetFirstValidWaypointIndex(route);
                if (firstWaypointIndex >= 0)
                {
                    return route.waypoints[firstWaypointIndex].position;
                }
            }

            return transform.position;
        }

        public bool TryBuildRoute(int routeIndex, out Vector2 routeStart, out List<Vector2> routeWaypoints)
        {
            routeStart = transform.position;
            routeWaypoints = new List<Vector2>();
            if (!TryGetRoute(routeIndex, out var route))
            {
                return false;
            }

            int waypointStartIndex = 0;
            if (route.spawnPoint != null)
            {
                routeStart = route.spawnPoint.position;
            }
            else
            {
                int firstWaypointIndex = GetFirstValidWaypointIndex(route);
                if (firstWaypointIndex >= 0)
                {
                    routeStart = route.waypoints[firstWaypointIndex].position;
                    waypointStartIndex = firstWaypointIndex + 1;
                }
            }

            for (int i = waypointStartIndex; i < route.waypoints.Count; i++)
            {
                var waypoint = route.waypoints[i];
                if (waypoint == null)
                {
                    continue;
                }

                routeWaypoints.Add(waypoint.position);
            }

            return route.spawnPoint != null || routeWaypoints.Count > 0;
        }

        private bool TryGetRoute(int routeIndex, out CustomerRoute route)
        {
            route = null;
            if (routeIndex < 0 || routeIndex >= routes.Count)
            {
                return false;
            }

            route = routes[routeIndex];
            return route != null;
        }

        private int GetFirstValidWaypointIndex(CustomerRoute route)
        {
            if (route == null || route.waypoints == null)
            {
                return -1;
            }

            for (int i = 0; i < route.waypoints.Count; i++)
            {
                if (route.waypoints[i] != null)
                {
                    return i;
                }
            }

            return -1;
        }

        private bool HasRouteData(CustomerRoute route)
        {
            if (route == null)
            {
                return false;
            }

            if (route.spawnPoint != null)
            {
                return true;
            }

            for (int i = 0; i < route.waypoints.Count; i++)
            {
                if (route.waypoints[i] != null)
                {
                    return true;
                }
            }

            return false;
        }

        private Vector2 GetRouteAnchorPosition(CustomerRoute route)
        {
            if (route == null)
            {
                return transform.position;
            }

            if (route.waypoints != null)
            {
                for (int i = route.waypoints.Count - 1; i >= 0; i--)
                {
                    if (route.waypoints[i] != null)
                    {
                        return route.waypoints[i].position;
                    }
                }
            }

            if (route.spawnPoint != null)
            {
                return route.spawnPoint.position;
            }

            return transform.position;
        }

        private Vector2 GetTargetPosition(StructureBase targetStructure)
        {
            if (targetStructure is SalesStall stall && stall.parchaseTransform != null)
            {
                return stall.parchaseTransform.position;
            }

            if (targetStructure != null)
            {
                return targetStructure.transform.position;
            }

            return transform.position;
        }

        public IEnumerator CreatCustomer()
        {
            yield return new WaitForSeconds(2f);
            while (true)
            {
                if (IsStructureLocked(BuildingType.LingZhangTai))
                {
                    yield return new WaitForSeconds(0.5f);
                    continue;
                }
                // 过滤出还能继续排队的地点
                var availableStructures = new List<KeyValuePair<GoodsType, StructureBase>>();
                var stalls = GameController.Instance.salesStallList;
                for (int i = 0; i < stalls.Count; i++)
                {
                    var stall = stalls[i];
                    if (stall == null) continue;
                    if (stall.currentGoodsType == GoodsType.None) continue;

                    var stallType = GetStallBuildingType(stall);
                    if (IsStructureLocked(stallType))
                        continue;

                    switch (stallType)
                    {
                        case BuildingType.LingChaJia_1:
                            if (IsStructureLocked(BuildingType.YuShaHu_1))
                                continue;
                            break;
                        case BuildingType.LingChaJia_2:
                            if (IsStructureLocked(BuildingType.YuShaHu_2))
                                continue;
                            break;
                        case BuildingType.LingChaJia_3:
                            if (IsStructureLocked(BuildingType.YuShaHu_3))
                                continue;
                            break;
                        case BuildingType.LingChaJia_4:
                            if (IsStructureLocked(BuildingType.YuShaHu_4))
                                continue;
                            break;

                        case BuildingType.LingQiJia_1:
                            if (IsStructureLocked(BuildingType.LianQiLu_1))
                                continue;
                            break;
                        case BuildingType.LingQiJia_2:
                            if (IsStructureLocked(BuildingType.LianQiLu_2))
                                continue;
                            break;
                        case BuildingType.LingQiJia_3:
                            if (IsStructureLocked(BuildingType.LianQiLu_3))
                                continue;
                            break;
                    }

                    availableStructures.Add(new KeyValuePair<GoodsType, StructureBase>(stall.currentGoodsType, stall));
                }

                // 如果所有点位都满了，不生成顾客
                if (availableStructures.Count == 0)
                {
                    yield return new WaitForSeconds(spawnTime);
                    continue;
                }

                // 随机一个可用点位
                var randomPair = availableStructures[Random.Range(0, availableStructures.Count)];
                GoodsType goodsType = randomPair.Key;
                StructureBase structure = randomPair.Value;

                // 随机顾客类型
                var tempData = DataController.Instance.customerDataDic[(CustomerType)Extensions.RandomOne(customerTypeList)];

                if (_assetHandle == null)
                {
                    _assetHandle = GetComponent<AssetHandle>();
                }
                // 生成顾客
                GameObject obj = Instantiate(_assetHandle.Get<GameObject>(Extensions.GetCustomerResNameByType(tempData.type)));

                int routeIndex = GetBestRouteIndex(structure);
                if (routeIndex < 0)
                {
                    yield return new WaitForSeconds(spawnTime);
                    continue;
                }

                Vector3 spawnPosition = GetSpawnPositionForRoute(routeIndex);
                obj.transform.position = spawnPosition;

                obj.GetComponent<CustomerController>()
                    .Init(tempData, goodsType, structure, spawnPosition, this, routeIndex);
                yield return null;
                Debug.Log($"生成顾客：{obj.name},目标结构：{structure.name}, 目标位置：{obj.GetComponent<CustomerController>().nextPosition}");
               
                yield return new WaitForSeconds(spawnTime);

            }
        }
    }
}
