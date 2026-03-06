using System;
using System.Collections;
using System.Collections.Generic;
using Controller.Pickups;
using Controller.Player;
using Controller.Structure;
using Module;
using Module.Data;
using Unity.VisualScripting;
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
        [Header("Delivery Visual")]
        public float deliveryLaunchInterval = 0.04f;
        public float deliveryFlyHeight = 2.2f;
        public float deliveryFlyDuration = 0.65f;
        public float deliverySpawnRadius = 0.35f;
        public Transform bornTransform;
        public Canvas canvas;
        public override void Awake()
        {
            base.Awake();
            if (canvas == null)
            {
                canvas = GetComponentInChildren<Canvas>();
            }
        }
        public override void Start()
        {
            base.Start();
            if (characterController == null)
            {
                characterController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
            }

            // 纭繚瀛樺偍瀛楀吀宸插垵濮嬪寲
            if (storage == null)
            {
                storage = new Dictionary<DropItemType, int>();
            }

            RefreshCurrentCapacityByStorage();
            canvas.sortingOrder = sprite.sortingOrder + 1;
        }

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
            // 瑙ｉ攣鍐呭鍚庯紝鎸夊綋鍓嶆暟鎹悓姝ラ噰闆嗗憳涓庝俊锟?

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
            canvas.sortingOrder = sprite.sortingOrder + 1;
        }


        /// <summary>
        /// 鏍规嵁浠撳簱鏁版嵁锛屽鍒犻噰闆嗗憳瀹炰緥锛屼娇涔嬩笌 workingCollectorList 涓€锟?
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


            if (targetCount > collectorControllerList.Count)
            {
                for (int i = collectorControllerList.Count; i < targetCount; i++)
                {
                    CollectorController cc = Instantiate(_assetHandle.Get<GameObject>("XuanCaiTu"))
                        .GetComponent<CollectorController>();
                    cc.transform.position = collectorTransform.position;
                    cc.Init(warehouseCategory.workingCollectorList[i], this);
                    collectorControllerList.Add(cc);
                }
            }
            // 鍑忓皯閲囬泦锟?
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

            // 鍒锋柊淇℃伅灞曠ず
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


        private void OnEnable()
        {
            EventCenter.Instance.AddListener(EventMessages.LingChuGeBeginWorking, HandleBeginWorking);
            EventCenter.Instance.AddListener(EventMessages.LingChuGeDelivery, HandleLingChuGeDelivery);
            EventCenter.Instance.AddListener(EventMessages.LingChuGeStopDelivery, HandleLingChuGeStopDelivery);
            EventCenter.Instance.AddListener(EventMessages.UpdateLingChuGeWorkingInfo, HandleBeginWorking);
            EventCenter.Instance.AddListener(EventMessages.UpdateSturctureLockInfo, Init);
        }

        private void OnDisable()
        {
            EventCenter.Instance.RemoveListener(EventMessages.LingChuGeBeginWorking, HandleBeginWorking);
            EventCenter.Instance.RemoveListener(EventMessages.LingChuGeDelivery, HandleLingChuGeDelivery);
            EventCenter.Instance.RemoveListener(EventMessages.LingChuGeStopDelivery, HandleLingChuGeStopDelivery);
            EventCenter.Instance.RemoveListener(EventMessages.UpdateLingChuGeWorkingInfo, HandleBeginWorking);
            EventCenter.Instance.RemoveListener(EventMessages.UpdateSturctureLockInfo, Init);
        }


        private void Update()
        {
            if (content.activeInHierarchy)
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

        }

        public void HandleBeginWorking(params object[] args)
        {
            // 缁熶竴鍏ュ彛锛氭洿鏂颁粨搴撳紩鐢ㄥ苟鎸夊綋鍓嶆暟鎹悓姝ラ噰闆嗗憳
            warehouseCategory = PlayerDataModule.Instance.data.warehouselist.Find(x => x.warehouseCategoryType == categoryType);
            if (warehouseCategory == null)
            {
                return;
            }

            if (infoitem != null)
            {
                infoitem.Init(warehouseCategory, this);
            }

            EnsureWarehouseRuntimeState();
            RefreshCurrentCapacityByStorage();
            UpdateCollectorInfo();
        }

        public void HandleLingChuGeDelivery(params object[] args)
        {
            if (isDelivering) return;
            if (args == null || args.Length == 0)
            {
                return;
            }
            if (characterController == null || characterController.receiveTransform == null)
            {
                return;
            }
            EnsureWarehouseRuntimeState();
            RefreshCurrentCapacityByStorage();

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
                GameObject obj = Instantiate(_assetHandle.Get<GameObject>("DropObj"));
                Vector3 spawnPos = sendTransform != null ? sendTransform.position : transform.position;
                Vector2 scatter = UnityEngine.Random.insideUnitCircle * Mathf.Max(0f, deliverySpawnRadius);
                obj.transform.position = new Vector3(spawnPos.x + scatter.x, spawnPos.y + scatter.y, spawnPos.z);

                var drop = obj.GetComponent<DropController>();
                drop.Init(targetType);
                drop.flyHeight = deliveryFlyHeight + UnityEngine.Random.Range(-0.2f, 0.2f);
                drop.flyDuration = Mathf.Max(0.2f, deliveryFlyDuration + UnityEngine.Random.Range(-0.08f, 0.08f));
                if (drop.spriteRenderer != null)
                {
                    drop.spriteRenderer.sortingOrder = (sprite != null ? sprite.sortingOrder : 30000) + 3;
                }

                deliveringDrops.Add(drop);
                float delay = Mathf.Max(0f, deliveryLaunchInterval) * i;
                StartCoroutine(PlayDeliveryDropFly(drop, targetType, delay));
            }
        }

        private IEnumerator PlayDeliveryDropFly(DropController drop, DropItemType targetType, float delay)
        {
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            if (!isDelivering || drop == null || characterController == null || characterController.receiveTransform == null)
            {
                if (drop != null)
                {
                    deliveringDrops.Remove(drop);
                    Destroy(drop.gameObject);
                }
                yield break;
            }

            drop.FlyTo(
                characterController.transform,
                characterController.receiveTransform,
                () => { HandleDeliveryDropArrive(drop, targetType); }
            );
        }

        private void HandleDeliveryDropArrive(DropController drop, DropItemType targetType)
        {
            deliveringDrops.Remove(drop);
            if (storage.TryGetValue(targetType, out var leftCount))
            {
                leftCount--;
                if (leftCount <= 0)
                {
                    storage.Remove(targetType);
                }
                else
                {
                    storage[targetType] = leftCount;
                }
            }

            if (warehouseCategory != null && warehouseCategory.ownItemList != null)
            {
                int ownNum = storage.TryGetValue(targetType, out var left) ? left : 0;
                warehouseCategory.ownItemList.Set((int)targetType, ownNum);
            }
            currentcapacity = Mathf.Max(0, currentcapacity - 1);

            if (deliveringDrops.Count == 0)
            {
                isDelivering = false;
            }
            EventCenter.Instance.TriggerEvent(EventMessages.UpdateLingChuGeInfo);
        }


        public void HandleLingChuGeStopDelivery(params object[] args)
        {
            if (!isDelivering) return;

            isDelivering = false;

            for (int i = deliveringDrops.Count - 1; i >= 0; i--)
            {
                var drop = deliveringDrops[i];
                if (drop == null) continue;

                // 缁堟椋炶
                drop.ForceStop();

                // 杩旇繕搴撳瓨锛堝鏋滈敭涓嶅瓨鍦ㄥ垯鍏堝垱寤猴級
                if (!storage.TryAdd(drop.itemType, 1))
                {
                    storage[drop.itemType]++;
                }
                if (warehouseCategory != null && warehouseCategory.ownItemList != null)
                {
                    warehouseCategory.ownItemList.Set((int)drop.itemType, storage[drop.itemType]);
                }

                // 鍥炴敹鐗╀綋
                Destroy(drop.gameObject);
            }

            deliveringDrops.Clear();
            RefreshCurrentCapacityByStorage();
            EventCenter.Instance.TriggerEvent(EventMessages.UpdateLingChuGeInfo);
        }


        /// <summary>
        /// 鍙栧嚭璐х墿
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="inv"></param>
        public int Store(CollectorController controller, CollectorInventory inv, int maxStoreCount = int.MaxValue)
        {
            if (inv == null)
            {
                return 0;
            }

            EnsureWarehouseRuntimeState();
            RefreshCurrentCapacityByStorage();
            int freeSpace = maxcapacity - currentcapacity;
            if (freeSpace <= 0)
                return 0;

            int remainToStore = Mathf.Max(1, maxStoreCount);
            int storedTotal = 0;
            var keys = new List<DropItemType>(inv.dic.Keys);

            foreach (var key in keys)
            {
                if (freeSpace <= 0 || remainToStore <= 0)
                    break;

                int carryCount = inv.dic[key];
                int storeCount = Mathf.Min(carryCount, freeSpace);
                storeCount = Mathf.Min(storeCount, remainToStore);
                if (storeCount <= 0)
                    continue;

                if (!storage.TryAdd(key, storeCount))
                {
                    storage[key] += storeCount;
                }
                if (warehouseCategory != null && warehouseCategory.ownItemList != null)
                {
                    warehouseCategory.ownItemList.Set((int)key, storage[key]);
                }

                currentcapacity += storeCount;
                freeSpace -= storeCount;
                remainToStore -= storeCount;
                storedTotal += storeCount;
                inv.Remove(key, storeCount);

                // 琛ㄧ幇
                for (int i = 0; i < storeCount; i++)
                {
                    GameObject obj = Instantiate(_assetHandle.Get<GameObject>("DropObj"));
                    Transform from = controller != null && controller.receiveTransform != null
                        ? controller.receiveTransform
                        : transform;
                    obj.transform.position = from.position;

                    var cc = obj.GetComponent<DropController>();
                    cc.Init(key);
                    if (cc.spriteRenderer != null)
                    {
                        cc.spriteRenderer.sortingOrder = (sprite != null ? sprite.sortingOrder : 30000) + 2;
                    }
                    cc.FlyTo(receiveTransform);
                }
            }
            if (controller != null)
            {
                controller.RefreshCarryInfo();
            }

            EventCenter.Instance.TriggerEvent(EventMessages.UpdateLingChuGeInfo);
            return storedTotal;
        }

        public bool HasFreeCapacity()
        {
            EnsureWarehouseRuntimeState();
            RefreshCurrentCapacityByStorage();
            if (maxcapacity <= 0)
            {
                return false;
            }
            return currentcapacity < maxcapacity;
        }


        private void EnsureWarehouseRuntimeState()
        {
            if (storage == null)
            {
                storage = new Dictionary<DropItemType, int>();
            }

            if (warehouseCategory == null)
            {
                var playerData = PlayerDataModule.Instance.data;
                if (playerData != null)
                {
                    warehouseCategory = playerData.warehouselist.Find(x => x.warehouseCategoryType == categoryType);
                }
            }

            RestoreStorageFromOwnItemListIfNeeded();

            if (maxcapacity > 0 || warehouseCategory == null)
            {
                return;
            }

            maxcapacity = warehouseCategory.capacity;
            var progresses = PlayerDataModule.Instance.data.cardUpProgressesList;
            if (categoryType == WarehouseCategoryType.LingChuGe_1)
            {
                var cardData = progresses.Find(x => x.developType == CardDevelopType.UpgradeLingChuGe_1);
                if (cardData != null)
                {
                    maxcapacity += cardData.level * 10;
                }
            }
            else
            {
                var cardData = progresses.Find(x => x.developType == CardDevelopType.UpgradeLingChuGe_2);
                if (cardData != null)
                {
                    maxcapacity += cardData.level * 10;
                }
            }
        }

        private void RestoreStorageFromOwnItemListIfNeeded()
        {
            if (warehouseCategory?.ownItemList?.list == null)
            {
                return;
            }

            if (storage == null)
            {
                storage = new Dictionary<DropItemType, int>();
            }

            // Runtime storage is authoritative during gameplay; only hydrate when it is empty.
            if (storage.Count > 0)
            {
                return;
            }

            foreach (var kv in warehouseCategory.ownItemList.list)
            {
                if (kv == null || kv.value <= 0)
                {
                    continue;
                }

                if (!Enum.IsDefined(typeof(DropItemType), kv.key))
                {
                    continue;
                }

                storage[(DropItemType)kv.key] = kv.value;
            }
        }

        private void RefreshCurrentCapacityByStorage()
        {
            if (storage == null)
            {
                storage = new Dictionary<DropItemType, int>();
            }
            int total = 0;
            foreach (var kv in storage)
            {
                if (kv.Value > 0)
                {
                    total += kv.Value;
                }
            }
            currentcapacity = total;
        }

    }
}




