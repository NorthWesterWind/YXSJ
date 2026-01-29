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
        public MeshRenderer renderer;

        protected override void Start()
        {
        }
        void OnEnable()
        {
            EventCenter.Instance.AddListener(EventMessages.UpdateSturctureLockInfo, Init);
        }
        void OnDisable()
        {
            EventCenter.Instance.RemoveListener(EventMessages.UpdateSturctureLockInfo, Init);
        }

        public void Init(params object[] args)
        {
            if (GameController.Instance.unlockedBuildingTypes.Contains(buildingType))
            {
                return;
            }
            var playerData = PlayerDataModule.Instance.data;
            var lockData = GetLockData(playerData.currentMapID);
            var state = GetStructureState(playerData, lockData);
            for (int i = freightClerkList.Count; i > 0; i++)
            {
                Destroy(freightClerkList[i - 1].gameObject);
                freightClerkList.RemoveAt(i - 1);
            }
            RefreshView(state, lockData);
          
        }

        private void RefreshView(StructureState state, StructureLockData lockData)
        {
            switch (state)
            {
                case StructureState.Locked:
                case StructureState.CanUnlock:
                    ShowLock(lockData);
                    break;

                case StructureState.Unlocked:
                    ShowContent();
                    break;
            }
        }

        private void ShowContent()
        {
            content.SetActive(true);
            renderer.sortingOrder = sprite.sortingOrder + 1;
            structureLock.gameObject.SetActive(false);
            GameController.Instance.unlockedBuildingTypes.Add(buildingType);
              UpdateYunDiZheInfo();
        }

        public StructureLockData GetLockData(int mapId)
        {
            var list = DataController.Instance.GetStructureLockList(mapId);
            return list?.Find(s => s.buildingType == buildingType);
        }
        private StructureState GetStructureState(PlayerData playerData, StructureLockData lockData)
        {
            if (lockData == null)
                return StructureState.Unlocked;

            var locked = playerData.structLockDataDic[playerData.currentMapID];
            var unlocked = playerData.structUnLockDataDic[playerData.currentMapID];
            var canUnlock = playerData.structCanUnLockDataDic[playerData.currentMapID];

            if (unlocked.Contains(buildingType))
                return StructureState.Unlocked;

            if (locked.Contains(buildingType))
                return StructureState.Locked;

            return StructureState.CanUnlock;
        }


        public void UpdateYunDiZheInfo(params object[] args)
        {
            PlayerData playerData = PlayerDataModule.Instance.data;
            if (playerData.deliverData.workingNum > freightClerkList.Count)
            {
                for (int i = freightClerkList.Count; i < playerData.deliverData.workingNum; i++)
                {
                    FreightClerkController freightClerk = Instantiate(_assetHandle.Get<GameObject>("FreightClerk"), bornTransform, false).GetComponent<FreightClerkController>();
                    freightClerk.Init();
                    freightClerkList.Add(freightClerk);
                }
            }


        }
        public override void AddEvent()
        {
            base.AddEvent();
            EventCenter.Instance.AddListener(EventMessages.UpdateYunDiZheInfo, UpdateYunDiZheInfo);



        }


        void Update()
        {

        }
    }
}