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

        private Dictionary<StructureBase, int> placeCustomerCount = new();
        private const int MaxCustomerPerPlace = 5;
        private Coroutine createCustomerCoroutine;

        private void OnEnable()
        {
            Debug.Log($"CustomerFactory OnEnable: {GetInstanceID()}");
            EventCenter.Instance.AddListener(EventMessages.MapDataPrepared, HandleCustomerCreat);
            EventCenter.Instance.AddListener(EventMessages.CustomerLeave, OnCustomerLeft);
        }

        private void OnDisable()
        {
            EventCenter.Instance.RemoveListener(EventMessages.MapDataPrepared, HandleCustomerCreat);
            EventCenter.Instance.RemoveListener(EventMessages.CustomerLeave, OnCustomerLeft);

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

            customerTypeList = mapData.customerTypeList;
            if (createCustomerCoroutine != null)
            {
                StopCoroutine(createCustomerCoroutine);
                createCustomerCoroutine = null;
            }

            createCustomerCoroutine = StartCoroutine(CreatCustomer());
        }

        bool IsStructureUnlocked(BuildingType buildingType)
        {
            var playerData =PlayerDataModule.Instance.data;
            return !playerData.structUnLockDataDic[playerData.currentMapID].Contains(buildingType);
        }

        public IEnumerator CreatCustomer()
        {
            while (true)
            {
                // 过滤出还能继续排队的地点
                var availableStructures = GameController.Instance.goodBuild
                    .Where(pair =>
                    {
                        if (IsStructureUnlocked((pair.Value as SalesStall).buildingType))
                            return false;
                        if (!placeCustomerCount.ContainsKey(pair.Value))
                            return true;

                        return placeCustomerCount[pair.Value] < MaxCustomerPerPlace;
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

                // 生成顾客
                GameObject obj = Instantiate(_assetHandle.Get<GameObject>(Extensions.GetCustomerResNameByType(tempData.type)));

                obj.transform.position = GetRandomPosition();
                obj.GetComponent<CustomerController>().Init(tempData, goodsType, structure);

                // 记录该地点顾客数量+1
                if (!placeCustomerCount.ContainsKey(structure))
                    placeCustomerCount[structure] = 0;

                placeCustomerCount[structure]++;

                yield return new WaitForSeconds(spawnTime);
            }
        }


        private Vector3 GetRandomPosition()
        {
            Vector3 position = transform.position;
            position.x += Random.Range(-5f, 5f);
            return position;
        }


        /// <summary>
        /// 是否还能向该地点派遣顾客
        /// </summary>
        public bool CanDispatchCustomer(StructureBase place)
        {
            if (place == null)
                return false;

            if (!placeCustomerCount.ContainsKey(place))
                placeCustomerCount[place] = 0;

            return placeCustomerCount[place] < MaxCustomerPerPlace;
        }


        /// <summary>
        /// 外部通知：某个地点顾客离开了（-1）
        /// </summary>
        public void OnCustomerLeft(params object[] args)
        {
            StructureBase place = (StructureBase)args[0];
            if (place == null)
                return;

            if (!placeCustomerCount.ContainsKey(place))
                return;

            placeCustomerCount[place] = Mathf.Max(0, placeCustomerCount[place] - 1);
        }

        /// <summary>
        /// （可选）获取当前可派遣顾客的所有地点
        /// </summary>
        public List<StructureBase> GetAvailablePlaces()
        {
            return GameController.Instance.goodBuild
                .Select(kv => kv.Value)
                .Where(CanDispatchCustomer)
                .ToList();
        }
    }
}