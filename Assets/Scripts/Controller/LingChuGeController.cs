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
        public WarehouseCategory warehouseCategory;
        public Transform receiveTransform;
        public Transform sendTransform;
        public LingChuGeInfo infoitem;
        public Transform collectorTransform;
        public WarehouseCategoryType categoryType;
        public List<CollectorController> collectorControllerList = new List<CollectorController>();
        public Dictionary<DropItemType, int> storage = new Dictionary<DropItemType, int>();
        public int maxcapacity;
        public int currentcapacity;
        public PlayerController characterController;

        private bool isDelivering = false;
        private List<DropController> deliveringDrops = new();
        public Transform bornTransform;

        public void Init(params object[] args)
        {
            if (GameController.Instance.unlockedBuildingTypes.Contains(structureType))
            {
                return;
            }
            var playerData = PlayerDataModule.Instance.data;
            var lockData = GetLockData(playerData.currentMapID);
            var state = GetStructureState(playerData, lockData);
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
            structureLock.gameObject.SetActive(false);
            GameController.Instance.unlockedBuildingTypes.Add(structureType);
            // 解锁内容后，按当前数据同步采集员与信息

            GetComponent<Canvas>().sortingOrder = sprite.sortingOrder + 1;
            var playerData = PlayerDataModule.Instance.data;
            maxcapacity = playerData.warehouselist.Find(x => x.warehouseCategoryType == categoryType).capacity;
            if (categoryType == WarehouseCategoryType.LingChuGe_1)
            {
                var cardData = playerData.cardUpProgressesList.Find(x => x.developType == CardDevelopType.UpgradeLingChuGe_1);
                if (cardData != null)
                {
                    maxcapacity += cardData.level * 10;
                }
            }
            else
            {
                var cardData = playerData.cardUpProgressesList.Find(x => x.developType == CardDevelopType.UpgradeLingChuGe_2);
                if (cardData != null)
                {
                    maxcapacity += cardData.level * 10;
                }
            }
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
                    CollectorController cc = Instantiate(_assetHandle.Get<GameObject>("XuaCaiTu"))
                        .GetComponent<CollectorController>();
                    cc.transform.position = collectorTransform.position;
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
            EventCenter.Instance.AddListener(EventMessages.LingChuGeBeginWorking, HandleBeginWorking);
            EventCenter.Instance.AddListener(EventMessages.LingChuGeDelivery, HandleLingChuGeDelivery);
            EventCenter.Instance.AddListener(EventMessages.LingChuGeStopDelivery, HandleLingChuGeStopDelivery);
            EventCenter.Instance.AddListener(EventMessages.UpdateSturctureLockInfo, Init);
        }

        private void OnDisable()
        {
            EventCenter.Instance.RemoveListener(EventMessages.LingChuGeBeginWorking, HandleBeginWorking);
            EventCenter.Instance.RemoveListener(EventMessages.LingChuGeDelivery, HandleLingChuGeDelivery);
            EventCenter.Instance.RemoveListener(EventMessages.LingChuGeStopDelivery, HandleLingChuGeStopDelivery);
            EventCenter.Instance.RemoveListener(EventMessages.UpdateSturctureLockInfo, Init);
        }


        private void Update()
        {
            if (Vector2.Distance(characterController.gameObject.transform.position, transform.position) < 8)
            {
                infoitem.ShowInfo();
            }
            else
            {
                infoitem.HideInfo();
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
        public void Store(CollectorController controller, CollectorInventory inv)
        {
            int freeSpace = maxcapacity - currentcapacity;
            if (freeSpace <= 0)
                return;

            // 拷贝 key，避免修改集合异常
            var keys = new List<DropItemType>(inv.dic.Keys);

            foreach (var key in keys)
            {
                if (freeSpace <= 0)
                    break;

                int carryCount = inv.dic[key];
                int storeCount = Mathf.Min(carryCount, freeSpace);
                if (storeCount <= 0)
                    continue;

                // 仓库存
                if (!storage.TryAdd(key, storeCount))
                    storage[key] += storeCount;

                currentcapacity += storeCount;
                freeSpace -= storeCount;

                // 背包扣
                inv.Remove(key, storeCount);

                // 表现
                for (int i = 0; i < storeCount; i++)
                {
                    GameObject obj = Instantiate(_assetHandle.Get<GameObject>("DropObj"));
                    obj.transform.position = controller.receiveTransform.position;

                    var cc = obj.GetComponent<DropController>();
                    cc.Init(key);
                    cc.FlyTo(receiveTransform);
                }
            }

            EventCenter.Instance.TriggerEvent(EventMessages.UpdateLingChuGeInfo);
        }

    }
}
