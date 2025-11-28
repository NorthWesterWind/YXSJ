using System.Collections.Generic;
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
        public Dictionary<int,MapData>  mapDataDic = new();
        public Dictionary<int , RewardData> taskRewardDataDic = new();
        public Dictionary<int , StorageBagData> storageBagDataDic = new();
        public Dictionary<int, WeaponData> weaponDataDic = new();
        public Dictionary<int , TaskData> mapTaskDataDic1 = new(); // 30 
        public Dictionary<int , TaskData> mapTaskDataDic2 = new(); // 60
        public Dictionary<int , TaskData> mapTaskDataDic3 = new(); //90
        public Dictionary<int , TaskData> mapTaskDataDic4 = new(); //100
        public Dictionary<int , TaskData> mapTaskDataDic5 = new(); //110
        void Start()
        {
            PrepareData();
        }

        // Update is called once per frame
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
            mapDataDic.Clear();
            mapDataDic = JsonConvert.DeserializeObject<Dictionary<int, MapData>>(mapStr);
            EventCenter.Instance.TriggerEvent(EventMessages.MapDataPrepared);
            
            string rewardStr = (await ResourceLoader.Instance.LoadAssetAsync<TextAsset>("RewardData")).text;
            taskRewardDataDic.Clear();
            taskRewardDataDic = JsonConvert.DeserializeObject<Dictionary<int, RewardData>>(rewardStr);
            
            string storageStr = (await ResourceLoader.Instance.LoadAssetAsync<TextAsset>("StorageBagData")).text;
            storageBagDataDic.Clear();
            storageBagDataDic =  JsonConvert.DeserializeObject<Dictionary<int, StorageBagData>>(storageStr);
            
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
        }
        
        
        public List<TaskData> GetTaskGroupIds(int taskId, int groupSize, int mapId)
        {
            if (groupSize <= 0)
            {
                Debug.LogError("groupSize 必须 > 0");
                return null;
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
                Debug.LogError($"不存在的 mapId: {mapId}");
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

        
    }
}