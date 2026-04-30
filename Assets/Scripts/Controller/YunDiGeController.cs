using System.Collections.Generic;
using Controller.Structure;
using Module;
using Module.Data;
using Unity.VisualScripting;
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

        void OnEnable()
        {
            EventCenter.Instance.AddListener(EventMessages.UpdateSturctureLockInfo, Init);
            EventCenter.Instance.AddListener(EventMessages.UpdateYunDiZheInfo, UpdateYunDiZheInfo);
        }
        void OnDisable()
        {
            EventCenter.Instance.RemoveListener(EventMessages.UpdateSturctureLockInfo, Init);
            EventCenter.Instance.RemoveListener(EventMessages.UpdateYunDiZheInfo, UpdateYunDiZheInfo);
        }

        public void Init(params object[] args)
        {
            var playerData = PlayerDataModule.Instance.data;
            if (GameController.Instance.unlockedBuildingTypes.Contains(buildingType))
            {
                var unlocked = playerData.structUnLockDataDic[playerData.currentMapID];
                if (!unlocked.Contains(buildingType))
                {
                    unlocked.Add(buildingType);
                }
                playerData.structLockDataDic[playerData.currentMapID].Remove(buildingType);
                playerData.structCanUnLockDataDic[playerData.currentMapID].Remove(buildingType);
            }
            var lockData = GetLockData(playerData.currentMapID);
            var state = GetStructureState(playerData, lockData);
            ClearFreightClerks();
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
            if (playerData.deliverData == null)
            {
                ClearFreightClerks();
                return;
            }

            freightClerkList.RemoveAll(x => x == null);
            int targetCount = Mathf.Max(0, playerData.deliverData.workingNum);

            if (targetCount > freightClerkList.Count)
            {
                for (int i = freightClerkList.Count; i < targetCount; i++)
                {
                    CreateFreightClerk();
                }
            }
            else if (targetCount < freightClerkList.Count)
            {
                int removeCount = freightClerkList.Count - targetCount;

                for (int i = freightClerkList.Count - 1; i >= 0 && removeCount > 0; i--)
                {
                    var clerk = freightClerkList[i];
                    if (clerk == null)
                    {
                        freightClerkList.RemoveAt(i);
                        removeCount--;
                        continue;
                    }

                    if (clerk.HasCarriedProducts())
                    {
                        continue;
                    }

                    freightClerkList.RemoveAt(i);
                    DestroyFreightClerk(clerk);
                    removeCount--;
                }

                for (int i = freightClerkList.Count - 1; i >= 0 && removeCount > 0; i--)
                {
                    var clerk = freightClerkList[i];
                    if (clerk == null)
                    {
                        freightClerkList.RemoveAt(i);
                        removeCount--;
                        continue;
                    }

                    freightClerkList.RemoveAt(i);
                    clerk.StopWorking();
                    removeCount--;
                }
            }
        }

        private void CreateFreightClerk()
        {
            if (_assetHandle == null)
            {
                _assetHandle = GetComponent<AssetHandle>();
            }

            if (_assetHandle == null)
            {
                Debug.LogWarning("[YunDiGe] Missing AssetHandle, cannot create FreightClerk.");
                return;
            }

            var prefab = _assetHandle.Get<GameObject>("FreightClerk");
            if (prefab == null)
            {
                Debug.LogWarning("[YunDiGe] Missing FreightClerk prefab.");
                return;
            }

            FreightClerkController freightClerk = Instantiate(prefab).GetComponent<FreightClerkController>();
            if (freightClerk == null)
            {
                return;
            }

            var spawnPoint = bornTransform != null ? bornTransform.position : transform.position;
            freightClerk.transform.position = spawnPoint;
            freightClerk.Init();
            freightClerkList.Add(freightClerk);
        }

        private void ClearFreightClerks()
        {
            FreightClerkController.ResetStationReservations();
            if (freightClerkList == null || freightClerkList.Count == 0)
            {
                return;
            }

            var clerks = freightClerkList.ToArray();
            freightClerkList.Clear();
            for (int i = 0; i < clerks.Length; i++)
            {
                var clerk = clerks[i];
                if (clerk == null)
                {
                    continue;
                }

                DestroyFreightClerk(clerk);
            }
        }

        private void DestroyFreightClerk(FreightClerkController clerk)
        {
            if (clerk == null)
            {
                return;
            }

            clerk.CleanupBeforeDestroy();
            Destroy(clerk.gameObject);
        }
        public override void AddEvent()
        {
            base.AddEvent();

        }


        void Update()
        {

        }
    }
}
