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
        public Dictionary< MapType,MapData>  mapDataDic = new();
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
            mapDataDic = JsonConvert.DeserializeObject<Dictionary<MapType, MapData>>(mapStr);
            EventCenter.Instance.TriggerEvent(EventMessages.MapDataPrepared);
        }
    }
}