using System;
using System.Collections.Generic;
using Controller.Pickups;
using Controller.Player;
using Controller.Structure;
using Module;
using Module.Data;
using UnityEngine;
using Utils;
using View.LingChuGe;

namespace Controller
{
    public class LingChuGeController : StructureBase
    {

        public WarehouseCategory  warehouseCategory;
        public Transform receiveTransform;
        public Transform sendTransform;
        public LingChuGeInfo infoitem;
        public Transform collectorTransform;
        public WarehouseCategoryType categoryType; 
        public List<CollectorController>  collectorControllerList = new List<CollectorController>();
        public Dictionary< DropItemType , int> storage = new Dictionary<DropItemType, int>();
        public int capacity;
        public PlayerController characterController;
        
        private bool isDelivering = false;
        private List<DropController> deliveringDrops = new();
        public  MeshRenderer meshRenderer;
        public Transform bornTransform;
        
        public void Init(params object[] args)
        {
            if( GameController.Instance.unlockedBuildingTypes.Contains(structureType))
            {
                return;
            }   
            var playerData = PlayerDataModule.Instance.data;
            var lockData = GetLockData(playerData.currentMapID);
            var state = GetStructureState(playerData, lockData);
            for (int i = collectorControllerList.Count; i > 0; i++)
            {
                Destroy(collectorControllerList[i - 1].gameObject);
                collectorControllerList.RemoveAt(i - 1);
            }
            RefreshView(state, lockData);
            UpdateCollectorInfo();
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
            meshRenderer.sortingOrder = sprite.sortingOrder + 1;
            structureLock.gameObject.SetActive(false);
            GameController.Instance.unlockedBuildingTypes.Add(structureType);
            // 解锁内容后，按当前数据同步采集员与信息
            HandleBeginWorking();
        }


        /// <summary>
        /// 根据仓库数据，增删采集员实例，使之与 workingCollectorList 一致
        /// </summary>
        public void UpdateCollectorInfo(params object[] args)
        {
            PlayerData playerData = PlayerDataModule.Instance.data;
            warehouseCategory = playerData.warehouselist.Find(x => x.warehouseCategoryType == categoryType);
            if (warehouseCategory == null)
            {
                return;
            }

            int targetCount = warehouseCategory.workingCollectorList.Count;

            // 增加采集员
            if (targetCount > collectorControllerList.Count)
            {
                for (int i = collectorControllerList.Count; i < targetCount; i++)
                {
                    CollectorController cc = Instantiate(_assetHandle.Get<GameObject>("XuaCaiTu"), bornTransform, false)
                        .GetComponent<CollectorController>();
                    cc.Init(warehouseCategory.workingCollectorList[i], this);
                    collectorControllerList.Add(cc);
                }
            }
            // 减少采集员
            else if (targetCount < collectorControllerList.Count)
            {
                for (int i = collectorControllerList.Count - 1; i >= targetCount; i--)
                {
                    var collector = collectorControllerList[i];
                    collectorControllerList.RemoveAt(i);
                    if (collector != null)
                    {
                        Destroy(collector.gameObject);
                    }
                }
            }

            // 刷新信息展示
            if (infoitem != null)
            {
                infoitem.Init(warehouseCategory, this);
            }
        }

          public StructureLockData GetLockData(int mapId)
        {
            var list = DataController.Instance.GetStructureLockList(mapId);
            return list?.Find(s => s.buildingType == structureType);
        }
        private StructureState GetStructureState(PlayerData playerData, StructureLockData lockData)
        {
            if (lockData == null)
                return StructureState.Unlocked;

            var locked = playerData.structLockDataDic[playerData.currentMapID];
            var unlocked = playerData.structUnLockDataDic[playerData.currentMapID];
            var canUnlock = playerData.structCanUnLockDataDic[playerData.currentMapID];

            if (unlocked.Contains(structureType))
                return StructureState.Unlocked;

            if (locked.Contains(structureType))
                return StructureState.Locked;

            return StructureState.CanUnlock;
        }
        protected override void Start()
        {
            base.Start();
            if (characterController == null)
            {
                characterController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
            }

            // 确保存储字典已初始化
            if (storage == null)
            {
                storage = new Dictionary<DropItemType, int>();
            }
        }

        private void OnEnable()
        {
            EventCenter.Instance.AddListener(EventMessages.LingChuGeBeginWorking , HandleBeginWorking);
            EventCenter.Instance.AddListener(EventMessages.LingChuGeDelivery , HandleLingChuGeDelivery);
            EventCenter.Instance.AddListener(EventMessages.LingChuGeStopDelivery , HandleLingChuGeStopDelivery);
            EventCenter.Instance.AddListener(EventMessages.UpdateSturctureLockInfo, Init);
        }

        private void OnDisable()
        {
            EventCenter.Instance.RemoveListener(EventMessages.LingChuGeBeginWorking , HandleBeginWorking);
            EventCenter.Instance.RemoveListener(EventMessages.LingChuGeDelivery , HandleLingChuGeDelivery);
            EventCenter.Instance.RemoveListener(EventMessages.LingChuGeStopDelivery , HandleLingChuGeStopDelivery);
            EventCenter.Instance.RemoveListener(EventMessages.UpdateSturctureLockInfo, Init);
        }


        private void Update()
        {
            if (Vector2.Distance(characterController.gameObject.transform.position, transform.position) < 8)
            {
                infoitem.gameObject.SetActive(true);
            }
            else
            {
                infoitem.gameObject.SetActive(false);
            }
        }

        public void HandleBeginWorking(params object[] args)
        {
            // 统一入口：更新仓库引用并按当前数据同步采集员
            warehouseCategory = PlayerDataModule.Instance.data.warehouselist.Find(x => x.warehouseCategoryType == categoryType);
            if (warehouseCategory == null)
            {
                return;
            }

            if (infoitem != null)
            {
                infoitem.Init(warehouseCategory, this);
            }

            UpdateCollectorInfo();
        }

        public void HandleLingChuGeDelivery(params object[] args)
        {
            if (isDelivering) return;

            DropItemType targetType = (DropItemType)args[0];

            if (!storage.TryGetValue(targetType, out var count))
            {
                count = 0;
            }

            count = Mathf.Min(count, characterController.RemainCapacity);

            if (count <= 0) return;

            isDelivering = true;

            for (int i = 0; i < count; i++)
            {
                GameObject obj = Instantiate(_assetHandle.Get<GameObject>("DropItem"));
                obj.transform.position = sendTransform.position;
 
                var drop = obj.GetComponent<DropController>();
                drop.Init(targetType);

                deliveringDrops.Add(drop);

                drop.FlyTo(
                    characterController.transform,
                    characterController.receiveTransform,
                    () =>
                    {
                        // 成功送达回调
                        deliveringDrops.Remove(drop);
                        storage[targetType]--;

                        if (deliveringDrops.Count == 0)
                            isDelivering = false;
                    }
                );
            }
        }

        
        public void HandleLingChuGeStopDelivery(params object[] args)
        {
            if (!isDelivering) return;

            isDelivering = false;

            for (int i = deliveringDrops.Count - 1; i >= 0; i--)
            {
                var drop = deliveringDrops[i];
                if (drop == null) continue;

                // 终止飞行
                drop.ForceStop();

                // 返还库存（如果键不存在则先创建）
                if (!storage.TryAdd(drop.itemType, 1))
                {
                    storage[drop.itemType]++;
                }

                // 回收物体
                Destroy(drop.gameObject);
            }

            deliveringDrops.Clear();
        }
        
        
        /// <summary>
        /// 取出货物
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="inv"></param>
        public void Store( CollectorController controller , CollectorInventory inv)
        {
            foreach (var kv in inv.dic)
            {
                GameObject obj = Instantiate(_assetHandle.Get<GameObject>("DropObj"));
                obj.transform.position = controller.receiveTransform.position;
                var cc =  obj.GetComponent<DropController>();
                cc.Init(controller.targetType);
                cc.FlyTo(receiveTransform);
                if (!storage.TryAdd(kv.Key, kv.Value))
                    storage[kv.Key] += kv.Value;
            }
            inv.Clear();
        }
    }
}
