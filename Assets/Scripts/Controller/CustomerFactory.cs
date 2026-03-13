using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Controller.Structure;
using Module;
using Module.Data;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace Controller
{
    public class CustomerFactory : MonoBehaviour
    {
        private AssetHandle _assetHandle;
        public MapData mapData;
        public List<int> customerTypeList = new();
        public float spawnTime;
        public float spawnRadiusX = 3f;
        public float spawnRadiusY = 1.5f;

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

                obj.transform.position = GetRandomPosition();

                obj.GetComponent<CustomerController>()
                    .Init(tempData, goodsType, structure, transform.position, spawnRadiusX, spawnRadiusY);
                yield return null;
                Debug.Log($"生成顾客：{obj.name},目标结构：{structure.name}, 目标位置：{obj.GetComponent<CustomerController>().nextPosition}");
               
                yield return new WaitForSeconds(spawnTime);

            }
        }


        private Vector3 GetRandomPosition()
        {
            Vector3 origin = transform.position;
            var map = PolyNav.PolyNavMap.current;
            if (map == null)
            {
                var mapObj = GameObject.FindWithTag("Map");
                if (mapObj != null)
                {
                    map = mapObj.GetComponent<PolyNav.PolyNavMap>();
                }
                if (map == null)
                {
                    var mapObjByName = GameObject.Find("Map");
                    if (mapObjByName != null)
                    {
                        map = mapObjByName.GetComponent<PolyNav.PolyNavMap>();
                    }
                }
            }
            if (map != null && map.nodesCount == 0)
            {
                map.GenerateMap();
            }

            for (int i = 0; i < 8; i++)
            {
                Vector3 position = origin;
                position.x += Random.Range(-spawnRadiusX, spawnRadiusX);
                position.y += Random.Range(-spawnRadiusY, spawnRadiusY);

                if (map == null)
                {
                    return position;
                }

                Vector2 pos2 = new Vector2(position.x, position.y);
                if (map.PointIsValid(pos2))
                {
                    return position;
                }
            }

            Vector3 fallback = origin;
            fallback.x += Random.Range(-spawnRadiusX, spawnRadiusX);
            fallback.y += Random.Range(-spawnRadiusY, spawnRadiusY);
            if (map != null)
            {
                Vector2 pos2 = new Vector2(fallback.x, fallback.y);
                if (!map.PointIsValid(pos2))
                {
                    pos2 = map.GetCloserEdgePoint(pos2);
                }
                fallback = new Vector3(pos2.x, pos2.y, fallback.z);
            }
            return fallback;
        }



    }
}
