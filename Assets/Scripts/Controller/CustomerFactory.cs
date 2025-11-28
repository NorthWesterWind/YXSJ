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

        private void OnEnable()
        {
            EventCenter.Instance.AddListener(EventMessages.MapDataPrepared, HandleCustomerCreat);
        }

        private void OnDisable()
        {
            EventCenter.Instance.RemoveListener(EventMessages.MapDataPrepared, HandleCustomerCreat);
        }

        private void Start()
        {
            _assetHandle = GetComponent<AssetHandle>();
        }


        public void HandleCustomerCreat(params object[] args)
        {
            customerTypeList.Clear();
            mapData = DataController.Instance.mapDataDic[
                ModuleMgr.Instance.GetModule<PlayerDataModule>().data.currentMapID];
            customerTypeList = mapData.customerTypeList;
            StartCoroutine(CreatCustomer());
        }

        public IEnumerator CreatCustomer()
        {
            while (true)
            {
               
                var tempdata =
                    DataController.Instance.customerDataDic[(CustomerType)Extensions.RandomOne(customerTypeList)];
                var randomPair =
                    GameController.Instance.goodBuild.ElementAt(
                        Random.Range(0, GameController.Instance.goodBuild.Count));
                GoodsType key = randomPair.Key;
                StructureBase value = randomPair.Value;
                GameObject obj = Instantiate(_assetHandle.Get<GameObject>(Extensions.GetCustomerResNameByType(tempdata.type)));
                obj.transform.position = GetRandomPosition();
                obj.GetComponent<CustomerController>().Init(tempdata, key , value);
                yield return new WaitForSeconds(10);
            }
        }


        private Vector3 GetRandomPosition()
        {
            Vector3 position = transform.position;
            position.x += Random.Range(-5f, 5f);
            position.y += Random.Range(-5f, 5f);
            return position;
        }
    }
}