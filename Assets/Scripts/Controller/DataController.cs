using System.Collections.Generic;
using System.Threading.Tasks;
using Module;
using Module.Data;
using Newtonsoft.Json;
using UnityEngine;
using Utils;

namespace Controller
{
    public class DataController : MonoSingleton<DataController>
    {
        public Dictionary<MonsterType, MonsterData> monsterDataDic = new();
        public Dictionary<CustomerType, CustomerData> customerDataDic = new();
        public Dictionary<int, MapData> mapDataDic = new();
        public Dictionary<int, RewardData> taskRewardDataDic = new();
        public Dictionary<int, StotageBagData> storageBagDataDic = new();
        public Dictionary<int, WeaponData> weaponDataDic = new();
        public Dictionary<int, TaskData> mapTaskDataDic1 = new(); // 30 
        public Dictionary<int, TaskData> mapTaskDataDic2 = new(); // 60
        public Dictionary<int, TaskData> mapTaskDataDic3 = new(); //90
        public Dictionary<int, TaskData> mapTaskDataDic4 = new(); //100
        public Dictionary<int, TaskData> mapTaskDataDic5 = new(); //110

        public Dictionary<int, SevenDayRewardData> sevenDayRewardDataDic = new();
        public Dictionary<int, TalentData> talentDataDic = new();
        public List<CardLevelData> cardLevelDataList = new();
        public Dictionary<int, GiftpackData> giftpackDataDic = new();
        public Dictionary<int, OrderData> orderDataDic = new Dictionary<int, OrderData>();

        public List<MapLockData> mapLockDataList_1 = new List<MapLockData>();
        public List<MapLockData> mapLockDataList_2 = new List<MapLockData>();
        public List<MapLockData> mapLockDataList_3 = new List<MapLockData>();
        public List<MapLockData> mapLockDataList_4 = new List<MapLockData>();
        public List<MapLockData> mapLockDataList_5 = new List<MapLockData>();


        public List<StructureLockData> structureLockDataList_1 = new List<StructureLockData>();
        public List<StructureLockData> structureLockDataList_2 = new List<StructureLockData>();
        public List<StructureLockData> structureLockDataList_3 = new List<StructureLockData>();
        public List<StructureLockData> structureLockDataList_4 = new List<StructureLockData>();
        public List<StructureLockData> structureLockDataList_5 = new List<StructureLockData>();

        async void Start()
        {
            await PrepareData();
        }


        void Update()
        {

        }
        public List<StructureLockData> GetStructureLockList(int mapID)
        {
            switch (mapID)
            {
                case 1:
                    return structureLockDataList_1;
                case 2:
                    return structureLockDataList_2;
                case 3:
                    return structureLockDataList_3;
                case 4:
                    return structureLockDataList_4;
                case 5:
                    return structureLockDataList_5;
                default:
                    return null;
            }
        }


        private async Task PrepareData()
        {
            string monstetStr = (await ResourceLoader.Instance.LoadAssetAsync<TextAsset>("MonsterData")).text;
            monsterDataDic.Clear();
            monsterDataDic = JsonConvert.DeserializeObject<Dictionary<MonsterType, MonsterData>>(monstetStr);


            string customerStr = (await ResourceLoader.Instance.LoadAssetAsync<TextAsset>("CustomerData")).text;
            customerDataDic.Clear();
            customerDataDic = JsonConvert.DeserializeObject<Dictionary<CustomerType, CustomerData>>(customerStr);


            string mapStr = (await ResourceLoader.Instance.LoadAssetAsync<TextAsset>("MapData")).text;
            mapDataDic?.Clear();
            mapDataDic = JsonConvert.DeserializeObject<Dictionary<int, MapData>>(mapStr);


            string rewardStr = (await ResourceLoader.Instance.LoadAssetAsync<TextAsset>("RewardData")).text;
            taskRewardDataDic.Clear();
            taskRewardDataDic = JsonConvert.DeserializeObject<Dictionary<int, RewardData>>(rewardStr);

            string storageStr = (await ResourceLoader.Instance.LoadAssetAsync<TextAsset>("StorageBagData")).text;
            storageBagDataDic.Clear();
            storageBagDataDic = JsonConvert.DeserializeObject<Dictionary<int, StotageBagData>>(storageStr);

            string weaponStr = (await ResourceLoader.Instance.LoadAssetAsync<TextAsset>("WeaponData")).text;
            weaponDataDic.Clear();
            weaponDataDic = JsonConvert.DeserializeObject<Dictionary<int, WeaponData>>(weaponStr);

            string taskStr1 = (await ResourceLoader.Instance.LoadAssetAsync<TextAsset>("TaskData_1")).text;
            mapTaskDataDic1.Clear();
            mapTaskDataDic1 = JsonConvert.DeserializeObject<Dictionary<int, TaskData>>(taskStr1);

            string taskStr2 = (await ResourceLoader.Instance.LoadAssetAsync<TextAsset>("TaskData_2")).text;
            mapTaskDataDic2.Clear();
            mapTaskDataDic2 = JsonConvert.DeserializeObject<Dictionary<int, TaskData>>(taskStr2);

            string taskStr3 = (await ResourceLoader.Instance.LoadAssetAsync<TextAsset>("TaskData_3")).text;
            mapTaskDataDic3.Clear();
            mapTaskDataDic3 = JsonConvert.DeserializeObject<Dictionary<int, TaskData>>(taskStr3);

            string taskStr4 = (await ResourceLoader.Instance.LoadAssetAsync<TextAsset>("TaskData_4")).text;
            mapTaskDataDic4.Clear();
            mapTaskDataDic4 = JsonConvert.DeserializeObject<Dictionary<int, TaskData>>(taskStr4);

            string taskStr5 = (await ResourceLoader.Instance.LoadAssetAsync<TextAsset>("TaskData_5")).text;
            mapTaskDataDic5.Clear();
            mapTaskDataDic5 = JsonConvert.DeserializeObject<Dictionary<int, TaskData>>(taskStr5);

            string sevenDataStr = (await ResourceLoader.Instance.LoadAssetAsync<TextAsset>("SevenDayReward")).text;
            sevenDayRewardDataDic.Clear();
            sevenDayRewardDataDic = JsonConvert.DeserializeObject<Dictionary<int, SevenDayRewardData>>(sevenDataStr);

            string talentDataStr = (await ResourceLoader.Instance.LoadAssetAsync<TextAsset>("TalentData")).text;
            talentDataDic.Clear();
            talentDataDic = JsonConvert.DeserializeObject<Dictionary<int, TalentData>>(talentDataStr);

            string cardDataStr = (await ResourceLoader.Instance.LoadAssetAsync<TextAsset>("CardLevelData")).text;
            cardLevelDataList.Clear();
            cardLevelDataList = JsonConvert.DeserializeObject<List<CardLevelData>>(cardDataStr);

            string giftpackDataStr = (await ResourceLoader.Instance.LoadAssetAsync<TextAsset>("GiftpackData")).text;
            giftpackDataDic.Clear();
            giftpackDataDic = JsonConvert.DeserializeObject<Dictionary<int, GiftpackData>>(giftpackDataStr);

            string orderDataStr = (await ResourceLoader.Instance.LoadAssetAsync<TextAsset>("OrderData")).text;
            orderDataDic.Clear();
            orderDataDic = JsonConvert.DeserializeObject<Dictionary<int, OrderData>>(orderDataStr);

            string mapLockDataStr1 = (await ResourceLoader.Instance.LoadAssetAsync<TextAsset>("MapLock_1")).text;
            mapLockDataList_1.Clear();
            mapLockDataList_1 = JsonConvert.DeserializeObject<List<MapLockData>>(mapLockDataStr1);

            string mapLockDataStr2 = (await ResourceLoader.Instance.LoadAssetAsync<TextAsset>("MapLock_2")).text;
            mapLockDataList_2.Clear();
            mapLockDataList_2 = JsonConvert.DeserializeObject<List<MapLockData>>(mapLockDataStr2);

            string mapLockDataStr3 = (await ResourceLoader.Instance.LoadAssetAsync<TextAsset>("MapLock_3")).text;
            mapLockDataList_3.Clear();
            mapLockDataList_3 = JsonConvert.DeserializeObject<List<MapLockData>>(mapLockDataStr3);

            string mapLockDataStr4 = (await ResourceLoader.Instance.LoadAssetAsync<TextAsset>("MapLock_4")).text;
            mapLockDataList_4.Clear();
            mapLockDataList_4 = JsonConvert.DeserializeObject<List<MapLockData>>(mapLockDataStr4);

            string mapLockDataStr5 = (await ResourceLoader.Instance.LoadAssetAsync<TextAsset>("MapLock_5")).text;
            mapLockDataList_5.Clear();
            mapLockDataList_5 = JsonConvert.DeserializeObject<List<MapLockData>>(mapLockDataStr5);


            string structureLockDataStr1 = (await ResourceLoader.Instance.LoadAssetAsync<TextAsset>("StructureLockData_1")).text;
            structureLockDataList_1.Clear();
            structureLockDataList_1 = JsonConvert.DeserializeObject<List<StructureLockData>>(structureLockDataStr1);


            string structureLockDataStr2 = (await ResourceLoader.Instance.LoadAssetAsync<TextAsset>("StructureLockData_2")).text;
            structureLockDataList_2.Clear();
            structureLockDataList_2 = JsonConvert.DeserializeObject<List<StructureLockData>>(structureLockDataStr2);



            string structureLockDataStr3 = (await ResourceLoader.Instance.LoadAssetAsync<TextAsset>("StructureLockData_3")).text;
            structureLockDataList_3.Clear();
            structureLockDataList_3 = JsonConvert.DeserializeObject<List<StructureLockData>>(structureLockDataStr3);


            string structureLockDataStr4 = (await ResourceLoader.Instance.LoadAssetAsync<TextAsset>("StructureLockData_4")).text;
            structureLockDataList_4.Clear();
            structureLockDataList_4 = JsonConvert.DeserializeObject<List<StructureLockData>>(structureLockDataStr4);




            string structureLockDataStr5 = (await ResourceLoader.Instance.LoadAssetAsync<TextAsset>("StructureLockData_5")).text;
            structureLockDataList_5.Clear();
            structureLockDataList_5 = JsonConvert.DeserializeObject<List<StructureLockData>>(structureLockDataStr5);


        }

        /// <summary>
        /// 更新建筑解锁信息
        /// </summary>
        /// <param name="mapId"></param>
        /// <param name="structureLockDataList"></param>
        public void UpdateStructureLockInfo()
        {
            ///根据当前解锁的任务数据  进行可解锁建筑数据划分
            PlayerData playerData = PlayerDataModule.Instance.data;
            for (int i = 0; i < structureLockDataList_1.Count; i++)
            {
                var list1 = playerData.structLockDataDic[1];
                var list2 = playerData.structUnLockDataDic[1];
                var list3 = playerData.structCanUnLockDataDic[1];
                if (list3.Contains(structureLockDataList_1[i].buildingType) || list2.Contains(structureLockDataList_1[i].buildingType))
                {
                    continue;
                }
                if (list1.Contains(structureLockDataList_1[i].buildingType))
                {
                    continue;
                }
                list1.Add(structureLockDataList_1[i].buildingType);
            }
            for (int i = 0; i < structureLockDataList_2.Count; i++)
            {
                var list1 = playerData.structLockDataDic[2];
                var list2 = playerData.structUnLockDataDic[2];
                var list3 = playerData.structCanUnLockDataDic[2];
                if (list3.Contains(structureLockDataList_2[i].buildingType) || list2.Contains(structureLockDataList_2[i].buildingType))
                {
                    continue;
                }
                if (list1.Contains(structureLockDataList_2[i].buildingType))
                {
                    continue;
                }
                list1.Add(structureLockDataList_2[i].buildingType);
            }
            for (int i = 0; i < structureLockDataList_3.Count; i++)
            {
                var list1 = playerData.structLockDataDic[3];
                var list2 = playerData.structUnLockDataDic[3];
                var list3 = playerData.structCanUnLockDataDic[3];
                if (list3.Contains(structureLockDataList_3[i].buildingType) || list2.Contains(structureLockDataList_3[i].buildingType))
                {
                    continue;
                }
                if (list1.Contains(structureLockDataList_3[i].buildingType))
                {
                    continue;
                }
                list1.Add(structureLockDataList_3[i].buildingType);
            }
            for (int i = 0; i < structureLockDataList_4.Count; i++)
            {
                var list1 = playerData.structLockDataDic[4];
                var list2 = playerData.structUnLockDataDic[4];
                var list3 = playerData.structCanUnLockDataDic[4];
                if (list3.Contains(structureLockDataList_4[i].buildingType) || list2.Contains(structureLockDataList_4[i].buildingType))
                {
                    continue;
                }
                if (list1.Contains(structureLockDataList_4[i].buildingType))
                {
                    continue;
                }
                list1.Add(structureLockDataList_4[i].buildingType);
            }
            for (int i = 0; i < structureLockDataList_5.Count; i++)
            {
                var list1 = playerData.structLockDataDic[5];
                var list2 = playerData.structUnLockDataDic[5];
                var list3 = playerData.structCanUnLockDataDic[5];
                if (list3.Contains(structureLockDataList_5[i].buildingType) || list2.Contains(structureLockDataList_5[i].buildingType))
                {
                    continue;
                }
                if (list1.Contains(structureLockDataList_5[i].buildingType))
                {
                    continue;
                }
                list1.Add(structureLockDataList_5[i].buildingType);
            }

            //已解锁建筑物，进行建筑数据判断
            switch (playerData.currentMapID)
            {
                case 1:
                    var list1 = playerData.structUnLockDataDic[1];
                    FillStructureData(list1);
                    break;
                case 2:
                    var list2 = playerData.structUnLockDataDic[2];
                    FillStructureData(list2);
                    break;
                case 3:
                    var list3 = playerData.structUnLockDataDic[3];
                    FillStructureData(list3);
                    break;
                case 4:
                    var list4 = playerData.structUnLockDataDic[4];
                    FillStructureData(list4);
                    break;
                case 5:
                    var list5 = playerData.structUnLockDataDic[5];
                    break;
            }


            List<TaskData> taskDatas = playerData.listenInTaskList;
            for (int i = 0; i < taskDatas.Count; i++)
            {
                if (taskDatas[i].type == TaskType.Construct)
                {
                    BuildingType buildingType = (BuildingType)taskDatas[i].aimId;
                    if (playerData.structUnLockDataDic[playerData.currentMapID].Contains(buildingType))
                    {
                        //该建筑已经解锁
                        continue;
                    }

                    if (!playerData.structCanUnLockDataDic[playerData.currentMapID].Contains(buildingType))
                    {
                        if (PlayerDataModule.Instance.data.currentMapID == 1 &&
                        (buildingType == BuildingType.YuShaHu_1 || buildingType == BuildingType.LingChaJia_1 || buildingType == BuildingType.LingZhangTai))
                        {
                            continue;
                        }
                        //该建筑没有在可解锁列表中  添加
                        if (!playerData.structCanUnLockDataDic[playerData.currentMapID].Contains(buildingType))
                        {
                            playerData.structCanUnLockDataDic[playerData.currentMapID].Add(buildingType);
                        }

                        //从锁定容器中移除
                        if (playerData.structLockDataDic[playerData.currentMapID].Contains(buildingType))
                        {
                            playerData.structLockDataDic[playerData.currentMapID].Remove(buildingType);
                        }

                    }

                }
            }
            EventCenter.Instance.TriggerEvent(EventMessages.UpdateSturctureLockInfo);
        }

        public void FillStructureData(List<BuildingType> buildingTypes)
        {
            foreach (var buildingType in buildingTypes)
            {
                if (buildingType == BuildingType.YuShaHu_1)
                {
                    var productionData = PlayerDataModule.Instance.data.ProductStationDataList.Find(x => x.buildingType == BuildingType.YuShaHu_1);
                    if (productionData == null)
                    {
                        PlayerDataModule.Instance.data.ProductStationDataList.Add(new ProductStationData(BuildingType.YuShaHu_1));
                    }
                }
                if (buildingType == BuildingType.LingZhangTai)
                {
                    if (PlayerDataModule.Instance.data.cashierData == null)
                    {
                        PlayerDataModule.Instance.data.cashierData = new CashierData();
                    }
                }
                if (buildingType == BuildingType.YunDiGe)
                {
                    if (PlayerDataModule.Instance.data.deliverData == null)
                    {
                        PlayerDataModule.Instance.data.deliverData = new DeliverData();
                    }
                }
                if (buildingType == BuildingType.LingChuGe_1)
                {
                    var warehouseCategory = PlayerDataModule.Instance.data.warehouselist.Find(x => x.warehouseCategoryType == WarehouseCategoryType.LingChuGe_1);
                    if (warehouseCategory == null)
                    {
                        PlayerDataModule.Instance.data.warehouselist.Add(new WarehouseCategory(WarehouseCategoryType.LingChuGe_1));
                    }
                    if (buildingType == BuildingType.LingChuGe_2)
                    {
                        var warehouseCategory1 = PlayerDataModule.Instance.data.warehouselist.Find(x => x.warehouseCategoryType == WarehouseCategoryType.LingChuGe_2);
                        if (warehouseCategory1 == null)
                        {
                            PlayerDataModule.Instance.data.warehouselist.Add(new WarehouseCategory(WarehouseCategoryType.LingChuGe_2));
                        }
                    }
                }
            }
        }

        public List<TaskData> GetTaskGroupIds()
        {
            int groupSize = mapDataDic[PlayerDataModule.Instance.data.currentMapID].taskGroupSize;
            int mapId = PlayerDataModule.Instance.data.currentMapID;
            int taskId = PlayerDataModule.Instance.data.nowTaskId;
            if (taskId == 0)
            {
                taskId = 1;
            }
            int groupIndex = (taskId - 1) / groupSize;
            int start = groupIndex * groupSize + 1;
            int end = start + groupSize - 1;
            Dictionary<int, TaskData> dic = mapId switch
            {
                1 => mapTaskDataDic1,
                2 => mapTaskDataDic2,
                3 => mapTaskDataDic3,
                4 => mapTaskDataDic4,
                5 => mapTaskDataDic5,
                _ => null
            };
            if (dic == null)
            {
                Debug.LogError($"不存在的  mapId: {mapId}");
                return null;
            }
            List<TaskData> groupList = new();
            for (int id = start; id <= end; id++)
            {
                if (dic.TryGetValue(id, out TaskData data))
                {
                    groupList.Add(data);
                }
                else
                {
                    Debug.LogWarning($"任务ID {id} 在 map {mapId} 中不存在");
                }
            }
            return groupList;
        }


        public void InitMapLock()
        {
            List<MapLockData> mapLockDatas = new();
            switch (PlayerDataModule.Instance.data.currentMapID)
            {
                case 1:
                    mapLockDatas = mapLockDataList_1;
                    break;
                case 2:
                    mapLockDatas = mapLockDataList_2;
                    break;
                case 3:
                    mapLockDatas = mapLockDataList_3;
                    break;
                case 4:
                    mapLockDatas = mapLockDataList_4;
                    break;
                case 5:
                    mapLockDatas = mapLockDataList_5;
                    break;
                default:
                    break;

            }
            for (int i = 0; i < mapLockDatas.Count; i++)
            {
                if (GameController.Instance.mapLockDic.ContainsKey(mapLockDatas[i].monsterType))
                {
                    GameController.Instance.mapLockDic[mapLockDatas[i].monsterType].Init(mapLockDatas[i]);
                }
            }
        }
    }
}