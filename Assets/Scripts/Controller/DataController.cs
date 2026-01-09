using System.Collections.Generic;
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

        void Start()
        {
            PrepareData();
        }


        void Update()
        {

        }


        private async void PrepareData()
        {
            string monstetStr = (await ResourceLoader.Instance.LoadAssetAsync<TextAsset>("MonsterData")).text;
            monsterDataDic.Clear();
            monsterDataDic = JsonConvert.DeserializeObject<Dictionary<MonsterType, MonsterData>>(monstetStr);
            EventCenter.Instance.TriggerEvent(EventMessages.MonsterBeginCreate);

            string customerStr = (await ResourceLoader.Instance.LoadAssetAsync<TextAsset>("CustomerData")).text;
            customerDataDic.Clear();
            customerDataDic = JsonConvert.DeserializeObject<Dictionary<CustomerType, CustomerData>>(customerStr);
            EventCenter.Instance.TriggerEvent(EventMessages.CustomerBeginCreate);


            string mapStr = (await ResourceLoader.Instance.LoadAssetAsync<TextAsset>("MapData")).text;
            mapDataDic?.Clear();
            mapDataDic = JsonConvert.DeserializeObject<Dictionary<int, MapData>>(mapStr);
            EventCenter.Instance.TriggerEvent(EventMessages.MapDataPrepared);

            string rewardStr = (await ResourceLoader.Instance.LoadAssetAsync<TextAsset>("RewardData")).text;
            taskRewardDataDic.Clear();
            taskRewardDataDic = JsonConvert.DeserializeObject<Dictionary<int, RewardData>>(rewardStr);

            string storageStr = (await ResourceLoader.Instance.LoadAssetAsync<TextAsset>("StorageBagData")).text;
            storageBagDataDic.Clear();
            storageBagDataDic = JsonConvert.DeserializeObject<Dictionary<int, StotageBagData>>(storageStr);

            string weaponStr = (await ResourceLoader.Instance.LoadAssetAsync<TextAsset>("WeaponData")).text;
            weaponDataDic.Clear();
            weaponDataDic = JsonConvert.DeserializeObject<Dictionary<int, WeaponData>>(weaponStr);
            EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerEquimentInfo);

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
            EventCenter.Instance.TriggerEvent(EventMessages.MapTaskDataPrepared);

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

           InitMapLock();

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


        public List<TaskData> GetTaskGroupIds()
        {
            int groupSize = mapDataDic[ModuleMgr.Instance.GetModule<PlayerDataModule>().data.currentMapID].taskGroupSize;
            int mapId = ModuleMgr.Instance.GetModule<PlayerDataModule>().data.currentMapID;
            int taskId = ModuleMgr.Instance.GetModule<PlayerDataModule>().data.nowTaskId;
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
            switch (ModuleMgr.Instance.GetModule<PlayerDataModule>().data.currentMapID)
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