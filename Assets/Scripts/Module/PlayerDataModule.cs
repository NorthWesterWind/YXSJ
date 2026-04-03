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
using Newtonsoft.Json.Linq;
using UnityEngine;
using Utils;
using View;
using Extensions = Utils.Extensions;

namespace Module
{
    public class PlayerDataModule : MonoSingleton<PlayerDataModule>
    {
        public PlayerData data = new();
        private Coroutine _runtimeRestoreCoroutine;
        private Coroutine _runtimeLayerFixCoroutine;
        private Coroutine _autoSaveCoroutine;
        private PlayerController _cachedPlayerController;
        private bool _inventoryCapturePending;
        private float _inventoryCaptureAt = -1f;
        private float _lastServerSaveRealtime = float.NegativeInfinity;
        private int _runtimeRestoredMapId = -1;
        private int _lastSpeedTimeSecond = -1;
        private bool _isSavingLocalData;
        private bool _pendingLocalSave;
        private const float InventoryCaptureDelaySeconds = 0.05f;
        private const float AutoSaveIntervalSeconds = 30f;
        private const float AutoUploadIntervalSeconds = 120f;

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
            ProcessPendingInventoryCapture();
            UpdateSpeedTimeCountdown();
        }

        private void ProcessPendingInventoryCapture()
        {
            if (!_inventoryCapturePending || Time.unscaledTime < _inventoryCaptureAt)
            {
                return;
            }

            _inventoryCapturePending = false;
            CapturePlayerInventory();
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

            _inventoryCapturePending = true;
            _inventoryCaptureAt = Time.unscaledTime + InventoryCaptureDelaySeconds;
        }

        public void FillStructureLockProgressData()
        {
            foreach (var task in data.listenInTaskList)
            {
                if (task.type == TaskType.Upgrade)
                {
                    BuildingType type = (BuildingType)task.aimId;
                    var stationData = GetProductStationData(type);
                    if (stationData != null)
                    {
                        SetTaskProgressAtLeast(task.taskId, stationData.priceLevel);
                    }
                    if (type == BuildingType.LingZhangTai)
                    {
                        if (data.cashierData != null)
                        {
                            SetTaskProgressAtLeast(task.taskId, data.cashierData.workspeedLevel);
                        }
                    }
                    if (type == BuildingType.YunDiGe)
                    {
                        if (data.deliverData == null)
                        {
                            data.deliverData = new DeliverData();
                        }
                        SetTaskProgressAtLeast(task.taskId, data.deliverData.speedLevel);
                    }
                    if (type == BuildingType.LingChuGe_1)
                    {
                        var warehouse = data.warehouselist.Find(x => x.warehouseCategoryType == WarehouseCategoryType.LingChuGe_1);
                        if (warehouse != null)
                        {
                            SetTaskProgressAtLeast(task.taskId, warehouse.atkLevel);
                        }
                    }
                    if (type == BuildingType.LingChuGe_2)
                    {
                        var warehouse = data.warehouselist.Find(x => x.warehouseCategoryType == WarehouseCategoryType.LingChuGe_2);
                        if (warehouse != null)
                        {
                            SetTaskProgressAtLeast(task.taskId, warehouse.atkLevel);
                        }
                    }
                }

                if (task.type == TaskType.Construct)
                {
                    BuildingType type = (BuildingType)task.aimId;
                    var _data = data.structureLockProgressDataList.Find(x => x.buildType == type && x.mapId == data.currentMapID);
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
                        SetTaskProgressAtLeast(task.taskId, 1);
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

                    var _data = data.mapLockDataProgressList.Find(x => x.monsterType == monster && x.mapId == data.currentMapID);
                    if (_data == null)
                    {
                        data.mapLockDataProgressList.Add(new MapLockDataProgress(monster, data.currentMapID, data1.lockId, false, 0, true));
                        EventCenter.Instance.TriggerEvent(EventMessages.UpdateMapLockState, monster);
                    }
                    else if (_data.isUnlock)
                    {
                        SetTaskProgressAtLeast(task.taskId, 1);
                    }
                }
            }

            DataController.Instance.UpdateStructureLockInfo();
        }

        private void SetTaskProgressAtLeast(int taskId, int value)
        {
            if (data.taskProgressDic.ContainsKey(taskId))
            {
                data.taskProgressDic[taskId] = Mathf.Max(data.taskProgressDic[taskId], value);
            }
            else
            {
                data.taskProgressDic.Add(taskId, value);
            }
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
                                       data.runtimeProductionStationDataList.Any(x => x.mapId == currentMapId);
                if (hasSavedRuntime && pickupController.products.Count == 0)
                {
                    return;
                }
            }

            data.runtimeCustomerDataList.Clear();
            data.runtimeProductionDataList.RemoveAll(x => x.mapId == currentMapId);
            data.runtimeProductionStationDataList.RemoveAll(x => x.mapId == currentMapId);

            var products = pickupController.products;
            for (int i = products.Count - 1; i >= 0; i--)
            {
                var pickup = products[i];
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
        }

        private void CapturePlayerInventory()
        {
            var player = GetCachedPlayerController();
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

        private PlayerController GetCachedPlayerController()
        {
            if (_cachedPlayerController == null)
            {
                _cachedPlayerController = FindObjectOfType<PlayerController>();
            }

            return _cachedPlayerController;
        }

        public void CaptureCurrentRuntimeState()
        {
            CaptureRuntimeWorldState();
        }

        public ProductStationData GetProductStationData(BuildingType buildingType, int mapId = -1)
        {
            if (data == null || buildingType == BuildingType.None)
            {
                return null;
            }

            NormalizeProductStationData();
            int targetMapId = mapId > 0 ? mapId : Mathf.Max(1, data.currentMapID);
            return data.ProductStationDataList.Find(x =>
                x != null &&
                x.mapId == targetMapId &&
                x.buildingType == buildingType);
        }

        public ProductStationData GetProductStationDataByGoods(GoodsType goodsType, int mapId = -1)
        {
            if (data == null || goodsType == GoodsType.None)
            {
                return null;
            }

            NormalizeProductStationData();
            int targetMapId = mapId > 0 ? mapId : Mathf.Max(1, data.currentMapID);
            return data.ProductStationDataList.Find(x =>
                x != null &&
                x.mapId == targetMapId &&
                x.goodsType == goodsType);
        }

        public ProductStationData GetOrCreateProductStationData(BuildingType buildingType, GoodsType goodsType, int mapId = -1)
        {
            if (data == null || buildingType == BuildingType.None)
            {
                return null;
            }

            int targetMapId = mapId > 0 ? mapId : Mathf.Max(1, data.currentMapID);
            var stationData = GetProductStationData(buildingType, targetMapId);
            if (stationData != null)
            {
                if (goodsType != GoodsType.None && stationData.goodsType == GoodsType.None)
                {
                    stationData.goodsType = goodsType;
                }

                return stationData;
            }

            stationData = new ProductStationData(targetMapId, buildingType, goodsType);
            data.ProductStationDataList.Add(stationData);
            return stationData;
        }

        public void ClearRuntimeProductionCache(int mapId = -1)
        {
            EnsureRuntimeWorldSaveData();

            if (mapId > 0)
            {
                data.runtimeProductionDataList.RemoveAll(x => x.mapId == mapId);
                data.runtimeProductionStationDataList.RemoveAll(x => x.mapId == mapId);
            }
            else
            {
                data.runtimeProductionDataList.Clear();
                data.runtimeProductionStationDataList.Clear();
            }

            _runtimeRestoredMapId = -1;
        }

        public void ClearRuntimePlayerInventoryCache(bool clearLiveInventory = true)
        {
            EnsureRuntimeWorldSaveData();
            data.runtimePlayerDropList.Clear();
            data.runtimePlayerGoodsList.Clear();
            _inventoryCapturePending = false;
            _inventoryCaptureAt = -1f;
            _runtimeRestoredMapId = -1;

            if (!clearLiveInventory)
            {
                return;
            }

            var player = GetCachedPlayerController();
            if (player == null)
            {
                return;
            }

            player.dropDic.Clear();
            player.goodsDic.Clear();
            player.currentCarryNum = 0;
            if (player.playerInfo != null)
            {
                player.playerInfo.UpdateTxt();
            }
            else
            {
                EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerInfo);
            }

            EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerCarryInfo);
        }

        public void ClearRuntimeStateForMapSwitch()
        {
            ClearRuntimeProductionCache();
            ClearRuntimePlayerInventoryCache();
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
                CaptureRuntimeWorldState();
                SavePlayerDataAsync(false);
                if (Time.realtimeSinceStartup - _lastServerSaveRealtime >= AutoUploadIntervalSeconds)
                {
                    SavePlayerDataToSever(false);
                }
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
            if (_runtimeRestoredMapId == currentMapId)
            {
                return;
            }

            data.runtimeCustomerDataList.Clear();
            RestoreProductsForCurrentMap(currentMapId, gameController, pickupController);
            RestoreProductionStationsForCurrentMap(currentMapId, gameController);
            RestorePlayerInventory();
            _runtimeRestoredMapId = currentMapId;
            ScheduleRuntimeSortingFix(gameController, pickupController);
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
            station.RefreshProductLayout();
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
                var products = pickupController.products;
                for (int i = products.Count - 1; i >= 0; i--)
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
            stall.RefreshProductLayout();
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
            var player = GetCachedPlayerController();
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

        public async Task SavePlayerDataAsync(bool captureRuntime = true)
        {
            if (captureRuntime)
            {
                CaptureRuntimeWorldState();
            }

            _pendingLocalSave = true;
            if (_isSavingLocalData)
            {
                return;
            }

            _isSavingLocalData = true;
            try
            {
                var path = Path.Combine(Application.persistentDataPath, JsonFileName.PlayerData + "." + data.userAccount);
                while (_pendingLocalSave)
                {
                    _pendingLocalSave = false;
                    await JsonUtil.SaveDataAsync(data, path);
                }
            }
            finally
            {
                _isSavingLocalData = false;
            }
        }
        public void SavePlayerDataToSever(bool captureRuntime = true)
        {
            if (captureRuntime)
            {
                CaptureRuntimeWorldState();
            }

            _lastServerSaveRealtime = Time.realtimeSinceStartup;
            LoginUtil.Instance.SaveToServer();
        }

        public void BeginAutoSave()
        {
            if (_autoSaveCoroutine != null)
            {
                return;
            }

            _autoSaveCoroutine = StartCoroutine(AutoSaveCoroutine());
        }

        private IEnumerator AutoSaveCoroutine()
        {
            var wait = new WaitForSecondsRealtime(AutoSaveIntervalSeconds);
            yield return wait;
            while (true)
            {
                CaptureRuntimeWorldState();
                SavePlayerDataAsync(false);
                if (Time.realtimeSinceStartup - _lastServerSaveRealtime >= AutoUploadIntervalSeconds)
                {
                    SavePlayerDataToSever(false);
                }
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
                         data = DeserializePlayerDataWithLegacyFallback(respone.more);
                         NormalizeOrderProgressData();
                         NormalizeCashierData();
                         NormalizeStructureUnlockData();
                         NormalizeProductStationData();
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
                         NormalizeCashierData();
                         NormalizeStructureUnlockData();
                         NormalizeProductStationData();
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
                     RefreshYuanBaoKuangDongDailyCountIfNeeded(now);
                     if (now.Year != data.lastTime.Year || now.Month != data.lastTime.Month)
                     {
                         data.monthlyLimitMoney = 0;
                     }
                     if (data.lastloginday != now.ToString("yyyy/MM/dd"))
                     {
                         data.todayUseZhuanPanNum = 0;
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
            while (data.orderDataprogressList.Count < 4)
            {
                var randomKey = DataController.Instance.orderDataDic.Keys.ElementAt(UnityEngine.Random.Range(0, DataController.Instance.orderDataDic.Count));
                var randomValue = DataController.Instance.orderDataDic[randomKey];
                var list = data.mapLockDataProgressList.FindAll(x => x.isUnlock == true && x.mapId == data.currentMapID);
                List<GoodsType> goodsTypeList = new List<GoodsType>();
                List<DropItemType> dropItemTypeList = new List<DropItemType>();
                foreach (var item in list)
                {
                    goodsTypeList.Add(Extensions.GetGoodsTypeByMonsterType(item.monsterType));
                    dropItemTypeList.Add(Extensions.GetDropTypeByMonsterType(item.monsterType));
                }

                if (goodsTypeList.Count == 0 || dropItemTypeList.Count == 0)
                {
                    break;
                }

                data.orderDataprogressList.Add(new OrderDataProgress(randomKey,
                    new Dictionary<GoodsType, OrderProgressValue>() { { goodsTypeList[UnityEngine.Random.Range(0, goodsTypeList.Count)], new OrderProgressValue(0, randomValue.needNum) } },
                     new Dictionary<DropItemType, OrderProgressValue>() { { dropItemTypeList[UnityEngine.Random.Range(0, dropItemTypeList.Count)], new OrderProgressValue(0, randomValue.needNum) } }
                           ));
            }
        }

        private PlayerData DeserializePlayerDataWithLegacyFallback(string json)
        {
            var settings = new JsonSerializerSettings
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace
            };

            try
            {
                return JsonConvert.DeserializeObject<PlayerData>(json, settings);
            }
            catch (JsonException ex)
            {
                Debug.LogWarning($"[PlayerDataLoad] Primary deserialize failed: {ex.Message}");
            }

            if (TryNormalizeLegacyOrderProgressJson(json, out string repairedJson))
            {
                try
                {
                    Debug.LogWarning("[PlayerDataLoad] Retrying with normalized legacy order progress JSON.");
                    return JsonConvert.DeserializeObject<PlayerData>(repairedJson, settings);
                }
                catch (JsonException ex)
                {
                    Debug.LogWarning($"[PlayerDataLoad] Normalized retry failed: {ex.Message}");
                }
            }

            if (TryRemoveOrderProgressJson(json, out string strippedJson))
            {
                Debug.LogWarning("[PlayerDataLoad] Falling back to empty order progress list for legacy save.");
                return JsonConvert.DeserializeObject<PlayerData>(strippedJson, settings);
            }

            return JsonConvert.DeserializeObject<PlayerData>(json, settings);
        }

        public void RefreshYuanBaoKuangDongDailyCountIfNeeded()
        {
            RefreshYuanBaoKuangDongDailyCountIfNeeded(DateTime.Now);
        }

        public void RefreshYuanBaoKuangDongDailyCountIfNeeded(DateTime now)
        {
            if (data == null)
            {
                return;
            }

            if (!ShouldRefreshYuanBaoKuangDong(now))
            {
                return;
            }

            data.lastRefrashTime = now.ToString("yyyy-MM-dd HH:mm:ss");
            data.remainCount = GetYuanBaoKuangDongDailyCount();
        }

        public DateTime GetYuanBaoKuangDongNextRefreshTime()
        {
            return GetYuanBaoKuangDongNextRefreshTime(DateTime.Now);
        }

        public DateTime GetYuanBaoKuangDongNextRefreshTime(DateTime now)
        {
            return now.Date.AddDays(1);
        }

        public string GetYuanBaoKuangDongNextRefreshText()
        {
            return GetYuanBaoKuangDongNextRefreshText(DateTime.Now);
        }

        public string GetYuanBaoKuangDongNextRefreshText(DateTime now)
        {
            DateTime nextRefreshTime = GetYuanBaoKuangDongNextRefreshTime(now);
            TimeSpan remainTime = nextRefreshTime - now;
            int totalMinutes = Mathf.Max(0, Mathf.CeilToInt((float)remainTime.TotalMinutes));
            int hours = totalMinutes / 60;
            int minutes = totalMinutes % 60;
            return $"还有{hours}小时{minutes}分钟刷新。";
        }

        private bool ShouldRefreshYuanBaoKuangDong(DateTime now)
        {
            if (string.IsNullOrEmpty(data.lastRefrashTime))
            {
                return true;
            }

            if (!DateTime.TryParse(data.lastRefrashTime, out DateTime lastRefreshTime))
            {
                return true;
            }

            return lastRefreshTime.Date < now.Date;
        }

        private int GetYuanBaoKuangDongDailyCount()
        {
            int count = 30;
            var cardProgress = data.cardUpProgressesList?.Find(x => x.developType == CardDevelopType.UpgradeGetYuanBaoLing);
            if (cardProgress != null)
            {
                count += cardProgress.level * 10;
            }

            return count;
        }

        private bool TryNormalizeLegacyOrderProgressJson(string json, out string repairedJson)
        {
            repairedJson = null;
            try
            {
                var root = JObject.Parse(json);
                if (root["orderDataprogressList"] is not JArray orderArray)
                {
                    return false;
                }

                foreach (var orderToken in orderArray)
                {
                    if (orderToken is not JObject orderObj)
                    {
                        continue;
                    }

                    orderObj["goodDic"] = NormalizeLegacyProgressDictionaryToken(orderObj["goodDic"]);
                    orderObj["dropDic"] = NormalizeLegacyProgressDictionaryToken(orderObj["dropDic"]);
                }

                repairedJson = root.ToString(Formatting.None);
                return true;
            }
            catch (JsonException ex)
            {
                Debug.LogWarning($"[PlayerDataLoad] Normalize legacy order progress JSON failed: {ex.Message}");
                return false;
            }
        }

        private bool TryRemoveOrderProgressJson(string json, out string strippedJson)
        {
            strippedJson = null;
            try
            {
                var root = JObject.Parse(json);
                root["orderDataprogressList"] = new JArray();
                strippedJson = root.ToString(Formatting.None);
                return true;
            }
            catch (JsonException ex)
            {
                Debug.LogWarning($"[PlayerDataLoad] Remove order progress JSON failed: {ex.Message}");
                return false;
            }
        }

        private JObject NormalizeLegacyProgressDictionaryToken(JToken token)
        {
            var result = new JObject();
            if (token == null || token.Type == JTokenType.Null)
            {
                return result;
            }

            if (token.Type == JTokenType.Object)
            {
                foreach (var property in ((JObject)token).Properties())
                {
                    result[property.Name] = NormalizeLegacyProgressValueToken(property.Value);
                }
                return result;
            }

            if (token.Type != JTokenType.Array)
            {
                return result;
            }

            foreach (var item in (JArray)token)
            {
                if (item is JObject obj)
                {
                    var keyToken = obj["Key"] ?? obj["key"];
                    var valueToken = obj["Value"] ?? obj["value"];
                    if (keyToken != null)
                    {
                        result[keyToken.ToString()] = NormalizeLegacyProgressValueToken(valueToken);
                        continue;
                    }

                    if (obj.Properties().Count() == 1)
                    {
                        var property = obj.Properties().First();
                        result[property.Name] = NormalizeLegacyProgressValueToken(property.Value);
                    }
                    continue;
                }

                if (item is JArray pair && pair.Count >= 2)
                {
                    result[pair[0]?.ToString() ?? string.Empty] = NormalizeLegacyProgressValueToken(pair[1]);
                }
            }

            return result;
        }

        private JObject NormalizeLegacyProgressValueToken(JToken token)
        {
            int current = 0;
            int target = 0;

            if (token is JArray array)
            {
                current = array.Count > 0 ? array[0]?.Value<int>() ?? 0 : 0;
                target = array.Count > 1 ? array[1]?.Value<int>() ?? 0 : 0;
            }
            else if (token is JObject obj)
            {
                current = obj["current"]?.Value<int>()
                          ?? obj["Current"]?.Value<int>()
                          ?? obj["Item1"]?.Value<int>()
                          ?? 0;
                target = obj["target"]?.Value<int>()
                         ?? obj["Target"]?.Value<int>()
                         ?? obj["Item2"]?.Value<int>()
                         ?? 0;
            }
            else if (token != null && token.Type == JTokenType.Integer)
            {
                current = token.Value<int>();
                target = current;
            }

            return new JObject
            {
                ["current"] = current,
                ["target"] = target
            };
        }

        private void NormalizeOrderProgressData()
        {
            if (data == null)
            {
                return;
            }

            data.orderDataprogressList ??= new List<OrderDataProgress>();
            foreach (var order in data.orderDataprogressList)
            {
                if (order == null)
                {
                    continue;
                }

                order.goodDic ??= new Dictionary<GoodsType, OrderProgressValue>();
                order.dropDic ??= new Dictionary<DropItemType, OrderProgressValue>();

                foreach (var key in order.goodDic.Keys.ToList())
                {
                    order.goodDic[key] ??= new OrderProgressValue();
                }

                foreach (var key in order.dropDic.Keys.ToList())
                {
                    order.dropDic[key] ??= new OrderProgressValue();
                }
            }
        }

        private void NormalizeCashierData()
        {
            if (data == null || data.cashierData == null)
            {
                return;
            }

            var cashierData = data.cashierData;
            cashierData.maxpeopleLevel = Mathf.Max(1, cashierData.maxpeopleLevel);
            cashierData.maxworkspeedLevel = Mathf.Max(1, cashierData.maxworkspeedLevel);
            cashierData.peopleLevel = Mathf.Clamp(Mathf.Max(1, cashierData.peopleLevel), 1, cashierData.maxpeopleLevel);
            cashierData.workspeedLevel = Mathf.Clamp(Mathf.Max(1, cashierData.workspeedLevel), 1, cashierData.maxworkspeedLevel);
            cashierData.totalNum = Mathf.Clamp(Mathf.Max(1, cashierData.totalNum, cashierData.peopleLevel), 1, cashierData.maxpeopleLevel);
            cashierData.workingNum = Mathf.Clamp(cashierData.workingNum, 0, cashierData.totalNum);

            if (cashierData.currentWorkingSpeed <= 0f)
            {
                cashierData.currentWorkingSpeed = Mathf.Max(0.05f, 5f - (cashierData.workspeedLevel - 1) * 0.05f);
            }

            cashierData.currentWorkingSpeed = (float)Math.Round(cashierData.currentWorkingSpeed, 2);
            cashierData.earning = Mathf.Max(1f, cashierData.earning);
        }

        private void NormalizeProductStationData()
        {
            if (data == null)
            {
                return;
            }

            data.ProductStationDataList ??= new List<ProductStationData>();
            data.ProductStationDataList.RemoveAll(x => x == null || x.buildingType == BuildingType.None);

            int legacyMapId = Mathf.Max(1, data.currentMapID);
            List<ProductStationData> normalizedList = new List<ProductStationData>();
            foreach (var stationData in data.ProductStationDataList)
            {
                if (stationData.mapId <= 0)
                {
                    stationData.mapId = legacyMapId;
                }

                stationData.priceLevel = Mathf.Max(1, stationData.priceLevel);
                stationData.timelevel = Mathf.Max(1, stationData.timelevel);
                stationData.maxPriceLevel = Mathf.Max(1, stationData.maxPriceLevel);
                stationData.maxTimeLevel = Mathf.Max(1, stationData.maxTimeLevel);

                var existing = normalizedList.Find(x =>
                    x.mapId == stationData.mapId &&
                    x.buildingType == stationData.buildingType);
                if (existing == null)
                {
                    normalizedList.Add(stationData);
                    continue;
                }

                existing.priceLevel = Mathf.Max(existing.priceLevel, stationData.priceLevel);
                existing.timelevel = Mathf.Max(existing.timelevel, stationData.timelevel);
                existing.maxPriceLevel = Mathf.Max(existing.maxPriceLevel, stationData.maxPriceLevel);
                existing.maxTimeLevel = Mathf.Max(existing.maxTimeLevel, stationData.maxTimeLevel);
                if (existing.goodsType == GoodsType.None && stationData.goodsType != GoodsType.None)
                {
                    existing.goodsType = stationData.goodsType;
                }
            }

            data.ProductStationDataList = normalizedList;
        }

        private void NormalizeStructureUnlockData()
        {
            if (data == null)
            {
                return;
            }

            data.structLockDataDic ??= new Dictionary<int, List<BuildingType>>();
            data.structUnLockDataDic ??= new Dictionary<int, List<BuildingType>>();
            data.structCanUnLockDataDic ??= new Dictionary<int, List<BuildingType>>();

            for (int mapId = 1; mapId <= 5; mapId++)
            {
                if (!data.structLockDataDic.ContainsKey(mapId))
                {
                    data.structLockDataDic[mapId] = new List<BuildingType>();
                }

                if (!data.structUnLockDataDic.ContainsKey(mapId))
                {
                    data.structUnLockDataDic[mapId] = new List<BuildingType>();
                }

                if (!data.structCanUnLockDataDic.ContainsKey(mapId))
                {
                    data.structCanUnLockDataDic[mapId] = new List<BuildingType>();
                }
            }

            var realUnlockedByMap = new Dictionary<int, HashSet<BuildingType>>();
            for (int mapId = 1; mapId <= 5; mapId++)
            {
                realUnlockedByMap[mapId] = new HashSet<BuildingType>();
            }

            if (data.structureLockProgressDataList != null)
            {
                foreach (var progress in data.structureLockProgressDataList)
                {
                    if (progress == null || !progress.isUnlock)
                    {
                        continue;
                    }

                    if (!realUnlockedByMap.ContainsKey(progress.mapId))
                    {
                        realUnlockedByMap[progress.mapId] = new HashSet<BuildingType>();
                    }

                    realUnlockedByMap[progress.mapId].Add(progress.buildType);
                }
            }

            for (int mapId = 1; mapId <= 5; mapId++)
            {
                var sanitizedUnlocked = new HashSet<BuildingType>(GetDefaultUnlockedBuildingsForMap(mapId));
                foreach (var buildingType in realUnlockedByMap[mapId])
                {
                    sanitizedUnlocked.Add(buildingType);
                }

                data.structUnLockDataDic[mapId] = sanitizedUnlocked.ToList();
                data.structCanUnLockDataDic[mapId].RemoveAll(x => sanitizedUnlocked.Contains(x));
                data.structLockDataDic[mapId].RemoveAll(x => sanitizedUnlocked.Contains(x));
            }

            EnsureEmployeeFunctionUnlockedByYunDiGe();
        }

        public void RefreshStructureUnlockData()
        {
            NormalizeStructureUnlockData();
        }

        private void EnsureEmployeeFunctionUnlockedByYunDiGe()
        {
            if (data == null || data.employeeFunction == 1 || data.structUnLockDataDic == null)
            {
                return;
            }

            foreach (var unlockedList in data.structUnLockDataDic.Values)
            {
                if (unlockedList != null && unlockedList.Contains(BuildingType.YunDiGe))
                {
                    UnlockEmployeeFunction();
                    return;
                }
            }
        }

        private static IEnumerable<BuildingType> GetDefaultUnlockedBuildingsForMap(int mapId)
        {
            switch (mapId)
            {
                case 2:
                    yield return BuildingType.LingZhangTai;
                    yield return BuildingType.LingChaJia_1;
                    yield return BuildingType.YuShaHu_1;
                    yield break;
                case 3:
                    yield return BuildingType.LingZhangTai;
                    yield return BuildingType.YunDiGe;
                    yield return BuildingType.LingChuGe_1;
                    yield break;
                case 4:
                case 5:
                    yield return BuildingType.LingZhangTai;
                    yield return BuildingType.YunDiGe;
                    yield return BuildingType.LingChuGe_1;
                    yield return BuildingType.LingChuGe_2;
                    yield break;
                default:
                    yield break;
            }
        }

        public void AddOrderData()
        {
            if (data.orderDataprogressList.Count < 4)
            {
                var randomKey = DataController.Instance.orderDataDic.Keys.ElementAt(UnityEngine.Random.Range(0, DataController.Instance.orderDataDic.Count));
                var randomValue = DataController.Instance.orderDataDic[randomKey];
                var list = data.mapLockDataProgressList.FindAll(x => x.isUnlock == false && x.mapId == data.currentMapID);
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

                if (goodsTypeList.Count == 0 || dropItemTypeList.Count == 0)
                {
                    return;
                }

                data.orderDataprogressList.Add(new OrderDataProgress(randomKey,
                    new Dictionary<GoodsType, OrderProgressValue>() { { goodsTypeList[UnityEngine.Random.Range(0, goodsTypeList.Count)], new OrderProgressValue(0, randomValue.needNum) } },
                     new Dictionary<DropItemType, OrderProgressValue>() { { dropItemTypeList[UnityEngine.Random.Range(0, dropItemTypeList.Count)], new OrderProgressValue(0, randomValue.needNum) } }
                           ));

                EventCenter.Instance.TriggerEvent(EventMessages.UpdateOrderItem);
            }
        }

        private Coroutine orderRefreshCoroutine;
        private Coroutine orderAutoCheckCoroutine;
        public int maxOrderCount = 4;
        public float refreshInterval = 180f;
        public float checkInterval = 5f; 
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

                    yield return null; 
                }
                if (data.orderDataprogressList.Count < maxOrderCount)
                {
                    AddOrderData();
                }
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
                if (data.orderDataprogressList.Count < maxOrderCount)
                {
                    TryStartOrderRefresh();
                }
                else
                {
                    StopOrderRefresh();
                }
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
            UIController.Instance.Show<TipView>($"等级提升，获得翠芒珠x4！");
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
            data.star += rewardData.Jmz;
            data.taskPopCompleted += rewardData.Jmz;
            if (data.star >= WorldData.LevelRequirementDic[data.currentMapID])
            {
                data.star -= WorldData.LevelRequirementDic[data.currentMapID];
                data.talentPoint += 8;
                UpgradeAccountLevel();
            }
            data.tongbi += rewardData.Tq;
            data.goldIngot += rewardData.Jyb;
            EventCenter.Instance.TriggerEvent(EventMessages.MakeTongBiTask, rewardData.Tq);
            UIController.Instance.Show<TipView>("领取成功！");
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
            int roll = UnityEngine.Random.Range(0, 100); 

            switch (level)
            {
                case 1:
                    if (roll < 80) return CardLevelType.FanPing;
                    return CardLevelType.LingYun; 

                case 2:                                          
                    if (roll < 80) return CardLevelType.FanPing; 
                    if (roll < 95) return CardLevelType.LingYun; 
                    return CardLevelType.XianYun;                

                case 3:                                       
                    if (roll < 75) return CardLevelType.FanPing; 
                    if (roll < 90) return CardLevelType.LingYun; 
                    return CardLevelType.XianYun;               

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
                if (args == null || args.Length == 0 || args[0] is not BuildingType buildingType)
                {
                    Debug.LogWarning("[HandleConstructTask] Missing or invalid buildingType argument.");
                    return;
                }
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
                    if (GetProductStationData(buildingType) != null)
                    {
                     
                    }

                    GetOrCreateProductStationData(
                        buildingType,
                        GameController.Instance.buildings[buildingType].GetComponent<ProductionStation>().goodsType);
                }

                if (buildingType == BuildingType.YunDiGe)
                {
                    if (data.employeeFunction == 0)
                    {
                        UnlockEmployeeFunction();
                    }

                    if (data.deliverData == null)
                    {
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
