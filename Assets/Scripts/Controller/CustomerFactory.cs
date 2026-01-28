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

        bool IsStructureUnlocked(BuildingType buildingType)
        {
            var playerData = PlayerDataModule.Instance.data;
            return !playerData.structUnLockDataDic[playerData.currentMapID].Contains(buildingType);
        }

        public IEnumerator CreatCustomer()
        {
            yield return new WaitForSeconds(2f);
            while (true)
            {
                if (!PlayerDataModule.Instance.data.structUnLockDataDic[PlayerDataModule.Instance.data.currentMapID].Contains(BuildingType.LingZhangTai))
                {
                    yield return new WaitForSeconds(0.5f);
                    continue;
                }
                // 过滤出还能继续排队的地点
                var availableStructures = GameController.Instance.goodBuild
                    .Where(pair =>
                    {
                        if (IsStructureUnlocked((pair.Value as SalesStall).buildingType))
                            return false;
                        switch ((pair.Value as SalesStall).buildingType)
                        {
                            case BuildingType.LingChaJia_1:
                                if (IsStructureUnlocked(BuildingType.YuShaHu_1))
                                    return false;
                                break;
                            case BuildingType.LingChaJia_2:
                                if (IsStructureUnlocked(BuildingType.YuShaHu_2))
                                    return false;
                                break;
                            case BuildingType.LingChaJia_3:
                                if (IsStructureUnlocked(BuildingType.YuShaHu_3))
                                    return false;
                                break;
                            case BuildingType.LingChaJia_4:
                                if (IsStructureUnlocked(BuildingType.YuShaHu_4))
                                    return false;
                                break;

                            case BuildingType.LingQiJia_1:
                                if (IsStructureUnlocked(BuildingType.LianQiLu_1))
                                    return false;
                                break;
                            case BuildingType.LingQiJia_2:
                                if (IsStructureUnlocked(BuildingType.LianQiLu_2))
                                    return false;
                                break;
                            case BuildingType.LingQiJia_3:
                                if (IsStructureUnlocked(BuildingType.LianQiLu_3))
                                    return false;
                                break;
                        }
                        return true;
                    })
                    .ToList();

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

                obj.GetComponent<CustomerController>().Init(tempData, goodsType, structure);
                yield return null;
                Debug.Log($"生成顾客：{obj.name},目标结构：{structure.name}, 目标位置：{obj.GetComponent<CustomerController>().nextPosition}");
               
                yield return new WaitForSeconds(spawnTime);

            }
        }


        private Vector3 GetRandomPosition()
        {
            Vector3 position = transform.position;
            position.x += Random.Range(-3f, 3f);
            return position;
        }



    }
}