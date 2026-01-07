using System.Collections.Generic;
using Controller.Structure;
using Module;
using Module.Data;
using UnityEngine;
using Utils;

namespace Controller
{
    public class YunDiGeController : StructureBase
    {
        public List<FreightClerkController> freightClerkList = new();
        public Transform bornTransform;
        public BuildingType buildingType;

        protected override void Start()
        {
            // for (int i = 0; i < ModuleMgr.Instance.GetModule<PlayerDataModule>().data.totalNum; i++)
            // {
            //     GameObject obj = GameObject.Instantiate(_assetHandle.Get<GameObject>("FreightClerk"));
            //     obj.transform.position = bornTransform.position;
            //     var cc = obj.GetComponent<FreightClerkController>();
            //     cc.Init();
            //     freightClerkList.Add(cc);
            // }
        }

        public void Init()
        {
             PlayerData playerData = ModuleMgr.Instance.GetModule<PlayerDataModule>().data;
            List<StructureLockData> structureLocks = new();
            switch (playerData.currentMapID)
            {
                case 1:
                    structureLocks = DataController.Instance.structureLockDataList_1;
                    break;
                case 2:
                    structureLocks = DataController.Instance.structureLockDataList_2;
                    break;
                case 3:
                    structureLocks = DataController.Instance.structureLockDataList_3;
                    break;
                case 4:
                    structureLocks = DataController.Instance.structureLockDataList_4;
                    break;
                case 5:
                    structureLocks = DataController.Instance.structureLockDataList_5;
                    break;
            }
            var lockData = structureLocks.Find(s => s.buildingType == buildingType);
            if (lockData != null)
            {
                var progressData = playerData.structureLockDataList.Find(s => s.buildType == buildingType && s.lockId == lockData.lockId && s.mapId == playerData.currentMapID);
                if (progressData != null && progressData.isUnlock)
                {
                    content.SetActive(true);
                    structureLock.gameObject.SetActive(false);
                
                }
                else
                {
                    content.SetActive(false);
                    structureLock.gameObject.SetActive(true);
                    structureLock.InitInfo(lockData);
                }
            }
        }



        void Update()
        {
        }
    }
}