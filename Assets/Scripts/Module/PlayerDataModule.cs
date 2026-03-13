using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Controller;
using Controller.Pickups;
using Controller.Player;
using Controller.Structure;
using Module.Data;
using Newtonsoft.Json;
using UnityEngine;
using Utils;
using View;

namespace Module
{
    public class PlayerDataModule : MonoSingleton<PlayerDataModule>
    {
        public PlayerData data = new();
        private Coroutine _runtimeRestoreCoroutine;
        private Coroutine _runtimeLayerFixCoroutine;
        private int _runtimeRestoredMapId = -1;
        private int _lastSpeedTimeSecond = -1;

        public override void Awake()
        {
            EventCenter.Instance.AddListener(EventMessages.BeginJugmentRemainTime, BeginJugmentRemainTime);
            EventCenter.Instance.AddListener(EventMessages.ProduceTask, HandleProduceTask);
            EventCenter.Instance.AddListener(EventMessages.UpGradeStuctureTask, HandleUpGradeStuctureTask);
            EventCenter.Instance.AddListener(EventMessages.ConstructTask, HandleConstructTask);
            EventCenter.Instance.AddListener(EventMessages.SellTask, HandleSellTask);
            EventCenter.Instance.AddListener(EventMessages.HarvestTask, HandleHarvestTask);
            EventCenter.Instance.AddListener(EventMessages.MakeTongBiTask, HandleMakeTongBiTask);
            EventCenter.Instance.AddListener(EventMessages.UnLockMapTask, HandleUnLockMapTask);
            EventCenter.Instance.AddListener(EventMessages.MapDataPrepared, HandleMapDataPrepared);
            EventCenter.Instance.AddListener(EventMessages.UpdatePlayerInfo, HandlePlayerInventoryChanged);
            EventCenter.Instance.AddListener(EventMessages.UpdatePlayerCarryInfo, HandlePlayerInventoryChanged);
        }

        private void Update()
        {
            UpdateSpeedTimeCountdown();
        }

        private void UpdateSpeedTimeCountdown()
        {
            if (data == null)
            {
                return;
            }

            if (data.speedTime <= 0f)
            {
                if (_lastSpeedTimeSecond != 0)
                {
                    _lastSpeedTimeSecond = 0;
                    EventCenter.Instance.TriggerEvent(EventMessages.UpdateSpeedTime, 0);
                }
                return;
            }

            data.speedTime = Mathf.Max(0f, data.speedTime - Time.deltaTime);
            int seconds = Mathf.CeilToInt(data.speedTime);
            if (seconds != _lastSpeedTimeSecond)
            {
                _lastSpeedTimeSecond = seconds;
                EventCenter.Instance.TriggerEvent(EventMessages.UpdateSpeedTime, seconds);
            }
        }

        private void HandlePlayerInventoryChanged(params object[] args)
        {
            if (_runtimeRestoredMapId != data.currentMapID)
            {
                bool hasSavedRuntime = (data.runtimePlayerDropList != null && data.runtimePlayerDropList.Count > 0) ||
                                       (data.runtimePlayerGoodsList != null && data.runtimePlayerGoodsList.Count > 0);
                if (hasSavedRuntime)
                {
                    return;
                }
            }

            CapturePlayerInventory();
        }

        public void FillStructureLockProgressData()
        {
            foreach (var task in data.listenInTaskList)
            {
                if (task.type == TaskType.Upgrade)
                {
                    BuildingType type = (BuildingType)task.aimId;
                    if (data.ProductStationDataList.Find(x => x.buildingType == type) != null)
                    {
                        if (data.taskProgressDic.ContainsKey(task.taskId))
                        {
                            data.taskProgressDic[task.taskId] += data.ProductStationDataList.Find(x => x.buildingType == type).priceLevel;
                        }
                        else
                        {
                            data.taskProgressDic.Add(task.taskId, data.ProductStationDataList.Find(x => x.buildingType == type).priceLevel);
                        }

                    }
                    if (type == BuildingType.LingZhangTai)
                    {
                        if (data.taskProgressDic.ContainsKey(task.taskId))
                        {
                            data.taskProgressDic[task.taskId] += data.cashierData.workspeedLevel;
                        }
                        else
                        {
                            data.taskProgressDic.Add(task.taskId, data.cashierData.workspeedLevel);
                        }
                    }
                    if (type == BuildingType.YunDiGe)
                    {
                        if (data.deliverData == null)
                        {
                            data.deliverData = new DeliverData();
                        }
                        if (data.taskProgressDic.ContainsKey(task.taskId))
                        {
                            data.taskProgressDic[task.taskId] += data.deliverData.speedLevel;
                        }
                        else
                        {
                            data.taskProgressDic.Add(task.taskId, data.deliverData.speedLevel);
                        }
                    }
                    if (type == BuildingType.LingChuGe_1)
                    {
                        if (data.warehouselist.Find(x => x.warehouseCategoryType == WarehouseCategoryType.LingChuGe_1) != null)
                        {
                            WarehouseCategory warehouse = data.warehouselist.Find(x => x.warehouseCategoryType == WarehouseCategoryType.LingChuGe_1);
                            if (data.taskProgressDic.ContainsKey(task.taskId))
                            {
                                data.taskProgressDic[task.taskId] += warehouse.atkLevel;
                            }
                            else
                            {
                                data.taskProgressDic.Add(task.taskId, warehouse.atkLevel);
                            }
                        }
                    }
                    if (type == BuildingType.LingChuGe_2)
                    {
                        if (data.warehouselist.Find(x => x.warehouseCategoryType == WarehouseCategoryType.LingChuGe_2) != null)
                        {
                            WarehouseCategory warehouse = data.warehouselist.Find(x => x.warehouseCategoryType == WarehouseCategoryType.LingChuGe_2);
                            if (data.taskProgressDic.ContainsKey(task.taskId))
                            {
                                data.taskProgressDic[task.taskId] += warehouse.atkLevel;
                            }
                            else
                            {
                                data.taskProgressDic.Add(task.taskId, warehouse.atkLevel);
                            }
                        }
                    }
                }

                if (task.type == TaskType.Construct)
                {
                    BuildingType type = (BuildingType)task.aimId;
                    var _data = data.structureLockProgressDataList.Find(x => x.buildType == type);
                    if (_data == null)
                    {
                        switch (data.currentMapID)
                        {
                            case 1:
                                if (type != BuildingType.LingZhangTai && type != BuildingType.YuShaHu_1 &&
                                    type != BuildingType.LingChaJia_1)
                                {
                                    StructureLockData data1 = DataController.Instance.structureLockDataList_1.Find(x => x.buildingType == type);
                                    StructureLockProgressData progress1 = new StructureLockProgressData(type,
                                        data1.needMoney, data1.lockId, data.currentMapID);
                                    data.structureLockProgressDataList.Add(progress1);
                                    if (!data.structCanUnLockDataDic[data.currentMapID].Contains(type))
                                    {
                                        data.structCanUnLockDataDic[data.currentMapID].Add(type);
                                    }
                                    if (data.structLockDataDic[data.currentMapID].Contains(type))
                                    {
                                        data.structLockDataDic[data.currentMapID].Remove(type);
                                    }
                                }
                                break;
                            case 2:
                                StructureLockData data2 = DataController.Instance.structureLockDataList_2.Find(x => x.buildingType == type);
                                StructureLockProgressData progress2 = new StructureLockProgressData(type,
                                    data2.needMoney, data2.lockId, data.currentMapID);
                                data.structureLockProgressDataList.Add(progress2);
                                if (!data.structCanUnLockDataDic[data.currentMapID].Contains(type))
                                {
                                    data.structCanUnLockDataDic[data.currentMapID].Add(type);
                                }
                                if (data.structLockDataDic[data.currentMapID].Contains(type))
                                {
                                    data.structLockDataDic[data.currentMapID].Remove(type);
                                }
                                break;
                            case 3:
                                StructureLockData data3 = DataController.Instance.structureLockDataList_3.Find(x => x.buildingType == type);
                                StructureLockProgressData progress3 = new StructureLockProgressData(type,
                                    data3.needMoney, data3.lockId, data.currentMapID);
                                data.structureLockProgressDataList.Add(progress3);
                                if (!data.structCanUnLockDataDic[data.currentMapID].Contains(type))
                                {
                                    data.structCanUnLockDataDic[data.currentMapID].Add(type);
                                }
                                if (data.structLockDataDic[data.currentMapID].Contains(type))
                                {
                                    data.structLockDataDic[data.currentMapID].Remove(type);
                                }
                                break;
                            case 4:
                                StructureLockData data4 = DataController.Instance.structureLockDataList_4.Find(x => x.buildingType == type);
                                StructureLockProgressData progress4 = new StructureLockProgressData(type,
                                    data4.needMoney, data4.lockId, data.currentMapID);
                                data.structureLockProgressDataList.Add(progress4);
                                if (!data.structCanUnLockDataDic[data.currentMapID].Contains(type))
                                {
                                    data.structCanUnLockDataDic[data.currentMapID].Add(type);
                                }
                                if (data.structLockDataDic[data.currentMapID].Contains(type))
                                {
                                    data.structLockDataDic[data.currentMapID].Remove(type);
                                }
                                break;
                            case 5:
                                StructureLockData data5 = DataController.Instance.structureLockDataList_5.Find(x => x.buildingType == type);
                                StructureLockProgressData progress5 = new StructureLockProgressData(type,
                                    data5.needMoney, data5.lockId, data.currentMapID);
                                data.structureLockProgressDataList.Add(progress5);
                                if (!data.structCanUnLockDataDic[data.currentMapID].Contains(type))
                                {
                                    data.structCanUnLockDataDic[data.currentMapID].Add(type);
                                }
                                if (data.structLockDataDic[data.currentMapID].Contains(type))
                                {
                                    data.structLockDataDic[data.currentMapID].Remove(type);
                                }
                                break;

                        }

                    }
                    else
                    {
                        if (data.taskProgressDic.ContainsKey(task.taskId))
                        {
                            data.taskProgressDic[task.taskId] += 1;
                        }
                    }
                }
                if (task.type == TaskType.Unlock)
                {
                    MonsterType monster = (MonsterType)task.aimId;
                    MapLockData data1 = null;
                    switch (data.currentMapID)
                    {
                        case 1:
                            data1 = DataController.Instance.mapLockDataList_1.Find(x => x.monsterType == monster);
                            break;
                        case 2:
                            data1 = DataController.Instance.mapLockDataList_2.Find(x => x.monsterType == monster);
                            break;
                        case 3:
                            data1 = DataController.Instance.mapLockDataList_3.Find(x => x.monsterType == monster);
                            break;
                        case 4:
                            data1 = DataController.Instance.mapLockDataList_4.Find(x => x.monsterType == monster);
                            break;
                        case 5:
                            data1 = DataController.Instance.mapLockDataList_5.Find(x => x.monsterType == monster);
                            break;
                    }

                    // data1 可能为 null（currentMapID 不在 1-5 范围内，或 Find 未找到匹配项）
                    if (data1 == null)
                    {
                        Debug.LogError("data1 == null");
                    }

                    var _data = data.mapLockDataProgressList.Find(x => x.monsterType == monster);
                    if (_data == null)
                    {
                        data.mapLockDataProgressList.Add(new MapLockDataProgress(monster, data.currentMapID, data1.lockId, false, 0, true));
                        EventCenter.Instance.TriggerEvent(EventMessages.UpdateMapLockState, monster);
                    }
                }
            }

            DataController.Instance.UpdateStructureLockInfo();
        }
        public void BeginJugmentRemainTime(params object[] args)
        {
            if (data.age < 18)
            {
                StartCoroutine(CheckMinorPlayStatusCoroutine());
            }
        }

        private IEnumerator CheckMinorPlayStatusCoroutine()
        {
            Debug.Log("[防沉迷检测] 开始检测防沉迷状态。");
            while (true)
            {
                yield return new WaitForSeconds(30f);
                DateTime now = DateTime.Now;

                bool isTimeValid = now.Hour >= 20 && now.Hour < 21;
                if (isTimeValid)
                {
                    int remainingMinutes = 60 - now.Minute;
                    if (remainingMinutes == 10)
                    {
                        UIController.Instance.Show<AttentionView>(
                            "\u3000\u3000尊敬的玩家，您当前账号为未成年人账号，已被纳入防沉迷系统。根据国家新闻出版署下发《关于防止未成年人沉迷网络游戏的通知》及《关于进一步严格管理 切实防止未成年人沉迷网络游戏的通知》的要求，您剩余游戏时间还有10分钟!",
                            "疲劳游戏提示");
                    }

                    Debug.Log($"[防沉迷检测] 当前可游玩，还剩约 {remainingMinutes} 分钟");
                }
                else
                {
                    Debug.Log("[防沉迷检测] 已超出允许时间段，执行强制下线。");
                    UIController.Instance.Show<ForceQuitView>(
                        "\u3000\u3000尊敬的玩家，您目前为未成年人账号，已被纳入防沉迷系统。根据国家新闻出版署下发《关于防止未成年人沉迷网络游戏的通知》及《关于进一步严格管理 切实防止未成年人沉迷网络游戏的通知》，本游戏严格控制未成年人使用游戏时段，仅每周五、周六、周日和法定节假日的20时至21时提供1小时网络游戏服务。您已经进入疲劳游戏时间，系统将强制下线。",
                        "疲劳游戏提示", (Action)ForceQuit);

                    yield break; // 停止检测
                }
            }
        }
        private void ForceQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; // 在编辑器中停止播放
#else
    Application.Quit(); // 在打包后的应用中退出
#endif
        }

        private void EnsureRuntimeWorldSaveData()
        {
            if (data.runtimeCustomerDataList == null)
            {
                data.runtimeCustomerDataList = new List<RuntimeCustomerData>();
            }

            if (data.runtimeProductionDataList == null)
            {
                data.runtimeProductionDataList = new List<RuntimeProductionData>();
            }

            if (data.runtimeProductionStationDataList == null)
            {
                data.runtimeProductionStationDataList = new List<RuntimeProductionStationData>();
            }

            if (data.runtimePlayerDropList == null)
            {
                data.runtimePlayerDropList = new List<RuntimeDropItemCount>();
            }

            if (data.runtimePlayerGoodsList == null)
            {
                data.runtimePlayerGoodsList = new List<RuntimeGoodsCount>();
            }
        }

        private bool TryGetRuntimeContext(out GameController gameController, out ScenePickupController pickupController)
        {
            gameController = FindObjectOfType<GameController>();
            pickupController = null;
            if (gameController == null)
            {
                return false;
            }

            var pickupControllers = FindObjectsOfType<ScenePickupController>();
            if (pickupControllers == null || pickupControllers.Length == 0)
            {
                return false;
            }

            pickupController = pickupControllers.FirstOrDefault(x =>
                x != null &&
                x.gameObject != null &&
                x.gameObject.scene.IsValid() &&
                x.gameObject.scene.name.StartsWith("Game_"));
            if (pickupController == null)
            {
                pickupController = pickupControllers.FirstOrDefault(x => x != null && x.gameObject != null);
            }

            return pickupController != null;
        }

        private void CaptureRuntimeWorldState()
        {
            EnsureRuntimeWorldSaveData();

            CapturePlayerInventory();

            if (!TryGetRuntimeContext(out var gameController, out var pickupController))
            {
                return;
            }

            int currentMapId = data.currentMapID;
            if (_runtimeRestoredMapId != currentMapId)
            {
                bool hasSavedRuntime = data.runtimeProductionDataList.Any(x => x.mapId == currentMapId) ||
                                       data.runtimeProductionStationDataList.Any(x => x.mapId == currentMapId) ||
                                       data.runtimeCustomerDataList.Any(x => x.mapId == currentMapId);
                if (hasSavedRuntime && pickupController.products.Count == 0)
                {
                    return;
                }
            }

            data.runtimeCustomerDataList.RemoveAll(x => x.mapId == currentMapId);
            data.runtimeProductionDataList.RemoveAll(x => x.mapId == currentMapId);
            data.runtimeProductionStationDataList.RemoveAll(x => x.mapId == currentMapId);

            var customers = FindObjectsOfType<CustomerController>();
            foreach (var customer in customers)
            {
                if (customer == null || !customer.gameObject.activeInHierarchy) continue;
                if (customer.data == null) continue;
                if (customer.salesStall == null) continue;

                BuildingType targetBuildingType = customer.salesStall.buildingType;
                if (targetBuildingType == BuildingType.None)
                {
                    targetBuildingType = customer.salesStall.structureType;
                }

                Vector3 pos = customer.transform.position;
                data.runtimeCustomerDataList.Add(new RuntimeCustomerData
                {
                    mapId = currentMapId,
                    customerType = customer.data.type,
                    goodsType = customer.goodsType,
                    targetBuildingType = targetBuildingType,
                    state = (int)customer.state,
                    posX = pos.x,
                    posY = pos.y,
                    posZ = pos.z
                });
            }

            var products = pickupController.products.ToArray();
            foreach (var pickup in products)
            {
                if (pickup == null || !pickup.gameObject.activeInHierarchy) continue;
                if (!(pickup is Production production)) continue;
                if (production.station == null) continue;
                if (!(production.station is StructureBase stationBase)) continue;
                if (production.isTaken) continue;
                if (production.state != ItemState.OnWorkbench && production.state != ItemState.OnShelf) continue;

                BuildingType stationBuildingType = stationBase.structureType;
                if (stationBase is SalesStall stall && stall.buildingType != BuildingType.None)
                {
                    stationBuildingType = stall.buildingType;
                }
                else if (stationBase is ProductionStation productionStation && productionStation.buildingType != BuildingType.None)
                {
                    stationBuildingType = productionStation.buildingType;
                }

                Vector3 pos = production.transform.position;
                data.runtimeProductionDataList.Add(new RuntimeProductionData
                {
                    mapId = currentMapId,
                    goodsType = production.goodsType,
                    value = production.value,
                    stationBuildingType = stationBuildingType,
                    state = (int)production.state,
                    canPickup = production.canPickup,
                    posX = pos.x,
                    posY = pos.y,
                    posZ = pos.z
                });
            }

            CaptureProductionStationMaterials(currentMapId, gameController);

            int runtimeProductCount = data.runtimeProductionDataList.Count(x => x.mapId == currentMapId);
            int runtimeStationCount = data.runtimeProductionStationDataList.Count(x => x.mapId == currentMapId);
            int runtimeDropCount = data.runtimePlayerDropList?.Count ?? 0;
            int runtimeGoodsCount = data.runtimePlayerGoodsList?.Count ?? 0;
            Debug.Log($"[RuntimeSave] map={currentMapId} products={runtimeProductCount} stations={runtimeStationCount} drop={runtimeDropCount} goods={runtimeGoodsCount}");
        }

        private void CapturePlayerInventory()
        {
            var player = FindObjectOfType<PlayerController>();
            if (player == null)
            {
                return;
            }

            if (_runtimeRestoredMapId != data.currentMapID)
            {
                bool hasSavedRuntime = (data.runtimePlayerDropList != null && data.runtimePlayerDropList.Count > 0) ||
                                       (data.runtimePlayerGoodsList != null && data.runtimePlayerGoodsList.Count > 0);
                bool playerEmpty = (player.dropDic == null || player.dropDic.Count == 0) &&
                                   (player.goodsDic == null || player.goodsDic.Count == 0);
                if (hasSavedRuntime && playerEmpty)
                {
                    return;
                }
            }

            data.runtimePlayerDropList = new List<RuntimeDropItemCount>();
            if (player.dropDic != null)
            {
                foreach (var kv in player.dropDic)
                {
                    if (kv.Value <= 0) continue;
                    data.runtimePlayerDropList.Add(new RuntimeDropItemCount
                    {
                        itemType = kv.Key,
                        count = kv.Value
                    });
                }
            }

            data.runtimePlayerGoodsList = new List<RuntimeGoodsCount>();
            if (player.goodsDic != null)
            {
                foreach (var kv in player.goodsDic)
                {
                    if (kv.Value <= 0) continue;
                    data.runtimePlayerGoodsList.Add(new RuntimeGoodsCount
                    {
                        goodsType = kv.Key,
                        count = kv.Value
                    });
                }
            }
        }

        private void CaptureProductionStationMaterials(int currentMapId, GameController gameController)
        {
            if (gameController == null || gameController.productionStationList == null)
            {
                return;
            }

            for (int i = 0; i < gameController.productionStationList.Count; i++)
            {
                var station = gameController.productionStationList[i];
                if (station == null) continue;

                if (station.currentMaterialCount <= 0) continue;

                BuildingType stationBuildingType = station.structureType;
                if (station.buildingType != BuildingType.None)
                {
                    stationBuildingType = station.buildingType;
                }

                data.runtimeProductionStationDataList.Add(new RuntimeProductionStationData
                {
                    mapId = currentMapId,
                    stationBuildingType = stationBuildingType,
                    currentMaterialCount = station.currentMaterialCount
                });
            }
        }

        private void HandleMapDataPrepared(params object[] args)
        {
            if (_runtimeRestoreCoroutine != null)
            {
                StopCoroutine(_runtimeRestoreCoroutine);
                _runtimeRestoreCoroutine = null;
            }

            _runtimeRestoredMapId = -1;
            _runtimeRestoreCoroutine = StartCoroutine(RestoreRuntimeWorldStateCoroutine());
        }

        private IEnumerator RestoreRuntimeWorldStateCoroutine()
        {
            float waitTime = 0f;
            const float maxWaitTime = 5f;
            while (waitTime < maxWaitTime)
            {
                var gameController = FindObjectOfType<GameController>();
                if (gameController != null &&
                    FindObjectOfType<ScenePickupController>() != null &&
                    FindObjectOfType<PlayerController>() != null)
                {
                    RestoreRuntimeWorldState();
                    _runtimeRestoreCoroutine = null;
                    yield break;
                }

                waitTime += Time.unscaledDeltaTime;
                yield return null;
            }

            RestoreRuntimeWorldState();
            _runtimeRestoreCoroutine = null;
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                SavePlayerDataAsync();
                SavePlayerDataToSever();
            }
        }

        private void OnApplicationQuit()
        {
            SavePlayerDataAsync();
            SavePlayerDataToSever();
        }

        private void RestoreRuntimeWorldState()
        {
            EnsureRuntimeWorldSaveData();

            if (!TryGetRuntimeContext(out var gameController, out var pickupController))
            {
                return;
            }

            int currentMapId = data.currentMapID;
            int restoreProductCount = data.runtimeProductionDataList.Count(x => x.mapId == currentMapId);
            int restoreStationCount = data.runtimeProductionStationDataList.Count(x => x.mapId == currentMapId);
            int restoreDropCount = data.runtimePlayerDropList?.Count ?? 0;
            int restoreGoodsCount = data.runtimePlayerGoodsList?.Count ?? 0;
            Debug.Log($"[RuntimeRestore] map={currentMapId} products={restoreProductCount} stations={restoreStationCount} drop={restoreDropCount} goods={restoreGoodsCount}");
            if (_runtimeRestoredMapId == currentMapId)
            {
                return;
            }

            RestoreProductsForCurrentMap(currentMapId, gameController, pickupController);
            RestoreCustomersForCurrentMap(currentMapId, gameController);
            RestoreProductionStationsForCurrentMap(currentMapId, gameController);
            RestorePlayerInventory();
            _runtimeRestoredMapId = currentMapId;
            ScheduleRuntimeSortingFix(gameController, pickupController);
        }

        private void RestoreCustomersForCurrentMap(int currentMapId, GameController gameController)
        {
            var customerFactory = FindObjectOfType<CustomerFactory>();
            if (customerFactory == null)
            {
                return;
            }

            var assetHandle = customerFactory.GetComponent<AssetHandle>();
            if (assetHandle == null)
            {
                return;
            }

            var customerSaves = data.runtimeCustomerDataList.Where(x => x.mapId == currentMapId).ToList();
            for (int i = 0; i < customerSaves.Count; i++)
            {
                var saved = customerSaves[i];

                SalesStall stall = null;
                if (gameController.buildings.TryGetValue(saved.targetBuildingType, out var structure))
                {
                    stall = structure as SalesStall;
                }

                if (stall == null && gameController.goodBuild.TryGetValue(saved.goodsType, out var stallByGoods))
                {
                    stall = stallByGoods as SalesStall;
                }

                if (stall == null) continue;
                if (gameController.unlockedBuildingTypes.Contains(stall.buildingType) == false &&
                    gameController.unlockedBuildingTypes.Contains(stall.structureType) == false)
                {
                    continue;
                }

                if (!DataController.Instance.customerDataDic.TryGetValue(saved.customerType, out var customerData))
                {
                    continue;
                }

                var prefab = assetHandle.Get<GameObject>(Extensions.GetCustomerResNameByType(saved.customerType));
                if (prefab == null)
                {
                    continue;
                }

                Vector3 spawnPos = new Vector3(saved.posX, saved.posY, saved.posZ);
                GameObject obj = Instantiate(prefab);
                obj.transform.position = spawnPos;

                var customer = obj.GetComponent<CustomerController>();
                if (customer == null)
                {
                    Destroy(obj);
                    continue;
                }

                customer.Init(customerData, saved.goodsType, stall, spawnPos, 0f, 0f);
                ApplySavedCustomerState(customer, (NpcState)saved.state, stall);
            }
        }

        private void ApplySavedCustomerState(CustomerController customer, NpcState savedState, SalesStall stall)
        {
            switch (savedState)
            {
                case NpcState.QianWangGouMai:
                default:
                    break;
                case NpcState.WaitGouMaiWanCheng:
                    customer.WaitPurchase();
                    EventCenter.Instance.TriggerEvent(EventMessages.CustomerArrivedSell, customer, stall);
                    break;
                case NpcState.QianWangShouYinTai:
                    customer.state = NpcState.QianWangShouYinTai;
                    customer.SetNextPosition();
                    customer.agent.SetDestination(customer.nextPosition);
                    break;
                case NpcState.JieZhangChengGong:
                case NpcState.Angry:
                    customer.state = savedState;
                    customer.SetNextPosition();
                    customer.agent.SetDestination(customer.nextPosition);
                    break;
            }
        }

        private void RestoreProductsForCurrentMap(int currentMapId, GameController gameController, ScenePickupController pickupController)
        {
            var productSaves = data.runtimeProductionDataList.Where(x => x.mapId == currentMapId).ToList();
            if (productSaves.Count == 0)
            {
                return;
            }

            for (int i = 0; i < productSaves.Count; i++)
            {
                var saved = productSaves[i];
                StructureBase stationBase = null;
                if (!gameController.buildings.TryGetValue(saved.stationBuildingType, out var structure))
                {
                    if (gameController.goodBuild != null &&
                        gameController.goodBuild.TryGetValue(saved.goodsType, out var stallByGoods))
                    {
                        structure = stallByGoods;
                    }
                    else if (gameController.productionStationList != null)
                    {
                        structure = gameController.productionStationList
                            .FirstOrDefault(s => s != null &&
                                                 (s.buildingType == saved.stationBuildingType ||
                                                  s.structureType == saved.stationBuildingType));
                    }
                }

                if (saved.state == (int)ItemState.OnShelf &&
                    !(structure is SalesStall) &&
                    gameController.goodBuild != null &&
                    gameController.goodBuild.TryGetValue(saved.goodsType, out var stallByGoodsForShelf) &&
                    stallByGoodsForShelf is SalesStall)
                {
                    structure = stallByGoodsForShelf;
                }

                if (!(structure is StructureBase resolvedStructure))
                {
                    continue;
                }

                stationBase = resolvedStructure;

                var assetHandle = stationBase.GetComponent<AssetHandle>();
                if (assetHandle == null)
                {
                    continue;
                }

                var prefab = assetHandle.Get<GameObject>("Production");
                if (prefab == null)
                {
                    continue;
                }

                GameObject obj = Instantiate(prefab);
                obj.transform.position = new Vector3(saved.posX, saved.posY, saved.posZ);

                var production = obj.GetComponent<Production>();
                if (production == null)
                {
                    Destroy(obj);
                    continue;
                }

                production.Init(saved.goodsType, saved.value);
                production.SetStation(stationBase);
                production.canPickup = saved.canPickup;
                production.isTaken = false;
                production.SetState((ItemState)saved.state);

                if (production.spriteRenderer != null)
                {
                    int baseOrder = 30000 - Mathf.RoundToInt(stationBase.transform.position.y * 100);
                    if (stationBase.sprite != null && stationBase.sprite.sortingOrder > 0)
                    {
                        baseOrder = stationBase.sprite.sortingOrder;
                    }

                    int orderOffset = 3;
                    if (production.state == ItemState.OnShelf || stationBase is SalesStall)
                    {
                        orderOffset = 2;
                    }
                    if (stationBase is ProductionStation stationWithGrid)
                    {
                        production.spriteRenderer.sortingOrder =
                            stationWithGrid.grid.GetSortingOrderByPosition(baseOrder, orderOffset, production.transform.position);
                    }
                    else if (stationBase is SalesStall stallWithGrid)
                    {
                        production.spriteRenderer.sortingOrder =
                            stallWithGrid.grid.GetSortingOrderByPosition(baseOrder, orderOffset, production.transform.position);
                    }
                    else if (stationBase is CashierCounter cashierWithGrid)
                    {
                        production.spriteRenderer.sortingOrder =
                            cashierWithGrid.grid.GetSortingOrderByPosition(baseOrder, orderOffset, production.transform.position);
                    }
                    else
                    {
                        production.spriteRenderer.sortingOrder = baseOrder + orderOffset;
                    }
                }

                if (stationBase is ProductionStation productionStation &&
                    production.state == ItemState.OnWorkbench)
                {
                    productionStation.productionList.Add(production);
                }

                if (stationBase is SalesStall salesStall &&
                    production.state == ItemState.OnShelf)
                {
                    salesStall.productList.Add(production);
                }
            }

            foreach (var station in gameController.productionStationList)
            {
                ReflowStationProducts(station);
            }

            foreach (var stall in gameController.salesStallList)
            {
                stall.productList.RemoveAll(x => x == null);
                stall.currentGoodsCount = stall.productList.Count;
                ReflowStallProducts(stall);
            }

            ApplyRuntimeSortingFix(gameController, pickupController);
        }

        private void ScheduleRuntimeSortingFix(GameController gameController, ScenePickupController pickupController)
        {
            if (gameController == null || pickupController == null)
            {
                return;
            }

            if (_runtimeLayerFixCoroutine != null)
            {
                StopCoroutine(_runtimeLayerFixCoroutine);
                _runtimeLayerFixCoroutine = null;
            }

            _runtimeLayerFixCoroutine = StartCoroutine(RuntimeSortingFixCoroutine(gameController, pickupController));
        }

        private IEnumerator RuntimeSortingFixCoroutine(GameController gameController, ScenePickupController pickupController)
        {
            yield return null;
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            ApplyRuntimeSortingFix(gameController, pickupController);
            _runtimeLayerFixCoroutine = null;
        }

        private void ApplyRuntimeSortingFix(GameController gameController, ScenePickupController pickupController)
        {
            foreach (var station in gameController.productionStationList)
            {
                if (station == null) continue;
                ReflowStationProducts(station);
            }

            foreach (var stall in gameController.salesStallList)
            {
                if (stall == null) continue;
                ReflowStallProducts(stall);
            }

            if (gameController.buildings.TryGetValue(BuildingType.LingZhangTai, out var cashierBase))
            {
                var cashier = cashierBase as CashierCounter;
                if (cashier != null)
                {
                    ReflowCashierCoins(cashier, pickupController);
                }
            }
        }

        private void ReflowStationProducts(ProductionStation station)
        {
            if (station == null) return;

            if (station.productPosition != null)
            {
                station.grid.basePosition = station.productPosition.position;
            }
            else
            {
                station.grid.basePosition = station.transform.position;
            }

            station.productionList.RemoveAll(p => p == null);
            station.productionList.Sort((a, b) =>
            {
                if (a == null && b == null) return 0;
                if (a == null) return -1;
                if (b == null) return 1;
                int yCompare = a.transform.position.y.CompareTo(b.transform.position.y);
                if (yCompare != 0) return yCompare;
                return a.transform.position.x.CompareTo(b.transform.position.x);
            });

            int baseOrder = 30000 - Mathf.RoundToInt(station.transform.position.y * 100);
            if (station.sprite != null && station.sprite.sortingOrder > 0)
            {
                baseOrder = station.sprite.sortingOrder;
            }

            for (int i = 0; i < station.productionList.Count; i++)
            {
                var product = station.productionList[i];
                if (product == null) continue;
                Vector2 pos = station.grid.GetPositionByIndex(i);
                product.transform.position = new Vector3(pos.x, pos.y, product.transform.position.z);
                if (product.spriteRenderer != null)
                {
                    product.spriteRenderer.sortingOrder =
                        station.grid.GetSortingOrderByIndex(baseOrder, 3, i);
                }
            }

            station.grid.currentIndex = station.productionList.Count;
        }

        private void ReflowCashierCoins(CashierCounter cashier, ScenePickupController pickupController)
        {
            if (cashier == null) return;

            if (cashier.content_2 != null && cashier.content_2.activeInHierarchy && cashier.exportTransform2 != null)
            {
                cashier.grid.basePosition = cashier.exportTransform2.position;
            }
            else if (cashier.exportTransform != null)
            {
                cashier.grid.basePosition = cashier.exportTransform.position;
            }

            cashier.coinList.RemoveAll(c => c == null);
            if (pickupController != null)
            {
                cashier.coinList.Clear();
                var products = pickupController.products.ToArray();
                for (int i = 0; i < products.Length; i++)
                {
                    if (!(products[i] is Production production)) continue;
                    if (production.station != cashier) continue;
                    if (production.goodsType != GoodsType.TongBi) continue;
                    if (production.state != ItemState.OnWorkbench) continue;
                    cashier.coinList.Add(production);
                }
            }

            cashier.coinList.Sort((a, b) =>
            {
                if (a == null && b == null) return 0;
                if (a == null) return -1;
                if (b == null) return 1;
                int yCompare = a.transform.position.y.CompareTo(b.transform.position.y);
                if (yCompare != 0) return yCompare;
                return a.transform.position.x.CompareTo(b.transform.position.x);
            });

            int baseOrder = 30000 - Mathf.RoundToInt(cashier.transform.position.y * 100);
            if (cashier.sprite != null && cashier.sprite.sortingOrder > 0)
            {
                baseOrder = cashier.sprite.sortingOrder;
            }

            for (int i = 0; i < cashier.coinList.Count; i++)
            {
                var coin = cashier.coinList[i];
                if (coin == null) continue;
                Vector2 pos = cashier.grid.GetPositionByIndex(i);
                coin.transform.position = new Vector3(pos.x, pos.y, coin.transform.position.z);
                if (coin.spriteRenderer != null)
                {
                    coin.spriteRenderer.sortingOrder =
                        cashier.grid.GetSortingOrderByIndex(baseOrder, 3, i);
                }
                cashier.RegisterCoin(coin);
            }

            cashier.grid.currentIndex = cashier.coinList.Count;
            cashier.SortCoinsByHeight();
        }

        private void ReflowStallProducts(SalesStall stall)
        {
            if (stall == null) return;

            if (stall.baseTransform != null)
            {
                stall.grid.basePosition = stall.baseTransform.position;
            }
            else
            {
                stall.grid.basePosition = stall.transform.position;
            }

            stall.productList.RemoveAll(p => p == null);
            stall.productList.Sort((a, b) =>
            {
                if (a == null && b == null) return 0;
                if (a == null) return -1;
                if (b == null) return 1;
                int yCompare = a.transform.position.y.CompareTo(b.transform.position.y);
                if (yCompare != 0) return yCompare;
                return a.transform.position.x.CompareTo(b.transform.position.x);
            });

            int baseOrder = 30000 - Mathf.RoundToInt(stall.transform.position.y * 100);
            if (stall.sprite != null && stall.sprite.sortingOrder > 0)
            {
                baseOrder = stall.sprite.sortingOrder;
            }

            for (int i = 0; i < stall.productList.Count; i++)
            {
                var product = stall.productList[i];
                if (product == null) continue;
                Vector2 pos = stall.grid.GetPositionByIndex(i);
                product.transform.position = new Vector3(pos.x, pos.y, product.transform.position.z);
                if (product.spriteRenderer != null)
                {
                    product.spriteRenderer.sortingOrder =
                        stall.grid.GetSortingOrderByIndex(baseOrder, 2, i);
                }
            }

            stall.currentGoodsCount = stall.productList.Count;
            stall.grid.currentIndex = stall.productList.Count;
        }

        private void RestoreProductionStationsForCurrentMap(int currentMapId, GameController gameController)
        {
            if (data.runtimeProductionStationDataList == null || data.runtimeProductionStationDataList.Count == 0)
            {
                return;
            }

            var stationSaves = data.runtimeProductionStationDataList.Where(x => x.mapId == currentMapId).ToList();
            if (stationSaves.Count == 0)
            {
                return;
            }

            for (int i = 0; i < stationSaves.Count; i++)
            {
                var saved = stationSaves[i];
                ProductionStation station = null;
                if (gameController.buildings.TryGetValue(saved.stationBuildingType, out var structure))
                {
                    station = structure as ProductionStation;
                }
                else if (gameController.productionStationList != null)
                {
                    station = gameController.productionStationList
                        .FirstOrDefault(s => s != null &&
                                             (s.buildingType == saved.stationBuildingType ||
                                              s.structureType == saved.stationBuildingType));
                }

                if (station == null)
                {
                    continue;
                }

                int materialCount = Mathf.Max(0, saved.currentMaterialCount);
                station.currentMaterialCount = materialCount;

                if (station.productionInfo == null)
                {
                    continue;
                }

                station.productionInfo.Init(materialCount, station);
                if (materialCount > 0)
                {
                    station.productionInfo.gameObject.SetActive(true);
                    station.productionInfo.StartProductionLoop(station, station.structureType);
                    if (station.icon != null && station.icon.AnimationState != null)
                    {
                        station.icon.AnimationState.SetAnimation(0, "animation", true);
                    }
                }
                else
                {
                    station.productionInfo.gameObject.SetActive(false);
                    if (station.icon != null && station.icon.AnimationState != null)
                    {
                        station.icon.AnimationState.ClearTracks();
                    }
                }
            }
        }

        private void RestorePlayerInventory()
        {
            var player = FindObjectOfType<PlayerController>();
            if (player == null)
            {
                return;
            }

            player.dropDic.Clear();
            if (data.runtimePlayerDropList != null)
            {
                foreach (var entry in data.runtimePlayerDropList)
                {
                    if (entry == null) continue;
                    if (entry.count <= 0) continue;
                    player.dropDic[entry.itemType] = entry.count;
                }
            }

            player.goodsDic.Clear();
            if (data.runtimePlayerGoodsList != null)
            {
                foreach (var entry in data.runtimePlayerGoodsList)
                {
                    if (entry == null) continue;
                    if (entry.count <= 0) continue;
                    player.goodsDic[entry.goodsType] = entry.count;
                }
            }

            if (player.playerInfo != null)
            {
                player.playerInfo.UpdateTxt();
            }
            else
            {
                EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerInfo);
            }
        }

        public async Task SavePlayerDataAsync()
        {
            CaptureRuntimeWorldState();
            var path = Path.Combine(Application.persistentDataPath, JsonFileName.PlayerData + "." + data.userAccount);
            await JsonUtil.SaveDataAsync(data, path);
        }
        public void SavePlayerDataToSever()
        {
            CaptureRuntimeWorldState();
            LoginUtil.Instance.SaveToServer();
        }

        public void BeginAutoSave()
        {
            StartCoroutine(AutoSaveCoroutine());
        }

        private IEnumerator AutoSaveCoroutine()
        {
            var wait = new WaitForSeconds(10f);
            // Delay first save to avoid empty snapshot
            yield return wait;
            while (true)
            {
                SavePlayerDataAsync();
                SavePlayerDataToSever();
                Debug.Log("[自动保存] 玩家数据已自动保存。");
                yield return wait;
            }
        }



        public bool AddRecordMoney(int money)
        {
            int level = 0;
            if (data.age < 8)
            {
                level = 0;
            }
            else if (data.age < 16)
            {
                level = 1;
            }
            else if (data.age < 18)
            {
                level = 2;
            }
            else
            {
                level = 3;
            }

            switch (level)
            {
                case 0:
                    UIController.Instance.Show<AttentionView>(
                               "\u3000\u3000尊敬的玩家，您当前账号为未成年人账号，已被纳入防沉迷系统。根据国家新闻出版署下发《关于防止未成年人沉迷网络游戏的通知》及《关于进一步严格管理 切实防止未成年人沉迷网络游戏的通知》的要求，游戏中未满8周岁的用户，无法充值。",
                               "不可充值提示");
                    return false;
                case 1:
                    if (money > 50)
                    {
                        UIController.Instance.Show<AttentionView>(
                            "\u3000\u3000尊敬的玩家，您当前账号为未成年人账号，已被纳入防沉迷系统。根据国家新闻出版署下发《关于防止未成年人沉迷网络游戏的通知》及《关于进一步严格管理 切实防止未成年人沉迷网络游戏的通知》的要求，游戏中8周岁以上未满16周岁的用户，单次充值金额不得超过50元人民币，每月充值金额累计不得超过200元人民币，您本次剩余充值50元人民币。",
                            "充值提示");
                        return false;
                    }

                    if (data.monthlyLimitMoney + money > 200)
                    {
                        UIController.Instance.Show<AttentionView>(
                            $"\u3000\u3000尊敬的玩家，您当前账号为未成年人账号，已被纳入防沉迷系统。根据国家新闻出版署下发《关于防止未成年人沉迷网络游戏的通知》及《关于进一步严格管理 切实防止未成年人沉迷网络游戏的通知》的要求，游戏中8周岁以上未满16周岁的用户，单次充值金额不得超过50元人民币，每月充值金额累计不得超过200元人民币。您本月剩余充值{(200 - data.monthlyLimitMoney)}元人民币，无法充值。",
                            "充值提示");
                        return false;
                    }

                    data.monthlyLimitMoney += money;
                    return true;
                case 2:
                    if (money > 100)
                    {
                        UIController.Instance.Show<AttentionView>(
                            $"\u3000\u3000尊敬的玩家，您当前账号为未成年人账号，已被纳入防沉迷系统。根据国家新闻出版署下发《关于防止未成年人沉迷网络游戏的通知》及《关于进一步严格管理 切实防止未成年人沉迷网络游戏的通知》的要求，游戏中16周岁以上未满18周岁的用户，单次充值金额不得超过100元人民币，每月充值金额累计不得超过400元人民币，您本次剩余充值100元人民币。",
                            "充值提示");
                        return false;
                    }

                    if (data.monthlyLimitMoney + money > 400)
                    {
                        UIController.Instance.Show<AttentionView>(
                            $"\u3000\u3000尊敬的玩家，您当前账号为未成年人账号，已被纳入防沉迷系统。根据国家新闻出版署下发《关于防止未成年人沉迷网络游戏的通知》及《关于进一步严格管理 切实防止未成年人沉迷网络游戏的通知》的要求，游戏中16周岁以上未满18周岁的用户，单次充值金额不得超过100元人民币，每月充值金额累计不得超过400元人民币，您本月剩余充值{(400 - data.monthlyLimitMoney)}元人民币，无法充值。",
                            "充值提示");
                        return false;
                    }

                    data.monthlyLimitMoney += money;
                    return true;
                case 3:
                    return true;
            }

            return true;
        }



        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="onSuccess"></param>
        /// <param name="onFailure"></param>
        public void Register(string username, string password, Action onSuccess, Action<string> onFailure)
        {
            // 创建新账户
            LoginUtil.Instance.RegisterCheck(username, password, (respon) =>
            {
                if (respon == null)
                {
                    onFailure?.Invoke("服务器无响应。");
                    return;
                }

                switch (respon.state) //1.是注册成功，2.是注册失败 3.是用户已存在
                {
                    case 1:
                        onSuccess?.Invoke();
                        break;
                    case 2:
                        onFailure?.Invoke("注册失败。");
                        break;
                    case 3:
                        onFailure?.Invoke("用户已存在！");
                        break;
                    default:
                        onFailure?.Invoke("注册失败。");
                        break;
                }
            });
        }

        /// <summary>
        /// 实名
        /// </summary>
        public void RealName(string idnum, string chinese, string fcmLvl,
            Action<ResponseRealName> callback)
        {
            LoginUtil.Instance.RealName(idnum, chinese, fcmLvl, (response) =>
            {
                if (response == null)
                {
                    Debug.Log("JSON解析失败，检查格式是否正确");
                    return;
                }

                callback?.Invoke(response);
            });
        }

        public void Login(string user, string password, Action<int> callback)
        {
            LoginUtil.Instance.LoginCheck(user, password, (respone) =>
             {
                 if (respone.state == 1)
                 {
                     if (respone.more != null && !string.IsNullOrEmpty(respone.more))
                     {
                         data = null;
                         data = JsonConvert.DeserializeObject<PlayerData>(respone.more, new JsonSerializerSettings
                         {
                             ObjectCreationHandling = ObjectCreationHandling.Replace
                         });
                         data.age = respone.age;
                         Debug.Log($"[RuntimeLoad] products={(data.runtimeProductionDataList?.Count ?? 0)} stations={(data.runtimeProductionStationDataList?.Count ?? 0)} drop={(data.runtimePlayerDropList?.Count ?? 0)} goods={(data.runtimePlayerGoodsList?.Count ?? 0)}");
                         SavePlayerDataAsync();
                     }
                     else
                     {
                         data = new PlayerData();
                         data.userAccount = user;
                         data.userPassword = password;

                         FiilOrderData();
                         SavePlayerDataAsync();
                         SavePlayerDataToSever();

                     }

                     EnsureRuntimeWorldSaveData();
                     _runtimeRestoredMapId = -1;

                     if (data.SeventRecentlyWeek != Extensions.GetCurrentWeekNumber())
                     {
                         data.sevenDayRecordList.Clear();
                         data.GetSevenDayRewardIndex = 0;
                         data.SeventRecentlyWeek = Extensions.GetCurrentWeekNumber();
                     }

                     DateTime now = DateTime.Now;
                     if (now.Year != data.lastTime.Year || now.Month != data.lastTime.Month)
                     {
                         data.monthlyLimitMoney = 0;
                     }
                     if (data.lastloginday != now.ToString("yyyy/MM/dd"))
                     {
                         data.useZhuanPanTotalValue = 0;
                         data.playLingBaoCount = 3;
                         data.lastloginday = now.ToString("yyyy/MM/dd");
                     }

                     callback?.Invoke(respone.fcm);
                     DataController.Instance.UpdateStructureLockInfo();
                     StartOrderAutoCheck();
                 }
                 else if (respone.state == 2)
                 {
                     UIController.Instance.Show<TipView>("登录失败!");
                 }
                 else if (respone.state == 3)
                 {
                     UIController.Instance.Show<TipView>("密码错误!");
                 }
                 else
                 {
                     UIController.Instance.Show<TipView>(respone.msg);
                 }
             });
        }

        public void FiilOrderData()
        {
            for (int i = 0; i < data.orderDataprogressList.Count - 4; i++)
            {
                var randomKey = DataController.Instance.orderDataDic.Keys.ElementAt(UnityEngine.Random.Range(0, DataController.Instance.orderDataDic.Count));
                var randomValue = DataController.Instance.orderDataDic[randomKey];
                var list = data.mapLockDataProgressList.FindAll(x => x.isUnlock == true);
                List<GoodsType> goodsTypeList = new List<GoodsType>();
                List<DropItemType> dropItemTypeList = new List<DropItemType>();
                foreach (var item in list)
                {
                    goodsTypeList.Add(Extensions.GetGoodsTypeByMonsterType(item.monsterType));
                    dropItemTypeList.Add(Extensions.GetDropTypeByMonsterType(item.monsterType));
                }
                data.orderDataprogressList.Add(new OrderDataProgress(randomKey,
                    new Dictionary<GoodsType, (int, int)>() { { goodsTypeList[UnityEngine.Random.Range(0, goodsTypeList.Count)], (0, randomValue.needNum) } },
                     new Dictionary<DropItemType, (int, int)>() { { dropItemTypeList[UnityEngine.Random.Range(0, dropItemTypeList.Count)], (0, randomValue.needNum) } }
                           ));
            }
        }
        public void AddOrderData()
        {
            if (data.orderDataprogressList.Count < 4)
            {
                var randomKey = DataController.Instance.orderDataDic.Keys.ElementAt(UnityEngine.Random.Range(0, DataController.Instance.orderDataDic.Count));
                var randomValue = DataController.Instance.orderDataDic[randomKey];
                var list = data.mapLockDataProgressList.FindAll(x => x.isUnlock == false);
                List<MonsterType> result = new List<MonsterType>();
                MapData mapData = DataController.Instance.mapDataDic[data.currentMapID];
                List<MapLockData> mapLockDataList = null;
                switch (data.currentMapID)
                {
                    case 1:
                        mapLockDataList = DataController.Instance.mapLockDataList_1;
                        break;
                    case 2:
                        mapLockDataList = DataController.Instance.mapLockDataList_2;
                        break;
                    case 3:
                        mapLockDataList = DataController.Instance.mapLockDataList_3;
                        break;
                    case 4:
                        mapLockDataList = DataController.Instance.mapLockDataList_4;
                        break;
                    case 5:
                        mapLockDataList = DataController.Instance.mapLockDataList_5;
                        break;
                }
                for (int i = 0; i < mapData.monsterTypeList.Count; i++)
                {
                    if (list.Find(x => x.monsterType == (MonsterType)mapData.monsterTypeList[i]) != null)
                    {
                        result.Add((MonsterType)mapData.monsterTypeList[i]);
                    }
                    else
                    {
                        var lockdata = mapLockDataList.Find(x => x.monsterType == (MonsterType)mapData.monsterTypeList[i]);
                        if (lockdata == null)
                        {
                            result.Add((MonsterType)mapData.monsterTypeList[i]);
                        }
                    }
                }

                List<GoodsType> goodsTypeList = new List<GoodsType>();
                List<DropItemType> dropItemTypeList = new List<DropItemType>();
                foreach (var item in result)
                {
                    GoodsType goodsType = Extensions.GetGoodsTypeByMonsterType(item);
                    if (goodsTypeList.Contains(goodsType) == false)
                    {
                        goodsTypeList.Add(goodsType);
                    }
                    DropItemType dropItemType = Extensions.GetDropTypeByMonsterType(item);
                    if (dropItemTypeList.Contains(dropItemType) == false)
                    {
                        dropItemTypeList.Add(dropItemType);
                    }
                }
                data.orderDataprogressList.Add(new OrderDataProgress(randomKey,
                    new Dictionary<GoodsType, (int, int)>() { { goodsTypeList[UnityEngine.Random.Range(0, goodsTypeList.Count)], (0, randomValue.needNum) } },
                     new Dictionary<DropItemType, (int, int)>() { { dropItemTypeList[UnityEngine.Random.Range(0, dropItemTypeList.Count)], (0, randomValue.needNum) } }
                           ));
            }
        }

        private Coroutine orderRefreshCoroutine;
        private Coroutine orderAutoCheckCoroutine;
        public int maxOrderCount = 4;
        public float refreshInterval = 180f;
        public float checkInterval = 5f; // 每 1 秒检测一次
        public float orderRefreshProgress;
        private float refreshTimer;
        public void StartOrderAutoCheck()
        {
            if (orderAutoCheckCoroutine == null)
            {
                orderAutoCheckCoroutine = StartCoroutine(OrderAutoCheckLoop());
            }
        }
        private bool IsOrderFull()
        {
            return data.orderDataprogressList.Count >= maxOrderCount;
        }

        public void TryStartOrderRefresh()
        {
            if (IsOrderFull())
                return;

            if (orderRefreshCoroutine == null)
            {
                orderRefreshCoroutine = StartCoroutine(OrderRefreshLoop());
            }
        }
        public void StopOrderRefresh()
        {
            if (orderRefreshCoroutine != null)
            {
                StopCoroutine(orderRefreshCoroutine);
                orderRefreshCoroutine = null;
            }
        }
        private IEnumerator OrderRefreshLoop()
        {
            refreshTimer = 0f;
            orderRefreshProgress = 0f;

            while (data.orderDataprogressList.Count < maxOrderCount)
            {
                refreshTimer = 0f;

                while (refreshTimer < refreshInterval)
                {
                    refreshTimer += Time.deltaTime;
                    orderRefreshProgress = Mathf.Clamp01(refreshTimer / refreshInterval);

                    yield return null; // 每帧更新进度
                }

                // 时间到了，生成一个订单
                if (data.orderDataprogressList.Count < maxOrderCount)
                {
                    AddOrderData();
                }

                // 重置进度，准备下一次
                refreshTimer = 0f;
                orderRefreshProgress = 0f;
            }

            orderRefreshProgress = 0f;
            orderRefreshCoroutine = null;
        }


        private IEnumerator OrderAutoCheckLoop()
        {
            while (true)
            {
                // 没满 → 确保刷新协程在跑
                if (data.orderDataprogressList.Count < maxOrderCount)
                {
                    TryStartOrderRefresh();
                }
                else
                {
                    // 满了 → 停止刷新
                    StopOrderRefresh();
                }

                // 关键！！必须让出时间
                yield return new WaitForSeconds(checkInterval);
            }
        }




        public void AddJinYuanBao(int value)
        {
            data.goldIngot += value;
            EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
        }

        public bool RemoveJinYuanBao(int value)
        {
            if (data.goldIngot >= value)
            {
                data.goldIngot -= value;
                EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void AddYinQian(int value)
        {
            data.tongbi += value;
            EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
            EventCenter.Instance.TriggerEvent(EventMessages.MakeTongBiTask, value);
        }

        public bool RemoveYinQian(int value)
        {
            if (data.tongbi >= value)
            {
                data.tongbi -= value;
                EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
                return true;
            }
            else
            {
                return false;
            }
        }
        public void UpgradeAccountLevel()
        {

            data.accountLevel += 1;
            UIController.Instance.Show<TipView>($"等级提升至{data.accountLevel}级！");
            if (data.accountLevel >= 2)
            {
                if (data.characterFunction == 0)
                {
                    data.characterFunction = 1;
                    EventCenter.Instance.TriggerEvent(EventMessages.UpdateFunctionState);
                }
                if (data.cardFunction == 0)
                {
                    data.cardFunction = 1;
                    EventCenter.Instance.TriggerEvent(EventMessages.UpdateFunctionState);
                }

            }

            if (data.accountLevel >= 5)
            {
                if (data.mapFunction == 0)
                {
                    data.mapFunction = 1;
                    EventCenter.Instance.TriggerEvent(EventMessages.UpdateFunctionState);
                }

                if (data.levelLockMapList.Contains(2))
                {
                    data.levelLockMapList.Remove(2);
                }
            }

            if (data.accountLevel >= 10)
            {
                if (data.levelLockMapList.Contains(3))
                {
                    data.levelLockMapList.Remove(3);
                }
            }

            if (data.accountLevel >= 12 && data.ordenFunction == 0)
            {
                data.ordenFunction = 1;
                EventCenter.Instance.TriggerEvent(EventMessages.UpdateFunctionState);
            }

            if (data.accountLevel >= 20)
            {
                if (data.levelLockMapList.Contains(4))
                {
                    data.levelLockMapList.Remove(4);
                }
            }

            if (data.accountLevel >= 30)
            {
                if (data.levelLockMapList.Contains(5))
                {
                    data.levelLockMapList.Remove(5);
                }
            }
        }

        public void UnlockEmployeeFunction()
        {
            data.employeeFunction = 1;
            EventCenter.Instance.TriggerEvent(EventMessages.UpdateFunctionState);
        }

        public void GetTaskReward(int rewardId)
        {
            RewardData rewardData = DataController.Instance.taskRewardDataDic[rewardId];
            data.jingMangZhu += rewardData.Jmz;
            data.taskPopCompleted += rewardData.Jmz;
            if (data.jingMangZhu >= WorldData.LevelRequirementDic[data.currentMapID])
            {
                data.jingMangZhu -= WorldData.LevelRequirementDic[data.currentMapID];
                UpgradeAccountLevel();
            }
            data.tongbi += rewardData.Tq;
            data.goldIngot += rewardData.Jyb;
            EventCenter.Instance.TriggerEvent(EventMessages.MakeTongBiTask, rewardData.Tq);
            UIController.Instance.Show<TipView>("任务奖励领取成功！");
            EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
            EventCenter.Instance.TriggerEvent(EventMessages.UpdateLevelProgress);
        }

        public void GetSevenDayReward(int day)
        {
            SevenDayRewardData _data = DataController.Instance.sevenDayRewardDataDic[day];
            data.goldIngot += _data.Jyb;
            data.tongbi += _data.Yq;
            data.lingJing += _data.Lj;
            data.sevenDayRecordList.Add(day);
            data.sevenDayRecordTime = DateTime.Now.ToString("yyyy/MM/dd");
            EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
            EventCenter.Instance.TriggerEvent(EventMessages.MakeTongBiTask, _data.Yq);
        }




        public Dictionary<int, int> LotteryCard(GiftpackData giftpack)
        {
            Dictionary<int, int> dict = new Dictionary<int, int>();
            DrawFixedCards(dict, giftpack.linYunNum, CardLevelType.LingYun);
            DrawFixedCards(dict, giftpack.xianYunNum, CardLevelType.XianYun);

            int already = giftpack.linYunNum + giftpack.xianYunNum;
            int needMore = giftpack.totalnum - already;
            for (int i = 0; i < needMore; i++)
            {
                int value = GetRandomCardIdByQuality(DrawQualityByPackLevel(giftpack.level));
                if (!dict.ContainsKey(value))
                {
                    dict.Add(value, 1);
                }
                else
                {
                    dict[value]++;
                }
            }
            foreach (var value in dict)
            {
                var card = data.cardUpProgressesList.FirstOrDefault(c => c.id == value.Key);
                if (card != null)
                {
                    card.currentNum += value.Value;
                }
                else
                {
                    data.cardUpProgressesList.Add(new CardUpProgress(value.Key, value.Value));
                }
            }
            return dict;
        }
        private void DrawFixedCards(Dictionary<int, int> dict, int count, CardLevelType type)
        {
            if (count <= 0) return;
            var pool = DataController.Instance.cardLevelDataList
                .Where(c => c.levelType == type)
                .ToList();

            for (int i = 0; i < count; i++)
            {
                var card = pool[UnityEngine.Random.Range(0, pool.Count)];

                if (!dict.ContainsKey(card.id))
                    dict[card.id] = 0;

                dict[card.id]++;
            }
        }
        private CardLevelType DrawQualityByPackLevel(int level)
        {
            int roll = UnityEngine.Random.Range(0, 100); // 0..99

            switch (level)
            {
                case 1: // 低级
                    if (roll < 80) return CardLevelType.FanPing;
                    return CardLevelType.LingYun; // 20%

                case 2:                                          // 中级
                    if (roll < 80) return CardLevelType.FanPing; // 0-79
                    if (roll < 95) return CardLevelType.LingYun; // 80-94 => 15%
                    return CardLevelType.XianYun;                // 95-99 => 5%

                case 3:                                          // 高级
                    if (roll < 75) return CardLevelType.FanPing; // 0-74 => 75%
                    if (roll < 90) return CardLevelType.LingYun; // 75-89 => 15%
                    return CardLevelType.XianYun;                // 90-99 => 10%

                default:
                    return CardLevelType.FanPing;
            }
        }
        private int GetRandomCardIdByQuality(CardLevelType levelType)
        {
            var pool = DataController.Instance.cardLevelDataList.Where(c => c.levelType == levelType).ToList();
            if (pool.Count == 0)
                return -1;
            var selected = pool[UnityEngine.Random.Range(0, pool.Count)];
            return selected.id;
        }



        public void HandleProduceTask(params object[] args)
        {
            GoodsType goodsType = (GoodsType)args[0];
            foreach (var _data in data.listenInTaskList)
            {
                if (_data.type == TaskType.Produce)
                {
                    if ((GoodsType)_data.aimId == goodsType)
                    {
                        if (data.taskProgressDic.ContainsKey(_data.taskId))
                        {
                            data.taskProgressDic[_data.taskId]++;
                        }
                        else
                        {
                            data.taskProgressDic.Add(_data.taskId, 1);
                        }
                    }
                }
            }
            EventCenter.Instance.TriggerEvent(EventMessages.UpdateTaskMainView);
        }

        public void HandleUpGradeStuctureTask(params object[] args)
        {
            BuildingType buildingType = (BuildingType)args[0];
            foreach (var _data in data.listenInTaskList)
            {
                if (_data.type == TaskType.Upgrade)
                {
                    if ((BuildingType)_data.aimId == buildingType)
                    {
                        if (data.taskProgressDic.ContainsKey(_data.taskId))
                        {
                            data.taskProgressDic[_data.taskId]++;
                        }
                        else
                        {
                            data.taskProgressDic.Add(_data.taskId, 1);
                        }
                    }
                    if (buildingType == BuildingType.YuShaHu_1 && data.guideStep == GuideStep.UpgradePot)
                    {
                        data.guideStep = GuideStep.Finished;
                        UIController.Instance.Show<PlayerGuide>();
                    }
                }
            }
            EventCenter.Instance.TriggerEvent(EventMessages.UpdateTaskMainView);
        }
        public void HandleConstructTask(params object[] args)
        {
            try
            {
                BuildingType buildingType = (BuildingType)args[0];
                if (PlayerDataModule.Instance.data.currentMapID == 1)
                {
                    if (buildingType == BuildingType.YuShaHu_1 && data.guideStep == GuideStep.BuildYushaPot)
                    {
                        PlayerDataModule.Instance.data.guideStep = GuideStep.BuildTeaStand;
                        UIController.Instance.Show<PlayerGuide>();
                    }
                    else if (buildingType == BuildingType.LingChaJia_1 && data.guideStep == GuideStep.BuildTeaStand)
                    {
                        PlayerDataModule.Instance.data.guideStep = GuideStep.CollectMaterial;
                        UIController.Instance.Show<PlayerGuide>();
                    }
                    else if (buildingType == BuildingType.LingZhangTai && data.guideStep == GuideStep.BuildAccountDesk)
                    {
                        PlayerDataModule.Instance.data.guideStep = GuideStep.TakeTea;
                        UIController.Instance.Show<PlayerGuide>();
                    }
                }

                foreach (var _data in data.listenInTaskList)
                {
                    if (_data.type == TaskType.Construct)
                    {
                        if ((BuildingType)_data.aimId == buildingType)
                        {
                            if (data.taskProgressDic.ContainsKey(_data.taskId))
                            {
                                data.taskProgressDic[_data.taskId]++;
                            }
                            else
                            {
                                data.taskProgressDic.Add(_data.taskId, 1);
                            }
                        }
                    }
                }

                if (buildingType == BuildingType.YuShaHu_1 || buildingType == BuildingType.YuShaHu_2 ||
                    buildingType == BuildingType.YuShaHu_3 || buildingType == BuildingType.YuShaHu_4
                    || buildingType == BuildingType.LianQiLu_1 || buildingType == BuildingType.LianQiLu_2 ||
                    buildingType == BuildingType.LianQiLu_3)
                {
                    if (data.ProductStationDataList.Find(x => x.buildingType == buildingType) != null)
                    {
                        Debug.LogError(" yj ==>  重复添加生产台数据");
                    }

                    data.ProductStationDataList.Add(new ProductStationData(buildingType, GameController.Instance.buildings[buildingType].GetComponent<ProductionStation>().goodsType));
                }

                if (buildingType == BuildingType.YunDiGe)
                {
                    if (data.deliverData == null)
                    {
                        if (data.employeeFunction == 0)
                        {
                            UnlockEmployeeFunction();
                        }
                        data.deliverData = new DeliverData();
                    }
                }

                if (buildingType == BuildingType.LingZhangTai)
                {
                    if (data.cashierData == null)
                    {
                        data.cashierData = new CashierData();
                    }
                }

                if (buildingType == BuildingType.LingChuGe_1)
                {
                    if (data.warehouselist.Find(x => x.warehouseCategoryType == WarehouseCategoryType.LingChuGe_1) ==
                        null)
                    {
                        data.warehouselist.Add(new WarehouseCategory(WarehouseCategoryType.LingChuGe_1));
                    }
                }

                if (buildingType == BuildingType.LingChuGe_2)
                {
                    if (data.warehouselist.Find(x => x.warehouseCategoryType == WarehouseCategoryType.LingChuGe_2) ==
                        null)
                    {
                        data.warehouselist.Add(new WarehouseCategory(WarehouseCategoryType.LingChuGe_2));
                    }
                }

                EventCenter.Instance.TriggerEvent(EventMessages.UpdateSturctureLockInfo);
                EventCenter.Instance.TriggerEvent(EventMessages.UpdateTaskMainView);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                Debug.LogException(e);
            }
        }

        public void HandleSellTask(params object[] args)
        {
            GoodsType goodsType = (GoodsType)args[0];
            int value = (int)args[1];
            foreach (var _data in data.listenInTaskList)
            {
                if (_data.type == TaskType.Sell)
                {
                    if ((GoodsType)_data.aimId == goodsType)
                    {
                        if (data.taskProgressDic.ContainsKey(_data.taskId))
                        {
                            data.taskProgressDic[_data.taskId] += value;
                        }
                        else
                        {
                            data.taskProgressDic.Add(_data.taskId, value);
                        }
                    }
                }
            }
            EventCenter.Instance.TriggerEvent(EventMessages.UpdateTaskMainView);
        }

        public void HandleHarvestTask(params object[] args)
        {
            DropItemType dropItemType = (DropItemType)args[0];
            foreach (var _data in data.listenInTaskList)
            {
                if (_data.type == TaskType.Harvest)
                {
                    if ((DropItemType)_data.aimId == dropItemType)
                    {
                        if (data.taskProgressDic.ContainsKey(_data.taskId))
                        {
                            data.taskProgressDic[_data.taskId]++;
                        }
                        else
                        {
                            data.taskProgressDic.Add(_data.taskId, 1);
                        }

                        if (data.taskProgressDic[_data.taskId] > _data.keyValue)
                        {
                            // if (dropItemType == DropItemType.ShuangYunZhiFragment && data.currentMapID == 1 &&
                            //     data.guideStep == GuideStep.CollectMaterial)
                            // {
                            //     data.guideStep = GuideStep.DeliverMaterial;
                            //     UIController.Instance.Show<PlayerGuide>();
                            // }
                        }

                    }
                }
            }
            if (dropItemType == DropItemType.ShuangYunZhiFragment && data.currentMapID == 1 &&
                                data.guideStep == GuideStep.CollectMaterial)
            {
                data.guideStep = GuideStep.DeliverMaterial;
                UIController.Instance.Show<PlayerGuide>();
            }
            EventCenter.Instance.TriggerEvent(EventMessages.UpdateTaskMainView);
        }

        public void HandleMakeTongBiTask(params object[] args)
        {
            int value = (int)args[0];

            foreach (var _data in data.listenInTaskList)
            {
                if (_data.type == TaskType.Makemoney)
                {
                    if (data.taskProgressDic.ContainsKey(_data.taskId))
                    {
                        data.taskProgressDic[_data.taskId] += value;
                    }
                    else
                    {
                        data.taskProgressDic.Add(_data.taskId, value);
                    }
                }
            }
            EventCenter.Instance.TriggerEvent(EventMessages.UpdateTaskMainView);
        }
        public void HandleUnLockMapTask(params object[] args)
        {
            MonsterType value = (MonsterType)args[0];
            foreach (var _data in data.listenInTaskList)
            {
                if (_data.type == TaskType.Unlock && (MonsterType)_data.aimId == value)
                {
                    if (data.taskProgressDic.ContainsKey(_data.taskId))
                    {
                        data.taskProgressDic[_data.taskId] += 1;
                    }
                    else
                    {
                        data.taskProgressDic.Add(_data.taskId, 1);
                    }
                }

            }
            EventCenter.Instance.TriggerEvent(EventMessages.UpdateTaskMainView);
        }

        internal void Login(string text1, string text2)
        {
            throw new NotImplementedException();
        }
    }

}
