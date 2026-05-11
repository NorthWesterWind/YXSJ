using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Controller;
using Controller.Pickups;
using Controller.Player;
using Controller.Structure;
using Module.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        private Coroutine _runtimeRestoreVerifyCoroutine;
        private Coroutine _autoSaveCoroutine;
        private PlayerController _cachedPlayerController;
        private bool _inventoryCapturePending;
        private float _inventoryCaptureAt = -1f;
        private float _lastServerSaveRealtime = float.NegativeInfinity;
        private int _runtimeRestoredMapId = -1;
        private int _lastSpeedTimeSecond = -1;
        private bool _runtimeCaptureSuspended;
        private const float InventoryCaptureDelaySeconds = 0.05f;
        private const float AutoSaveIntervalSeconds = 5f;
        private const float AutoUploadIntervalSeconds = 5f;
        private const int YuanBaoKuangDongRefreshHours = 3;
        private const string YuanBaoKuangDongTimeFormat = "yyyy-MM-dd HH:mm:ss";

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
            if (_runtimeCaptureSuspended || !_inventoryCapturePending || Time.unscaledTime < _inventoryCaptureAt)
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
            if (_runtimeCaptureSuspended)
            {
                return;
            }

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
            if (data == null || DataController.Instance == null || data.listenInTaskList == null)
            {
                return;
            }

            data.structureLockProgressDataList ??= new List<StructureLockProgressData>();
            data.mapLockDataProgressList ??= new List<MapLockDataProgress>();
            NormalizeStructureUnlockData();

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
                    var progressData = data.structureLockProgressDataList.Find(x => x.buildType == type && x.mapId == data.currentMapID);
                    if (progressData == null)
                    {
                        if (data.currentMapID == 1 &&
                            (type == BuildingType.LingZhangTai || type == BuildingType.YuShaHu_1 ||
                             type == BuildingType.LingChaJia_1))
                        {
                            RemoveTaskProgress(task.taskId);
                            continue;
                        }

                        StructureLockData structureLockData = GetStructureLockData(type, data.currentMapID);
                        if (structureLockData == null)
                        {
                            Debug.LogWarning(
                                $"[FillStructureLockProgressData] Missing StructureLockData. mapId={data.currentMapID}, buildingType={type}, taskId={task.taskId}");
                            RemoveTaskProgress(task.taskId);
                            continue;
                        }

                        data.structureLockProgressDataList.Add(new StructureLockProgressData(
                            type,
                            structureLockData.needMoney,
                            structureLockData.lockId,
                            data.currentMapID));

                        if (data.structCanUnLockDataDic.TryGetValue(data.currentMapID, out var canUnlockList) &&
                            canUnlockList != null &&
                            !canUnlockList.Contains(type))
                        {
                            canUnlockList.Add(type);
                        }

                        if (data.structLockDataDic.TryGetValue(data.currentMapID, out var lockList) &&
                            lockList != null &&
                            lockList.Contains(type))
                        {
                            lockList.Remove(type);
                        }
                    }
                    else if (progressData.isUnlock)
                    {
                        SetTaskProgressAtLeast(task.taskId, 1);
                    }
                    else
                    {
                        RemoveTaskProgress(task.taskId);
                    }
                }
                if (task.type == TaskType.Unlock)
                {
                    MonsterType monster = (MonsterType)task.aimId;
                    MapLockData mapLockData = GetMapLockData(monster, data.currentMapID);
                    if (mapLockData == null)
                    {
                        Debug.LogWarning(
                            $"[FillStructureLockProgressData] Missing MapLockData. mapId={data.currentMapID}, monsterType={monster}, taskId={task.taskId}");
                        RemoveTaskProgress(task.taskId);
                        continue;
                    }

                    var mapProgressData = data.mapLockDataProgressList.Find(x => x.monsterType == monster && x.mapId == data.currentMapID);
                    if (mapProgressData == null)
                    {
                        data.mapLockDataProgressList.Add(new MapLockDataProgress(monster, data.currentMapID, mapLockData.lockId, false, 0, true));
                        EventCenter.Instance.TriggerEvent(EventMessages.UpdateMapLockState, monster);
                    }
                    else if (mapProgressData.isUnlock)
                    {
                        SetTaskProgressAtLeast(task.taskId, 1);
                    }
                }
            }

            DataController.Instance.UpdateStructureLockInfo();
        }

        public void RefreshTrackedTaskProgress()
        {
            if (data == null || DataController.Instance == null)
            {
                return;
            }

            if (data.listenInTaskList == null)
            {
                data.listenInTaskList = new List<TaskData>();
            }

            if (data.listenInTaskList.Count == 0)
            {
                var taskGroup = DataController.Instance.GetTaskGroupIds();
                if (taskGroup != null)
                {
                    data.listenInTaskList = taskGroup;
                }
            }

            if (data.listenInTaskList.Count == 0)
            {
                return;
            }

            TaskData trackedTask = data.listenInTaskList.Find(x => x.taskId == data.nowTaskId);
            RefreshSingleTaskProgress(trackedTask);
        }

        public void RefreshAllTrackedTaskProgress()
        {
            if (data == null || DataController.Instance == null)
            {
                return;
            }

            if (data.listenInTaskList == null)
            {
                data.listenInTaskList = new List<TaskData>();
            }

            if (data.listenInTaskList.Count == 0)
            {
                var taskGroup = DataController.Instance.GetTaskGroupIds();
                if (taskGroup != null)
                {
                    data.listenInTaskList = taskGroup;
                }
            }

            if (data.listenInTaskList.Count == 0)
            {
                return;
            }

            foreach (var task in data.listenInTaskList)
            {
                RefreshSingleTaskProgress(task);
            }
        }

        public float GetTotalBagCapacity()
        {
            if (data == null)
            {
                return 0f;
            }

            return data.bagCapacity + GetTalentBagCapacityBonus() + GetCurrentEquippedBagCapacity();
        }

        public float GetTalentBagCapacityBonus()
        {
            if (data == null || DataController.Instance == null || DataController.Instance.talentDataDic == null)
            {
                return data != null ? data.addBagCapacity : 0f;
            }

            float total = 0f;
            int maxTalentLevel = Mathf.Max(0, data.talentLevel);
            for (int i = 1; i <= maxTalentLevel; i++)
            {
                if (!DataController.Instance.talentDataDic.TryGetValue(i, out var talentData) || talentData == null)
                {
                    continue;
                }

                if (talentData.type != TalentType.BackpackCapacity)
                {
                    continue;
                }

                total += talentData.value;
            }

            return total;
        }

        public void RefreshTalentDerivedStats()
        {
            if (data == null)
            {
                return;
            }

            data.addBagCapacity = GetTalentBagCapacityBonus();
        }

        public float GetCurrentEquippedBagCapacity()
        {
            if (data == null)
            {
                return 0f;
            }

            if (!data.bagCapacityDataMigrated)
            {
                NormalizeBagCapacityData();
            }

            if (!TryGetBagCapacityById(data.currentBag, out float configuredCapacity))
            {
                return data.equippedBagCapacity;
            }

            if (!Mathf.Approximately(configuredCapacity, data.equippedBagCapacity))
            {
                data.equippedBagCapacity = configuredCapacity;
            }

            return data.equippedBagCapacity;
        }

        public float GetBagCapacityById(int bagId)
        {
            if (!TryGetBagCapacityById(bagId, out float capacity))
            {
                return 0f;
            }

            return capacity;
        }

        private void RefreshSingleTaskProgress(TaskData task)
        {
            if (task == null)
            {
                return;
            }

            switch (task.type)
            {
                case TaskType.Upgrade:
                    RefreshUpgradeTaskProgress(task);
                    break;
                case TaskType.Construct:
                    RefreshConstructTaskProgress(task);
                    break;
                case TaskType.Unlock:
                    RefreshUnlockTaskProgress(task);
                    break;
            }
        }

        private void RefreshUpgradeTaskProgress(TaskData task)
        {
            BuildingType type = (BuildingType)task.aimId;
            var stationData = GetProductStationData(type);
            if (stationData != null)
            {
                SetTaskProgressAtLeast(task.taskId, stationData.priceLevel);
            }

            if (type == BuildingType.LingZhangTai && data.cashierData != null)
            {
                SetTaskProgressAtLeast(task.taskId, data.cashierData.workspeedLevel);
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
                if (data.warehouselist == null)
                {
                    return;
                }
                var warehouse = data.warehouselist.Find(x => x.warehouseCategoryType == WarehouseCategoryType.LingChuGe_1);
                if (warehouse != null)
                {
                    SetTaskProgressAtLeast(task.taskId, warehouse.atkLevel);
                }
            }

            if (type == BuildingType.LingChuGe_2)
            {
                if (data.warehouselist == null)
                {
                    return;
                }
                var warehouse = data.warehouselist.Find(x => x.warehouseCategoryType == WarehouseCategoryType.LingChuGe_2);
                if (warehouse != null)
                {
                    SetTaskProgressAtLeast(task.taskId, warehouse.atkLevel);
                }
            }
        }

        private void RefreshConstructTaskProgress(TaskData task)
        {
            BuildingType type = (BuildingType)task.aimId;
            if (data.structureLockProgressDataList == null)
            {
                data.structureLockProgressDataList = new List<StructureLockProgressData>();
            }
            var progressData = data.structureLockProgressDataList.Find(x => x.buildType == type && x.mapId == data.currentMapID);
            if (progressData != null)
            {
                if (progressData.isUnlock)
                {
                    SetTaskProgressAtLeast(task.taskId, 1);
                }
                else
                {
                    RemoveTaskProgress(task.taskId);
                }
                return;
            }

            if (data.currentMapID == 1 &&
                (type == BuildingType.LingZhangTai || type == BuildingType.YuShaHu_1 || type == BuildingType.LingChaJia_1))
            {
                RemoveTaskProgress(task.taskId);
                return;
            }

            StructureLockData structureLockData = GetStructureLockData(type, data.currentMapID);
            if (structureLockData == null)
            {
                return;
            }

            data.structureLockProgressDataList.Add(new StructureLockProgressData(
                type,
                structureLockData.needMoney,
                structureLockData.lockId,
                data.currentMapID));

            if (data.structCanUnLockDataDic != null &&
                data.structCanUnLockDataDic.ContainsKey(data.currentMapID) &&
                !data.structCanUnLockDataDic[data.currentMapID].Contains(type))
            {
                data.structCanUnLockDataDic[data.currentMapID].Add(type);
            }

            if (data.structLockDataDic != null &&
                data.structLockDataDic.ContainsKey(data.currentMapID) &&
                data.structLockDataDic[data.currentMapID].Contains(type))
            {
                data.structLockDataDic[data.currentMapID].Remove(type);
            }

            DataController.Instance.UpdateStructureLockInfo();
        }

        private void RefreshUnlockTaskProgress(TaskData task)
        {
            MonsterType monster = (MonsterType)task.aimId;
            MapLockData mapLockData = GetMapLockData(monster, data.currentMapID);
            if (mapLockData == null)
            {
                return;
            }

            if (data.mapLockDataProgressList == null)
            {
                data.mapLockDataProgressList = new List<MapLockDataProgress>();
            }
            var progressData = data.mapLockDataProgressList.Find(x => x.monsterType == monster && x.mapId == data.currentMapID);
            if (progressData == null)
            {
                data.mapLockDataProgressList.Add(new MapLockDataProgress(monster, data.currentMapID, mapLockData.lockId, false, 0, true));
                EventCenter.Instance.TriggerEvent(EventMessages.UpdateMapLockState, monster);
                return;
            }

            if (progressData.isUnlock)
            {
                SetTaskProgressAtLeast(task.taskId, 1);
            }
        }

        private StructureLockData GetStructureLockData(BuildingType type, int mapId)
        {
            switch (mapId)
            {
                case 1:
                    return DataController.Instance.structureLockDataList_1.Find(x => x.buildingType == type);
                case 2:
                    return DataController.Instance.structureLockDataList_2.Find(x => x.buildingType == type);
                case 3:
                    return DataController.Instance.structureLockDataList_3.Find(x => x.buildingType == type);
                case 4:
                    return DataController.Instance.structureLockDataList_4.Find(x => x.buildingType == type);
                case 5:
                    return DataController.Instance.structureLockDataList_5.Find(x => x.buildingType == type);
                default:
                    return null;
            }
        }

        private MapLockData GetMapLockData(MonsterType monster, int mapId)
        {
            switch (mapId)
            {
                case 1:
                    return DataController.Instance.mapLockDataList_1.Find(x => x.monsterType == monster);
                case 2:
                    return DataController.Instance.mapLockDataList_2.Find(x => x.monsterType == monster);
                case 3:
                    return DataController.Instance.mapLockDataList_3.Find(x => x.monsterType == monster);
                case 4:
                    return DataController.Instance.mapLockDataList_4.Find(x => x.monsterType == monster);
                case 5:
                    return DataController.Instance.mapLockDataList_5.Find(x => x.monsterType == monster);
                default:
                    return null;
            }
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

        private void RemoveTaskProgress(int taskId)
        {
            if (data?.taskProgressDic == null)
            {
                return;
            }

            data.taskProgressDic.Remove(taskId);
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
                            "\u3000\u3000尊敬的玩家，您当前账号为未成年人账号，已被纳入防沉迷系统。根据国家新闻出版署下发《关于防止未成年人沉迷网络游戏的通知》及《关于进一步严格管理 切实防止未成年人沉迷网络游戏的通知》的要求，您剩余游戏时间不足10分钟!",
                            "剩余时间提示");
                    }

                    Debug.Log($"[防沉迷检测] 当前可游玩，还剩约{remainingMinutes}分钟");
                }
                else
                {
                    Debug.Log("[防沉迷检测] 已超出允许时间段，执行强制下线。");
                    UIController.Instance.Show<ForceQuitView>(
                        "\u3000\u3000尊敬的玩家，您目前为未成年人账号，已被纳入防沉迷系统。根据国家新闻出版署下发《关于防止未成年人沉迷网络游戏的通知》及《关于进一步严格管理 切实防止未成年人沉迷网络游戏的通知》的要求，每周五、周六、周日及法定节假日的20时至21时外为健康保护时段，当前时间段无法游玩，请合理安排时间。",
                        "健康游戏提示", (Action)ForceQuit);

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

            if (data.runtimeYuanBaoMonsterDataList == null)
            {
                data.runtimeYuanBaoMonsterDataList = new List<RuntimeYuanBaoMonsterData>();
            }

            if (data.runtimeYuanBaoDropDataList == null)
            {
                data.runtimeYuanBaoDropDataList = new List<RuntimeYuanBaoDropData>();
            }

            if (data.runtimeYuanBaoStateDataList == null)
            {
                data.runtimeYuanBaoStateDataList = new List<RuntimeYuanBaoStateData>();
            }
        }

        private bool TryGetRuntimeContext(out GameController gameController, out ScenePickupController pickupController)
        {
            gameController = null;
            pickupController = null;
            if (!TryGetExpectedRuntimeSceneName(out string expectedSceneName))
            {
                return false;
            }

            var gameControllers = FindObjectsOfType<GameController>(true);
            if (gameControllers != null)
            {
                gameController = gameControllers.FirstOrDefault(x =>
                    x != null &&
                    x.gameObject != null &&
                    x.gameObject.scene.IsValid() &&
                    x.gameObject.scene.name == expectedSceneName);
            }

            if (gameController == null)
            {
                return false;
            }

            var pickupControllers = FindObjectsOfType<ScenePickupController>(true);
            if (pickupControllers != null && pickupControllers.Length > 0)
            {
                pickupController = pickupControllers.FirstOrDefault(x =>
                    x != null &&
                    x.gameObject != null &&
                    x.gameObject.scene.IsValid() &&
                    x.gameObject.scene.name == expectedSceneName);
            }

            return true;
        }

        private bool TryGetExpectedRuntimeSceneName(out string sceneName)
        {
            sceneName = null;
            if (data == null || data.currentMapID <= 0)
            {
                return false;
            }

            sceneName = $"Game_{data.currentMapID}";
            return true;
        }

        private bool IsRuntimeCaptureSceneValid()
        {
            if (!TryGetExpectedRuntimeSceneName(out string expectedSceneName))
            {
                return false;
            }

            Scene activeScene = SceneManager.GetActiveScene();
            return activeScene.IsValid() && activeScene.name == expectedSceneName;
        }

        private void CaptureRuntimeWorldState()
        {
            EnsureRuntimeWorldSaveData();

            if (_runtimeCaptureSuspended)
            {
                return;
            }

            if (!IsRuntimeCaptureSceneValid())
            {
                return;
            }

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
                                       data.runtimeYuanBaoMonsterDataList.Any(x => x.mapId == currentMapId) ||
                                       data.runtimeYuanBaoDropDataList.Any(x => x.mapId == currentMapId) ||
                                       data.runtimeYuanBaoStateDataList.Any(x => x.mapId == currentMapId);
                if (hasSavedRuntime)
                {
                    return;
                }
            }

            data.runtimeCustomerDataList.Clear();
            data.runtimeProductionDataList.RemoveAll(x => x.mapId == currentMapId);
            data.runtimeProductionStationDataList.RemoveAll(x => x.mapId == currentMapId);
            data.runtimeYuanBaoMonsterDataList.RemoveAll(x => x.mapId == currentMapId);
            data.runtimeYuanBaoDropDataList.RemoveAll(x => x.mapId == currentMapId);
            data.runtimeYuanBaoStateDataList.RemoveAll(x => x.mapId == currentMapId);

            CaptureRuntimeProducts(currentMapId, gameController, pickupController);
            CaptureProductionStationMaterials(currentMapId, gameController);
            CaptureYuanBaoRuntimeState(currentMapId, gameController, pickupController);
        }

        private void CaptureRuntimeProducts(int currentMapId, GameController gameController, ScenePickupController pickupController)
        {
            HashSet<int> capturedIds = new HashSet<int>();

            if (gameController != null)
            {
                if (gameController.productionStationList != null)
                {
                    for (int i = 0; i < gameController.productionStationList.Count; i++)
                    {
                        var station = gameController.productionStationList[i];
                        if (station == null || station.productionList == null) continue;
                        for (int j = 0; j < station.productionList.Count; j++)
                        {
                            CaptureRuntimeProductFromContainer(
                                currentMapId,
                                station.productionList[j],
                                station,
                                ItemState.OnWorkbench,
                                capturedIds);
                        }
                    }
                }

                if (gameController.salesStallList != null)
                {
                    for (int i = 0; i < gameController.salesStallList.Count; i++)
                    {
                        var stall = gameController.salesStallList[i];
                        if (stall == null || stall.productList == null) continue;
                        for (int j = 0; j < stall.productList.Count; j++)
                        {
                            CaptureRuntimeProductFromContainer(
                                currentMapId,
                                stall.productList[j],
                                stall,
                                ItemState.OnShelf,
                                capturedIds);
                        }
                    }
                }

                if (gameController.buildings != null &&
                    gameController.buildings.TryGetValue(BuildingType.LingZhangTai, out var cashierBase) &&
                    cashierBase is CashierCounter cashier &&
                    cashier.coinList != null)
                {
                    for (int i = 0; i < cashier.coinList.Count; i++)
                    {
                        CaptureRuntimeProductFromContainer(
                            currentMapId,
                            cashier.coinList[i],
                            cashier,
                            ItemState.OnWorkbench,
                            capturedIds);
                    }
                }
            }

            if (pickupController == null || pickupController.products == null)
            {
                return;
            }

            for (int i = 0; i < pickupController.products.Count; i++)
            {
                if (pickupController.products[i] is not Production production) continue;
                CaptureRuntimeProductEntry(currentMapId, production, capturedIds);
            }
        }

        private void AddOrIncrementRuntimeProductionData(
            int currentMapId,
            GoodsType goodsType,
            int value,
            BuildingType stationBuildingType,
            ItemState state)
        {
            data.runtimeProductionDataList ??= new List<RuntimeProductionData>();

            int normalizedValue = goodsType == GoodsType.TongBi ? value : 0;
            int stateValue = (int)state;
            var saved = data.runtimeProductionDataList.Find(x =>
                x.mapId == currentMapId &&
                x.goodsType == goodsType &&
                x.stationBuildingType == stationBuildingType &&
                x.state == stateValue &&
                (goodsType == GoodsType.TongBi || x.value == normalizedValue));

            if (saved != null)
            {
                if (goodsType == GoodsType.TongBi)
                {
                    saved.value = Mathf.Max(0, saved.value) + Mathf.Max(0, normalizedValue);
                    saved.count = 1;
                    return;
                }

                saved.count = Mathf.Max(1, saved.count) + 1;
                return;
            }

            data.runtimeProductionDataList.Add(new RuntimeProductionData
            {
                mapId = currentMapId,
                goodsType = goodsType,
                value = normalizedValue,
                stationBuildingType = stationBuildingType,
                state = stateValue,
                count = 1
            });
        }

        private void AddOrIncrementRuntimeYuanBaoMonsterData(int currentMapId, int count)
        {
            if (count <= 0)
            {
                return;
            }

            data.runtimeYuanBaoMonsterDataList ??= new List<RuntimeYuanBaoMonsterData>();
            var saved = data.runtimeYuanBaoMonsterDataList.Find(x => x.mapId == currentMapId);
            if (saved != null)
            {
                saved.count = Mathf.Max(1, saved.count) + count;
                return;
            }

            data.runtimeYuanBaoMonsterDataList.Add(new RuntimeYuanBaoMonsterData
            {
                mapId = currentMapId,
                count = count
            });
        }

        private void AddOrIncrementRuntimeYuanBaoDropData(int currentMapId, int count)
        {
            if (count <= 0)
            {
                return;
            }

            data.runtimeYuanBaoDropDataList ??= new List<RuntimeYuanBaoDropData>();
            var saved = data.runtimeYuanBaoDropDataList.Find(x => x.mapId == currentMapId);
            if (saved != null)
            {
                saved.count = Mathf.Max(1, saved.count) + count;
                return;
            }

            data.runtimeYuanBaoDropDataList.Add(new RuntimeYuanBaoDropData
            {
                mapId = currentMapId,
                count = count
            });
        }

        private void CaptureRuntimeProductFromContainer(
            int currentMapId,
            Production production,
            StructureBase container,
            ItemState forcedState,
            HashSet<int> capturedIds)
        {
            if (production == null || container == null || !production.gameObject.activeInHierarchy)
            {
                return;
            }

            int instanceId = production.GetInstanceID();
            if (!capturedIds.Add(instanceId))
            {
                return;
            }

            BuildingType stationBuildingType = container.structureType;
            if (container is SalesStall stall && stall.buildingType != BuildingType.None)
            {
                stationBuildingType = stall.buildingType;
            }
            else if (container is ProductionStation productionStation && productionStation.buildingType != BuildingType.None)
            {
                stationBuildingType = productionStation.buildingType;
            }

            AddOrIncrementRuntimeProductionData(
                currentMapId,
                production.goodsType,
                production.value,
                stationBuildingType,
                forcedState);
        }

        private void CaptureRuntimeProductEntry(int currentMapId, Production production, HashSet<int> capturedIds)
        {
            if (production == null || !production.gameObject.activeInHierarchy)
            {
                return;
            }

            int instanceId = production.GetInstanceID();
            if (!capturedIds.Add(instanceId))
            {
                return;
            }

            if (production.station == null || production.station is not StructureBase stationBase)
            {
                return;
            }

            if (production.isTaken)
            {
                return;
            }

            if (production.state != ItemState.OnWorkbench && production.state != ItemState.OnShelf)
            {
                return;
            }

            BuildingType stationBuildingType = stationBase.structureType;
            if (stationBase is SalesStall stall && stall.buildingType != BuildingType.None)
            {
                stationBuildingType = stall.buildingType;
            }
            else if (stationBase is ProductionStation productionStation && productionStation.buildingType != BuildingType.None)
            {
                stationBuildingType = productionStation.buildingType;
            }

            AddOrIncrementRuntimeProductionData(
                currentMapId,
                production.goodsType,
                production.value,
                stationBuildingType,
                production.state);
        }

        private void CapturePlayerInventory()
        {
            if (!IsRuntimeCaptureSceneValid())
            {
                return;
            }

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

        private void CaptureYuanBaoRuntimeState(int currentMapId, GameController gameController, ScenePickupController pickupController)
        {
            EnsureYuanBaoKuangDongStateData();
            var state = GetOrCreateYuanBaoKuangDongState(currentMapId, DateTime.Now);
            data.runtimeYuanBaoStateDataList.RemoveAll(x => x.mapId == currentMapId);
            data.runtimeYuanBaoStateDataList.Add(new RuntimeYuanBaoStateData
            {
                mapId = currentMapId,
                generatedCount = state.generatedCount,
                remainCount = state.remainCount,
                lastRefreshTime = state.lastRefreshTime
            });

            FactoryController factory = FindYuanBaoFactory(gameController);
            if (factory != null)
            {
                int liveMonsterCount = factory.GetLiveMonsterPositions(MonsterType.JingYuanBao).Count;
                AddOrIncrementRuntimeYuanBaoMonsterData(currentMapId, liveMonsterCount);
            }

            if (pickupController == null || pickupController.materials == null)
            {
                return;
            }

            int liveDropCount = 0;
            for (int i = 0; i < pickupController.materials.Count; i++)
            {
                if (pickupController.materials[i] is not DropController drop) continue;
                if (!drop.gameObject.activeInHierarchy) continue;
                if (drop.isTaken) continue;
                if (drop.itemType != DropItemType.JingYuanBao) continue;
                liveDropCount += Mathf.Max(1, drop.count);
            }

            AddOrIncrementRuntimeYuanBaoDropData(currentMapId, liveDropCount);
        }

        private FactoryController FindYuanBaoFactory(GameController gameController)
        {
            if (gameController == null)
            {
                return null;
            }

            if (gameController.factoryControllers != null &&
                gameController.factoryControllers.TryGetValue(MonsterType.JingYuanBao, out var factory) &&
                factory != null)
            {
                return factory;
            }

            var scene = gameController.gameObject.scene;
            var factories = FindObjectsOfType<FactoryController>(true);
            for (int i = 0; i < factories.Length; i++)
            {
                var candidate = factories[i];
                if (candidate == null) continue;
                if (candidate.gameObject.scene != scene) continue;
                if (!candidate.isGoldenOnly) continue;
                return candidate;
            }

            return null;
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
            int currentMapId = data != null ? data.currentMapID : -1;
            int productCount = data?.runtimeProductionDataList?.Count(x => x.mapId == currentMapId) ?? 0;
            int stationCount = data?.runtimeProductionStationDataList?.Count(x => x.mapId == currentMapId && x.currentMaterialCount > 0) ?? 0;
            int yuanBaoMonsterCount = GetSavedYuanBaoMonsterCount(currentMapId);
            int yuanBaoDropCount = GetSavedYuanBaoDropCount(currentMapId);
            Debug.Log($"[RuntimeCapture] map={currentMapId} products={productCount} stations={stationCount} yuanBaoMonsters={yuanBaoMonsterCount} yuanBaoDrops={yuanBaoDropCount}");
            DebugLogManualRuntimeCaptureDetails();
        }

        private void DebugLogManualRuntimeCaptureDetails()
        {
            string activeSceneName = SceneManager.GetActiveScene().name;
            if (!TryGetRuntimeContext(out var gameController, out var pickupController))
            {
                Debug.Log($"[RuntimeCaptureDetail] activeScene={activeSceneName} runtimeContext=missing expectedScene=Game_{data?.currentMapID}");
                return;
            }

            int totalPickups = pickupController?.products?.Count ?? 0;
            int activeProductions = 0;
            int workbenchProducts = 0;
            int shelfProducts = 0;
            int missingStation = 0;
            int takenProducts = 0;
            int invalidStateProducts = 0;

            if ( pickupController != null &&pickupController.products != null)
            {
                for (int i = 0; i < pickupController.products.Count; i++)
                {
                    if (pickupController.products[i] is not Production production) continue;
                    if (production == null || !production.gameObject.activeInHierarchy) continue;

                    activeProductions++;
                    if (production.station == null)
                    {
                        missingStation++;
                    }
                    if (production.isTaken)
                    {
                        takenProducts++;
                    }

                    if (production.state == ItemState.OnWorkbench)
                    {
                        workbenchProducts++;
                    }
                    else if (production.state == ItemState.OnShelf)
                    {
                        shelfProducts++;
                    }
                    else
                    {
                        invalidStateProducts++;
                    }
                }
            }

            List<string> stationSummaries = new List<string>();
            if (gameController.productionStationList != null)
            {
                for (int i = 0; i < gameController.productionStationList.Count; i++)
                {
                    var station = gameController.productionStationList[i];
                    if (station == null) continue;
                    stationSummaries.Add($"{station.name}(build={station.buildingType},struct={station.structureType},mat={station.currentMaterialCount},products={station.productionList?.Count ?? 0})");
                }
            }

            List<string> stallSummaries = new List<string>();
            if (gameController.salesStallList != null)
            {
                for (int i = 0; i < gameController.salesStallList.Count; i++)
                {
                    var stall = gameController.salesStallList[i];
                    if (stall == null) continue;
                    stallSummaries.Add($"{stall.name}(build={stall.buildingType},goods={stall.currentGoodsType},count={stall.currentGoodsCount},list={stall.productList?.Count ?? 0})");
                }
            }

            Debug.Log($"[RuntimeCaptureDetail] activeScene={activeSceneName} expectedScene=Game_{data?.currentMapID} pickups={totalPickups} productions={activeProductions} workbench={workbenchProducts} shelf={shelfProducts} missingStation={missingStation} taken={takenProducts} invalidState={invalidStateProducts}");
            Debug.Log($"[RuntimeCaptureDetail] stations={string.Join(" | ", stationSummaries)}");
            Debug.Log($"[RuntimeCaptureDetail] stalls={string.Join(" | ", stallSummaries)}");
        }

        public void SuspendRuntimeCaptureForSceneTransition()
        {
            _runtimeCaptureSuspended = true;
            _inventoryCapturePending = false;
            _inventoryCaptureAt = -1f;
        }

        public void ResumeRuntimeCaptureForSceneTransition()
        {
            _runtimeCaptureSuspended = false;
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
                data.runtimeYuanBaoMonsterDataList.RemoveAll(x => x.mapId == mapId);
                data.runtimeYuanBaoDropDataList.RemoveAll(x => x.mapId == mapId);
                data.runtimeYuanBaoStateDataList.RemoveAll(x => x.mapId == mapId);
            }
            else
            {
                data.runtimeProductionDataList.Clear();
                data.runtimeProductionStationDataList.Clear();
                data.runtimeYuanBaoMonsterDataList.Clear();
                data.runtimeYuanBaoDropDataList.Clear();
                data.runtimeYuanBaoStateDataList.Clear();
            }

            _runtimeRestoredMapId = -1;
        }

        public void ClearRuntimeYuanBaoMonsterCache(int mapId = -1, bool clearDrops = false)
        {
            EnsureRuntimeWorldSaveData();

            if (mapId > 0)
            {
                data.runtimeYuanBaoMonsterDataList.RemoveAll(x => x.mapId == mapId);
                data.runtimeYuanBaoStateDataList.RemoveAll(x => x.mapId == mapId);
                if (clearDrops)
                {
                    data.runtimeYuanBaoDropDataList.RemoveAll(x => x.mapId == mapId);
                }
            }
            else
            {
                data.runtimeYuanBaoMonsterDataList.Clear();
                data.runtimeYuanBaoStateDataList.Clear();
                if (clearDrops)
                {
                    data.runtimeYuanBaoDropDataList.Clear();
                }
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

        public void ResetBusinessDataForCurrentMap()
        {
            if (data == null)
            {
                return;
            }

            ResetYuanBaoKuangDongCountForMapSwitch();
            ResetBusinessDataForMap(data.currentMapID);
        }

        private void ResetBusinessDataForMap(int mapId)
        {
            if (data == null || mapId <= 0)
            {
                return;
            }

            data.cashierData = null;
            data.deliverData = null;
            data.warehouselist = new List<WarehouseCategory>();

            if (!data.structUnLockDataDic.TryGetValue(mapId, out var unlockedBuildings) || unlockedBuildings == null)
            {
                return;
            }

            if (unlockedBuildings.Contains(BuildingType.LingZhangTai))
            {
                data.cashierData = new CashierData();
                NormalizeCashierData();
            }

            if (unlockedBuildings.Contains(BuildingType.YunDiGe))
            {
                data.deliverData = new DeliverData();
            }

            if (unlockedBuildings.Contains(BuildingType.LingChuGe_1))
            {
                data.warehouselist.Add(new WarehouseCategory(WarehouseCategoryType.LingChuGe_1));
            }

            if (unlockedBuildings.Contains(BuildingType.LingChuGe_2))
            {
                data.warehouselist.Add(new WarehouseCategory(WarehouseCategoryType.LingChuGe_2));
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
                    FindObjectOfType<PlayerController>() != null)
                {
                    RestoreRuntimeWorldState();
                    ResumeRuntimeCaptureForSceneTransition();
                    _runtimeRestoreCoroutine = null;
                    yield break;
                }

                waitTime += Time.unscaledDeltaTime;
                yield return null;
            }

            RestoreRuntimeWorldState();
            ResumeRuntimeCaptureForSceneTransition();
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
            RestoreYuanBaoRuntimeStateForCurrentMap(currentMapId, gameController, pickupController);
            RestorePlayerInventory();
            _runtimeRestoredMapId = currentMapId;
            Debug.Log($"[RuntimeRestore] map={currentMapId} savedProducts={(data.runtimeProductionDataList?.Count(x => x.mapId == currentMapId) ?? 0)} savedStations={(data.runtimeProductionStationDataList?.Count(x => x.mapId == currentMapId && x.currentMaterialCount > 0) ?? 0)} savedYuanBaoMonsters={GetSavedYuanBaoMonsterCount(currentMapId)} savedYuanBaoDrops={GetSavedYuanBaoDropCount(currentMapId)} liveProducts={CountLiveRuntimeProducts(gameController, pickupController)} liveStations={CountLiveRuntimeStations(gameController)} liveYuanBaoMonsters={CountLiveYuanBaoMonsters(gameController)} liveYuanBaoDrops={CountLiveYuanBaoDrops(pickupController)}");
            ScheduleRuntimeSortingFix(gameController, pickupController);
            ScheduleRuntimeRestoreVerification();
        }

        private void ScheduleRuntimeRestoreVerification()
        {
            if (_runtimeRestoreVerifyCoroutine != null)
            {
                StopCoroutine(_runtimeRestoreVerifyCoroutine);
                _runtimeRestoreVerifyCoroutine = null;
            }

            _runtimeRestoreVerifyCoroutine = StartCoroutine(RuntimeRestoreVerificationCoroutine());
        }

        private IEnumerator RuntimeRestoreVerificationCoroutine()
        {
            yield return null;
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            VerifyAndRepairRuntimeWorldState();
            _runtimeRestoreVerifyCoroutine = null;
        }

        private void VerifyAndRepairRuntimeWorldState()
        {
            EnsureRuntimeWorldSaveData();

            if (!TryGetRuntimeContext(out var gameController, out var pickupController))
            {
                return;
            }

            int currentMapId = data.currentMapID;
            int savedProductCount = data.runtimeProductionDataList?.Count(x => x.mapId == currentMapId) ?? 0;
            int savedStationCount = data.runtimeProductionStationDataList?.Count(x => x.mapId == currentMapId && x.currentMaterialCount > 0) ?? 0;
            int savedYuanBaoMonsterCount = GetSavedYuanBaoMonsterCount(currentMapId);
            int savedYuanBaoDropCount = GetSavedYuanBaoDropCount(currentMapId);

            if (savedProductCount <= 0 &&
                savedStationCount <= 0 &&
                savedYuanBaoMonsterCount <= 0 &&
                savedYuanBaoDropCount <= 0)
            {
                return;
            }

            int liveProductCount = CountLiveRuntimeProducts(gameController, pickupController);
            int liveStationCount = CountLiveRuntimeStations(gameController);
            int liveYuanBaoMonsterCount = CountLiveYuanBaoMonsters(gameController);
            int liveYuanBaoDropCount = CountLiveYuanBaoDrops(pickupController);

            bool needRestoreProducts = savedProductCount > 0 && liveProductCount == 0;
            bool needRestoreStations = savedStationCount > 0 && liveStationCount == 0;
            bool needRestoreYuanBaoMonsters = savedYuanBaoMonsterCount > liveYuanBaoMonsterCount;
            bool needRestoreYuanBaoDrops = savedYuanBaoDropCount > liveYuanBaoDropCount;
            Debug.Log($"[RuntimeVerify] map={currentMapId} savedProducts={savedProductCount} savedStations={savedStationCount} savedYuanBaoMonsters={savedYuanBaoMonsterCount} savedYuanBaoDrops={savedYuanBaoDropCount} liveProducts={liveProductCount} liveStations={liveStationCount} liveYuanBaoMonsters={liveYuanBaoMonsterCount} liveYuanBaoDrops={liveYuanBaoDropCount} needProducts={needRestoreProducts} needStations={needRestoreStations} needYuanBaoMonsters={needRestoreYuanBaoMonsters} needYuanBaoDrops={needRestoreYuanBaoDrops}");
            if (!needRestoreProducts &&
                !needRestoreStations &&
                !needRestoreYuanBaoMonsters &&
                !needRestoreYuanBaoDrops)
            {
                return;
            }

            if (needRestoreProducts)
            {
                RestoreProductsForCurrentMap(currentMapId, gameController, pickupController);
            }

            if (needRestoreStations)
            {
                RestoreProductionStationsForCurrentMap(currentMapId, gameController);
            }

            if (needRestoreYuanBaoMonsters || needRestoreYuanBaoDrops)
            {
                RestoreYuanBaoRuntimeStateForCurrentMap(currentMapId, gameController, pickupController);
            }

            ScheduleRuntimeSortingFix(gameController, pickupController);
        }

        private int CountLiveRuntimeProducts(GameController gameController, ScenePickupController pickupController)
        {
            HashSet<int> countedIds = new HashSet<int>();
            int count = 0;

            if (gameController != null)
            {
                if (gameController.productionStationList != null)
                {
                    for (int i = 0; i < gameController.productionStationList.Count; i++)
                    {
                        var station = gameController.productionStationList[i];
                        if (station == null || station.productionList == null) continue;
                        for (int j = 0; j < station.productionList.Count; j++)
                        {
                            if (IsLiveRuntimeProduct(station.productionList[j], countedIds))
                            {
                                count++;
                            }
                        }
                    }
                }

                if (gameController.salesStallList != null)
                {
                    for (int i = 0; i < gameController.salesStallList.Count; i++)
                    {
                        var stall = gameController.salesStallList[i];
                        if (stall == null || stall.productList == null) continue;
                        for (int j = 0; j < stall.productList.Count; j++)
                        {
                            if (IsLiveRuntimeProduct(stall.productList[j], countedIds))
                            {
                                count++;
                            }
                        }
                    }
                }

                if (gameController.buildings != null &&
                    gameController.buildings.TryGetValue(BuildingType.LingZhangTai, out var cashierBase) &&
                    cashierBase is CashierCounter cashier &&
                    cashier.coinList != null)
                {
                    for (int i = 0; i < cashier.coinList.Count; i++)
                    {
                        if (IsLiveRuntimeProduct(cashier.coinList[i], countedIds))
                        {
                            count++;
                        }
                    }
                }
            }

            if (pickupController == null || pickupController.products == null)
            {
                return count;
            }

            for (int i = 0; i < pickupController.products.Count; i++)
            {
                if (pickupController.products[i] is not Production production) continue;
                if (IsLiveRuntimeProduct(production, countedIds))
                {
                    count++;
                }
            }

            return count;
        }

        private bool IsLiveRuntimeProduct(Production production, HashSet<int> countedIds)
        {
            if (production == null || !production.gameObject.activeInHierarchy)
            {
                return false;
            }

            if (production.station is not StructureBase)
            {
                return false;
            }

            if (production.state != ItemState.OnWorkbench && production.state != ItemState.OnShelf)
            {
                return false;
            }

            return countedIds.Add(production.GetInstanceID());
        }

        private int CountLiveRuntimeStations(GameController gameController)
        {
            if (gameController == null || gameController.productionStationList == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < gameController.productionStationList.Count; i++)
            {
                var station = gameController.productionStationList[i];
                if (station == null) continue;
                if (station.currentMaterialCount <= 0) continue;
                count++;
            }

            return count;
        }

        private int CountLiveYuanBaoMonsters(GameController gameController)
        {
            var factory = FindYuanBaoFactory(gameController);
            if (factory == null)
            {
                return 0;
            }

            return factory.GetLiveMonsterPositions(MonsterType.JingYuanBao).Count;
        }

        private int CountLiveYuanBaoDrops(ScenePickupController pickupController)
        {
            if (pickupController == null || pickupController.materials == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < pickupController.materials.Count; i++)
            {
                if (pickupController.materials[i] is not DropController drop) continue;
                if (!drop.gameObject.activeInHierarchy) continue;
                if (drop.isTaken) continue;
                if (drop.itemType != DropItemType.JingYuanBao) continue;
                count += Mathf.Max(1, drop.count);
            }

            return count;
        }

        private int GetSavedYuanBaoMonsterCount(int mapId)
        {
            if (data?.runtimeYuanBaoMonsterDataList == null)
            {
                return 0;
            }

            return data.runtimeYuanBaoMonsterDataList
                .Where(x => x.mapId == mapId)
                .Sum(x => Mathf.Max(1, x.count));
        }

        private int GetSavedYuanBaoDropCount(int mapId)
        {
            if (data?.runtimeYuanBaoDropDataList == null)
            {
                return 0;
            }

            return data.runtimeYuanBaoDropDataList
                .Where(x => x.mapId == mapId)
                .Sum(x => Mathf.Max(1, x.count));
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

                int restoreCount = saved.goodsType == GoodsType.TongBi
                    ? (saved.value > 0 ? 1 : 0)
                    : Mathf.Max(1, saved.count);
                for (int restoreIndex = 0; restoreIndex < restoreCount; restoreIndex++)
                {
                    GameObject obj = Instantiate(prefab);
                    obj.transform.position = stationBase.transform.position;

                    var production = obj.GetComponent<Production>();
                    if (production == null)
                    {
                        Destroy(obj);
                        continue;
                    }

                    production.Init(saved.goodsType, saved.value);
                    production.SetStation(stationBase);
                    production.canPickup = saved.state == (int)ItemState.OnWorkbench || saved.state == (int)ItemState.OnShelf;
                    production.isTaken = false;
                    production.SetState((ItemState)saved.state);
                    if (pickupController != null && pickupController.products != null && !pickupController.products.Contains(production))
                    {
                        pickupController.products.Add(production);
                    }

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

                        if (stationBase is CashierCounter cashierWithGrid)
                        {
                            production.spriteRenderer.sortingOrder = cashierWithGrid.GetCoinSortingBaseOrder();
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

                    if (stationBase is CashierCounter cashierCounter &&
                        production.goodsType == GoodsType.TongBi &&
                        production.state == ItemState.OnWorkbench)
                    {
                        cashierCounter.RegisterCoin(production);
                    }
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
            if (gameController == null)
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
                    coin.spriteRenderer.sortingOrder = cashier.GetCoinSortingOrderByIndex(i);
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

        private void RestoreYuanBaoRuntimeStateForCurrentMap(int currentMapId, GameController gameController, ScenePickupController pickupController)
        {
            RestoreYuanBaoStateForCurrentMap(currentMapId);
            RestoreYuanBaoMonstersForCurrentMap(currentMapId, gameController);
            RestoreYuanBaoDropsForCurrentMap(currentMapId, gameController, pickupController);
        }

        private void RestoreYuanBaoStateForCurrentMap(int currentMapId)
        {
            EnsureRuntimeWorldSaveData();
            EnsureYuanBaoKuangDongStateData();

            var savedState = data.runtimeYuanBaoStateDataList.LastOrDefault(x => x.mapId == currentMapId);
            if (savedState == null)
            {
                return;
            }

            DateTime now = DateTime.Now;
            var state = GetOrCreateYuanBaoKuangDongState(currentMapId, now);

            DateTime currentRefreshTime;
            DateTime savedRefreshTime;
            bool hasCurrentRefresh = DateTime.TryParse(state.lastRefreshTime, out currentRefreshTime);
            bool hasSavedRefresh = DateTime.TryParse(savedState.lastRefreshTime, out savedRefreshTime);
            if (hasSavedRefresh && (!hasCurrentRefresh || savedRefreshTime >= currentRefreshTime))
            {
                state.lastRefreshTime = savedRefreshTime.ToString(YuanBaoKuangDongTimeFormat);
                state.generatedCount = Mathf.Max(0, savedState.generatedCount);
                state.remainCount = Mathf.Max(0, savedState.remainCount);
                NormalizeYuanBaoKuangDongState(state, now);
                if (currentMapId == Mathf.Max(1, data.currentMapID))
                {
                    SyncLegacyYuanBaoKuangDongState(state);
                }
            }
        }

        private void RestoreYuanBaoMonstersForCurrentMap(int currentMapId, GameController gameController)
        {
            int savedMonsterCount = GetSavedYuanBaoMonsterCount(currentMapId);
            if (savedMonsterCount <= 0)
            {
                return;
            }

            var factory = FindYuanBaoFactory(gameController);
            if (factory == null)
            {
                return;
            }

            int liveCount = CountLiveYuanBaoMonsters(gameController);
            for (int i = liveCount; i < savedMonsterCount; i++)
            {
                factory.SpawnRuntimeMonster(MonsterType.JingYuanBao);
            }
        }

        private void RestoreYuanBaoDropsForCurrentMap(int currentMapId, GameController gameController, ScenePickupController pickupController)
        {
            int savedDropCount = GetSavedYuanBaoDropCount(currentMapId);
            if (savedDropCount <= 0)
            {
                return;
            }

            var mine = FindYuanBaoKuangDong(gameController);
            if (mine == null)
            {
                return;
            }

            var assetHandle = mine.GetComponent<AssetHandle>();
            if (assetHandle == null)
            {
                return;
            }

            var prefab = assetHandle.Get<GameObject>("DropObj");
            if (prefab == null)
            {
                return;
            }

            int liveCount = CountLiveYuanBaoDrops(pickupController);
            int missingCount = savedDropCount - liveCount;
            if (missingCount <= 0)
            {
                return;
            }

            var obj = Instantiate(prefab);
            if (obj == null)
            {
                return;
            }

            obj.transform.position = GetRuntimeYuanBaoDropSpawnPosition(mine.transform.position);
            var drop = obj.GetComponent<DropController>();
            if (drop == null)
            {
                Destroy(obj);
                return;
            }

            drop.Init(DropItemType.JingYuanBao, missingCount);
            drop.canPickup = true;
            if (drop.spriteRenderer != null)
            {
                drop.spriteRenderer.sortingOrder = 30000 - Mathf.RoundToInt(obj.transform.position.y * 100f) + 2;
            }
        }

        private Vector3 GetRuntimeYuanBaoDropSpawnPosition(Vector3 center)
        {
            Vector2 offset = UnityEngine.Random.insideUnitCircle * 0.8f;
            return new Vector3(center.x + offset.x, center.y + offset.y, center.z);
        }

        private YuanBaoKuangDongCtr FindYuanBaoKuangDong(GameController gameController)
        {
            if (gameController == null)
            {
                return null;
            }

            if (gameController.buildings != null &&
                gameController.buildings.TryGetValue(BuildingType.YuanBaoKuangDong, out var structure) &&
                structure is YuanBaoKuangDongCtr mine)
            {
                return mine;
            }

            var scene = gameController.gameObject.scene;
            var mines = FindObjectsOfType<YuanBaoKuangDongCtr>(true);
            for (int i = 0; i < mines.Length; i++)
            {
                var candidate = mines[i];
                if (candidate == null) continue;
                if (candidate.gameObject.scene != scene) continue;
                return candidate;
            }

            return null;
        }

        private void RestorePlayerInventory()
        {
            var player = GetCachedPlayerController();
            if (player == null)
            {
                return;
            }

            player.dropDic.Clear();
            int carryCount = 0;
            if (data.runtimePlayerDropList != null)
            {
                foreach (var entry in data.runtimePlayerDropList)
                {
                    if (entry == null) continue;
                    if (entry.count <= 0) continue;
                    player.dropDic[entry.itemType] = entry.count;
                    carryCount += entry.count;
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
                    carryCount += entry.count;
                }
            }

            player.currentCarryNum = carryCount;

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

        public Task SavePlayerDataAsync(bool captureRuntime = true)
        {
            if (captureRuntime)
            {
                CaptureRuntimeWorldState();
            }
            return Task.CompletedTask;
        }
        public void SavePlayerDataToSever(bool captureRuntime = true)
        {
            if (captureRuntime)
            {
                CaptureRuntimeWorldState();
            }

            var snapshot = CreatePlayerDataSnapshot();
            if (snapshot == null)
            {
                return;
            }

            try
            {
                string snapshotJson = JsonConvert.SerializeObject(snapshot, Formatting.None);
                Debug.Log(
                    $"[RuntimeSave] user={snapshot.userAccount} digest={BuildCloudDataDigest(snapshotJson)} products={(snapshot.runtimeProductionDataList?.Count ?? 0)} stations={(snapshot.runtimeProductionStationDataList?.Count ?? 0)} playerDrop={(snapshot.runtimePlayerDropList?.Count ?? 0)} playerGoods={(snapshot.runtimePlayerGoodsList?.Count ?? 0)}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[RuntimeSave] Failed to build upload digest: {ex.Message}");
            }

            _lastServerSaveRealtime = Time.realtimeSinceStartup;
            LoginUtil.Instance.SaveToServer(snapshot);
        }

        public void StopAutoSave()
        {
            if (_autoSaveCoroutine == null)
            {
                return;
            }

            StopCoroutine(_autoSaveCoroutine);
            _autoSaveCoroutine = null;
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
                               "\u3000\u3000您当前账号为未满8周岁的未成年人账号，无法进行充值。",
                               "健康游戏提示");
                    return false;
                case 1:
                    if (money > 50)
                    {
                        UIController.Instance.Show<AttentionView>(
                            "\u3000\u3000您当前为8周岁以上未满16周岁的未成年人账号，本次单笔付费金额超过规定上限50元，无法购买。\r\n\u3000\u3000根据国家新闻出版署《关于防止未成年人沉迷网络游戏的通知》《关于进一步严格管理 切实防止未成年人沉迷网络游戏的通知》，未满8周岁：不提供付费服务；8周岁以上未满16周岁的未成年人用户：单笔付费不超过50元，每月累计不超过200元；16周岁以上的未成年人用户：单笔付费不超过100元，每月累计不超过400元。",
                            "健康游戏提示");
                        return false;
                    }

                    if (data.monthlyLimitMoney + money > 200)
                    {
                        UIController.Instance.Show<AttentionView>(
                            "\u3000\u3000您当前为8周岁以上未满16周岁的未成年人账号，购买此商品后，您当月交易的累计总额已达上限200元，暂无法购买。\r\n\u3000\u3000根据国家新闻出版署《关于防止未成年人沉迷网络游戏的通知》《关于进一步严格管理 切实防止未成年人沉迷网络游戏的通知》，未满8周岁：不提供付费服务；8周岁以上未满16周岁的未成年人用户：单笔付费不超过50元，每月累计不超过200元；16周岁以上的未成年人用户：单笔付费不超过100元，每月累计不超过400元。",
                            "健康游戏提示");
                        return false;
                    }

                    data.monthlyLimitMoney += money;
                      UIController.Instance.Show<AttentionView>(
                            $"\u3000\u3000您当前为8周岁以上未满16周岁的未成年人账号，您本月剩余充值金额为{Math.Max(0, 200 - data.monthlyLimitMoney)}元。\r\n\u3000\u3000根据国家新闻出版署《关于防止未成年人沉迷网络游戏的通知》《关于进一步严格管理 切实防止未成年人沉迷网络游戏的通知》，未满8周岁：不提供付费服务；8周岁以上未满16周岁的未成年人用户：单笔付费不超过50元，每月累计不超过200元；16周岁以上的未成年人用户：单笔付费不超过100元，每月累计不超过400元。",
                            "健康游戏提示");
                    return true;
                case 2:
                    if (money > 100)
                    {
                        UIController.Instance.Show<AttentionView>(
                            "\u3000\u3000您当前为16周岁以上未满18周岁的未成年人账号，本次单笔付费金额超过规定上限100元，无法购买。\r\n\u3000\u3000根据国家新闻出版署《关于防止未成年人沉迷网络游戏的通知》《关于进一步严格管理 切实防止未成年人沉迷网络游戏的通知》，未满8周岁：不提供付费服务；8周岁以上未满16周岁的未成年人用户：单笔付费不超过50元，每月累计不超过200元；16周岁以上的未成年人用户：单笔付费不超过100元，每月累计不超过400元。",
                            "健康游戏提示");
                        return false;
                    }

                    if (data.monthlyLimitMoney + money > 400)
                    {
                        UIController.Instance.Show<AttentionView>(
                            "\u3000\u3000您当前为16周岁以上未满18周岁的未成年人账号，购买此商品后，您当月交易的累计总额已达上限400元，暂无法购买。\r\n\u3000\u3000根据国家新闻出版署《关于防止未成年人沉迷网络游戏的通知》《关于进一步严格管理 切实防止未成年人沉迷网络游戏的通知》，未满8周岁：不提供付费服务；8周岁以上未满16周岁的未成年人用户：单笔付费不超过50元，每月累计不超过200元；16周岁以上的未成年人用户：单笔付费不超过100元，每月累计不超过400元。",
                            "健康游戏提示");
                        return false;
                    }

                    data.monthlyLimitMoney += money;
                     UIController.Instance.Show<AttentionView>(
                            $"\u3000\u3000您当前为16周岁以上未满18周岁的未成年人账号，您本月剩余充值金额为{Math.Max(0, 400 - data.monthlyLimitMoney)}元。\r\n\u3000\u3000根据国家新闻出版署《关于防止未成年人沉迷网络游戏的通知》《关于进一步严格管理 切实防止未成年人沉迷网络游戏的通知》，未满8周岁：不提供付费服务；8周岁以上未满16周岁的未成年人用户：单笔付费不超过50元，每月累计不超过200元；16周岁以上的未成年人用户：单笔付费不超过100元，每月累计不超过400元。",
                            "健康游戏提示");
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
            // 创建新账号
            LoginUtil.Instance.RegisterCheck(username, password, (respon) =>
            {
                if (respon == null)
                {
                    onFailure?.Invoke("服务器无响应。");
                    return;
                }

                switch (respon.state) // 1.注册成功，2.注册失败，3.用户已存在
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
        /// 实名认证
        /// </summary>
        public void RealName(string idnum, string chinese, string fcmLvl,
            Action<ResponseRealName> callback)
        {
            LoginUtil.Instance.RealName(idnum, chinese, fcmLvl, (response) =>
            {
                if (response == null)
                {
                    Debug.Log("JSON解析失败，请检查格式是否正确。");
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
                     bool hasServerSave = !string.IsNullOrWhiteSpace(respone.more);
                     if (hasServerSave)
                     {
                         if (TryDeserializePlayerDataWithLegacyFallback(respone.more, out var serverData))
                         {
                             data = serverData;
                             NormalizeOrderProgressData();
                             NormalizeCashierData();
                             NormalizeStructureUnlockData();
                             NormalizeWarehouseData();
                             NormalizeBagCapacityData();
                             NormalizeProductStationData();
                             Debug.Log($"[RuntimeLoad] source=server products={(data.runtimeProductionDataList?.Count ?? 0)} stations={(data.runtimeProductionStationDataList?.Count ?? 0)} drop={(data.runtimePlayerDropList?.Count ?? 0)} goods={(data.runtimePlayerGoodsList?.Count ?? 0)}");
                         }
                         else
                         {
                             Debug.LogError($"[RuntimeLoad] source=server_corrupted user={user} more={BuildCloudDataDigest(respone.more)}");
                             UIController.Instance.Show<TipView>("云存档数据损坏，无法登录该账号。");
                             return;
                         }
                     }
                     else
                     {
                         data = new PlayerData();
                         FiilOrderData();
                         NormalizeCashierData();
                         NormalizeStructureUnlockData();
                         NormalizeWarehouseData();
                         NormalizeBagCapacityData();
                         NormalizeProductStationData();
                         Debug.Log($"[RuntimeLoad] source=new_player user={user}");
                     }

                     ApplyLoginIdentity(respone, user, password);
                     SyncAgeFromLoginResponse(respone);
                     SavePlayerDataAsync();
                     if (!hasServerSave)
                     {
                         SavePlayerDataToSever();
                     }

                     if (false)
                     {
                         UIController.Instance.Show<TipView>("云存档损坏，已使用降级存档登录。");
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

                     callback?.Invoke(respone.GetResolvedFcm());
                     DataController.Instance.UpdateStructureLockInfo();
                     StartOrderAutoCheck();
                 }
                 else if (respone.state == 2)
                 {
                     UIController.Instance.Show<TipView>("登录失败！");
                 }
                 else if (respone.state == 3)
                 {
                     UIController.Instance.Show<TipView>("密码错误！");
                 }
                 else
                 {
                     UIController.Instance.Show<TipView>(respone.msg);
                 }
             });
        }

        private void ApplyLoginIdentity(ResponseLogin response, string fallbackAccount, string fallbackPassword)
        {
            if (data == null)
            {
                return;
            }

            string resolvedAccount = response != null ? response.GetResolvedAccount() : null;
            string resolvedPassword = response != null ? response.GetResolvedPassword() : null;
            data.userAccount = !string.IsNullOrWhiteSpace(resolvedAccount) ? resolvedAccount : fallbackAccount;
            data.userPassword = !string.IsNullOrWhiteSpace(resolvedPassword) ? resolvedPassword : fallbackPassword;

            int resolvedId = response != null ? response.GetResolvedId() : 0;
            if (resolvedId > 0)
            {
                data.user_id = resolvedId;
            }
        }

        private void SyncAgeFromLoginResponse(ResponseLogin response)
        {
            if (data == null || response == null)
            {
                return;
            }

            int resolvedAge = response.GetResolvedAge();
            if (resolvedAge > 0)
            {
                data.age = resolvedAge;
            }
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
                Debug.LogWarning($"[PlayerDataLoad] Primary deserialize failed: {ex.Message}; summary={BuildCloudDataDigest(json)}");
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
                    Debug.LogWarning($"[PlayerDataLoad] Normalized retry failed: {ex.Message}; summary={BuildCloudDataDigest(repairedJson)}");
                }
            }

            if (TryRemoveOrderProgressJson(json, out string strippedJson))
            {
                Debug.LogWarning("[PlayerDataLoad] Falling back to empty order progress list for legacy save.");
                return JsonConvert.DeserializeObject<PlayerData>(strippedJson, settings);
            }

            return JsonConvert.DeserializeObject<PlayerData>(json, settings);
        }

        private bool TryDeserializePlayerDataWithLegacyFallback(string json, out PlayerData playerData)
        {
            playerData = null;
            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            try
            {
                playerData = DeserializePlayerDataWithLegacyFallback(json);
                return playerData != null;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[PlayerDataLoad] Final deserialize failed: {ex.Message}; summary={BuildCloudDataDigest(json)}");
                return false;
            }
        }

        public void RefreshYuanBaoKuangDongDailyCountIfNeeded()
        {
            RefreshYuanBaoKuangDongDailyCountIfNeeded(DateTime.Now);
        }

        public void ResetYuanBaoKuangDongCountForMapSwitch()
        {
            ResetYuanBaoKuangDongCountForMapSwitch(DateTime.Now);
        }

        public void ResetYuanBaoKuangDongCountForMapSwitch(DateTime now)
        {
            RefreshYuanBaoKuangDongDailyCountIfNeeded(now);
        }

        public void RefreshYuanBaoKuangDongDailyCountIfNeeded(DateTime now)
        {
            if (data == null)
            {
                return;
            }

            GetOrCreateYuanBaoKuangDongState(Mathf.Max(1, data.currentMapID), now);
        }

        public int GetYuanBaoKuangDongRemainingCount(int mapId = -1)
        {
            if (data == null)
            {
                return 0;
            }

            int targetMapId = mapId > 0 ? mapId : Mathf.Max(1, data.currentMapID);
            return GetOrCreateYuanBaoKuangDongState(targetMapId, DateTime.Now).remainCount;
        }

        public int GetYuanBaoKuangDongGeneratedCount(int mapId = -1)
        {
            if (data == null)
            {
                return 0;
            }

            int targetMapId = mapId > 0 ? mapId : Mathf.Max(1, data.currentMapID);
            return GetOrCreateYuanBaoKuangDongState(targetMapId, DateTime.Now).generatedCount;
        }

        public bool TryConsumeYuanBaoKuangDongSpawnQuota(int mapId = -1)
        {
            if (data == null)
            {
                return false;
            }

            int targetMapId = mapId > 0 ? mapId : Mathf.Max(1, data.currentMapID);
            var state = GetOrCreateYuanBaoKuangDongState(targetMapId, DateTime.Now);
            int limit = GetYuanBaoKuangDongWindowCount();
            if (state.remainCount <= 0)
            {
                return false;
            }

            state.generatedCount = Mathf.Min(limit, state.generatedCount + 1);
            state.remainCount = Mathf.Clamp(limit - state.generatedCount, 0, limit);
            SyncLegacyYuanBaoKuangDongState(state);
            return true;
        }

        public DateTime GetYuanBaoKuangDongNextRefreshTime()
        {
            return GetYuanBaoKuangDongNextRefreshTime(DateTime.Now);
        }

        public DateTime GetYuanBaoKuangDongNextRefreshTime(DateTime now)
        {
            if (data == null)
            {
                return now.AddHours(YuanBaoKuangDongRefreshHours);
            }

            var state = GetOrCreateYuanBaoKuangDongState(Mathf.Max(1, data.currentMapID), now);
            if (!DateTime.TryParse(state.lastRefreshTime, out DateTime lastRefreshTime))
            {
                return now.AddHours(YuanBaoKuangDongRefreshHours);
            }

            return lastRefreshTime.AddHours(YuanBaoKuangDongRefreshHours);
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

        private string GetYuanBaoKuangDongNextRefreshText_Legacy(DateTime now)
        {
            return string.Empty;
        }

        private void EnsureYuanBaoKuangDongStateData()
        {
            if (data == null)
            {
                return;
            }

            data.yuanBaoKuangDongStateDataList ??= new List<YuanBaoKuangDongStateData>();
            if (data.yuanBaoKuangDongStateDataList.Count > 0)
            {
                return;
            }

            int mapId = Mathf.Max(1, data.currentMapID);
            int limit = GetYuanBaoKuangDongWindowCount();
            int remainCount = Mathf.Clamp(data.remainCount, 0, limit);
            data.yuanBaoKuangDongStateDataList.Add(new YuanBaoKuangDongStateData
            {
                mapId = mapId,
                generatedCount = Mathf.Clamp(limit - remainCount, 0, limit),
                remainCount = remainCount,
                lastRefreshTime = data.lastRefrashTime
            });
        }

        private YuanBaoKuangDongStateData GetOrCreateYuanBaoKuangDongState(int mapId, DateTime now)
        {
            EnsureYuanBaoKuangDongStateData();
            var state = data.yuanBaoKuangDongStateDataList.Find(x => x.mapId == mapId);
            if (state == null)
            {
                state = new YuanBaoKuangDongStateData
                {
                    mapId = mapId,
                    generatedCount = 0,
                    remainCount = GetYuanBaoKuangDongWindowCount(),
                    lastRefreshTime = now.ToString(YuanBaoKuangDongTimeFormat)
                };
                data.yuanBaoKuangDongStateDataList.Add(state);
            }

            NormalizeYuanBaoKuangDongState(state, now);
            if (mapId == Mathf.Max(1, data.currentMapID))
            {
                SyncLegacyYuanBaoKuangDongState(state);
            }

            return state;
        }

        private void NormalizeYuanBaoKuangDongState(YuanBaoKuangDongStateData state, DateTime now)
        {
            int limit = GetYuanBaoKuangDongWindowCount();
            DateTime windowStart;
            if (!DateTime.TryParse(state.lastRefreshTime, out windowStart))
            {
                windowStart = now;
            }

            while (windowStart.AddHours(YuanBaoKuangDongRefreshHours) <= now)
            {
                windowStart = windowStart.AddHours(YuanBaoKuangDongRefreshHours);
                state.generatedCount = 0;
                state.remainCount = limit;
            }

            int generatedFromRemain = Mathf.Clamp(limit - Mathf.Clamp(state.remainCount, 0, limit), 0, limit);
            state.generatedCount = Mathf.Clamp(Mathf.Max(state.generatedCount, generatedFromRemain), 0, limit);
            state.remainCount = Mathf.Clamp(limit - state.generatedCount, 0, limit);
            state.lastRefreshTime = windowStart.ToString(YuanBaoKuangDongTimeFormat);
        }

        private void SyncLegacyYuanBaoKuangDongState(YuanBaoKuangDongStateData state)
        {
            data.remainCount = state.remainCount;
            data.lastRefrashTime = state.lastRefreshTime;
        }

        private int GetYuanBaoKuangDongWindowCount()
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

        private void NormalizeWarehouseData()
        {
            if (data == null)
            {
                return;
            }

            const int defaultWarehouseCapacity = 60;
            const int defaultMaxAtkLevel = 60;
            const int defaultMaxNumLevel = 3;
            const float defaultWarehouseAtk = 10f;
            const float warehouseAtkLevelStep = 1.5f;

            data.warehouselist ??= new List<WarehouseCategory>();

            List<WarehouseCategoryType> expectedTypes = GetExpectedWarehouseTypesForCurrentMap();
            Dictionary<WarehouseCategoryType, WarehouseCategory> normalizedWarehouseMap = new Dictionary<WarehouseCategoryType, WarehouseCategory>();

            foreach (var warehouse in data.warehouselist)
            {
                if (warehouse == null || !IsSupportedWarehouseType(warehouse.warehouseCategoryType))
                {
                    continue;
                }

                if (!expectedTypes.Contains(warehouse.warehouseCategoryType))
                {
                    continue;
                }

                NormalizeWarehouseCategory(warehouse);
                if (!normalizedWarehouseMap.TryGetValue(warehouse.warehouseCategoryType, out var existingWarehouse))
                {
                    normalizedWarehouseMap[warehouse.warehouseCategoryType] = warehouse;
                    continue;
                }

                if (ReferenceEquals(existingWarehouse, warehouse))
                {
                    NormalizeWarehouseCategory(existingWarehouse);
                    continue;
                }

                existingWarehouse.capacity = Mathf.Max(existingWarehouse.capacity, warehouse.capacity);
                existingWarehouse.maxAtkLevel = Mathf.Max(existingWarehouse.maxAtkLevel, warehouse.maxAtkLevel);
                existingWarehouse.maxNumLevel = Mathf.Max(existingWarehouse.maxNumLevel, warehouse.maxNumLevel);
                existingWarehouse.atkLevel = Mathf.Max(existingWarehouse.atkLevel, warehouse.atkLevel);
                existingWarehouse.numLevel = Mathf.Max(existingWarehouse.numLevel, warehouse.numLevel);
                existingWarehouse.peopleNum = Mathf.Max(existingWarehouse.peopleNum, warehouse.peopleNum);
                existingWarehouse.atk = Mathf.Max(existingWarehouse.atk, warehouse.atk);
                existingWarehouse.workingCollectorList.AddRange(warehouse.workingCollectorList);
                existingWarehouse.unworkingCollectorList.AddRange(warehouse.unworkingCollectorList);
                existingWarehouse.targetTypeList.AddRange(warehouse.targetTypeList);
                existingWarehouse.ownItemList.list.AddRange(warehouse.ownItemList.list);
                NormalizeWarehouseCategory(existingWarehouse);
            }

            foreach (var expectedType in expectedTypes)
            {
                if (normalizedWarehouseMap.ContainsKey(expectedType))
                {
                    continue;
                }

                var warehouse = new WarehouseCategory(expectedType)
                {
                    capacity = defaultWarehouseCapacity,
                    maxAtkLevel = defaultMaxAtkLevel,
                    maxNumLevel = defaultMaxNumLevel,
                    atk = defaultWarehouseAtk
                };
                NormalizeWarehouseCategory(warehouse);
                normalizedWarehouseMap[expectedType] = warehouse;
            }

            data.warehouselist = expectedTypes
                .Where(normalizedWarehouseMap.ContainsKey)
                .Select(type => normalizedWarehouseMap[type])
                .ToList();

            return;

            List<WarehouseCategoryType> GetExpectedWarehouseTypesForCurrentMap()
            {
                List<WarehouseCategoryType> result = new List<WarehouseCategoryType>();
                if (data.structUnLockDataDic == null)
                {
                    return result;
                }

                int mapId = Mathf.Clamp(data.currentMapID, 1, 5);
                if (!data.structUnLockDataDic.TryGetValue(mapId, out var unlockedBuildings) || unlockedBuildings == null)
                {
                    return result;
                }

                if (unlockedBuildings.Contains(BuildingType.LingChuGe_1))
                {
                    result.Add(WarehouseCategoryType.LingChuGe_1);
                }

                if (unlockedBuildings.Contains(BuildingType.LingChuGe_2))
                {
                    result.Add(WarehouseCategoryType.LingChuGe_2);
                }

                return result;
            }

            bool IsSupportedWarehouseType(WarehouseCategoryType warehouseType)
            {
                return warehouseType == WarehouseCategoryType.LingChuGe_1 ||
                       warehouseType == WarehouseCategoryType.LingChuGe_2;
            }

            void NormalizeWarehouseCategory(WarehouseCategory warehouse)
            {
                warehouse.workingCollectorList ??= new List<Collector>();
                warehouse.unworkingCollectorList ??= new List<Collector>();
                warehouse.targetTypeList ??= new List<MonsterFamily>();
                warehouse.ownItemList ??= new SerializableIntDictionary<int>();
                warehouse.ownItemList.list ??= new List<IntKeyValue<int>>();

                warehouse.capacity = defaultWarehouseCapacity;
                warehouse.maxAtkLevel = defaultMaxAtkLevel;
                warehouse.maxNumLevel = defaultMaxNumLevel;
                warehouse.atkLevel = Mathf.Clamp(Mathf.Max(1, warehouse.atkLevel), 1, warehouse.maxAtkLevel);
                warehouse.numLevel = Mathf.Clamp(Mathf.Max(1, warehouse.numLevel), 1, warehouse.maxNumLevel);

                List<Collector> workingCollectors = new List<Collector>();
                HashSet<MonsterFamily> assignedMonsterSet = new HashSet<MonsterFamily>();
                foreach (var collector in warehouse.workingCollectorList)
                {
                    if (collector == null || !Enum.IsDefined(typeof(MonsterFamily), collector.monsterType) || collector.monsterType == MonsterFamily.None)
                    {
                        continue;
                    }

                    if (!assignedMonsterSet.Add(collector.monsterType))
                    {
                        continue;
                    }

                    workingCollectors.Add(collector);
                }

                int normalizedPeopleNum = Mathf.Clamp(
                    Mathf.Max(1, warehouse.peopleNum, warehouse.numLevel, workingCollectors.Count),
                    1,
                    warehouse.maxNumLevel);
                warehouse.numLevel = Mathf.Clamp(Mathf.Max(warehouse.numLevel, normalizedPeopleNum), 1, warehouse.maxNumLevel);
                warehouse.peopleNum = warehouse.numLevel;

                if (workingCollectors.Count > warehouse.peopleNum)
                {
                    workingCollectors = workingCollectors.Take(warehouse.peopleNum).ToList();
                }

                List<Collector> unworkingCollectors = new List<Collector>();
                foreach (var collector in warehouse.unworkingCollectorList)
                {
                    if (collector == null)
                    {
                        continue;
                    }

                    collector.monsterType = MonsterFamily.None;
                    unworkingCollectors.Add(collector);
                }

                int nextCollectorId = 1;
                foreach (var collector in workingCollectors)
                {
                    collector.id = nextCollectorId++;
                }

                int needIdleCollectorCount = warehouse.peopleNum - workingCollectors.Count;
                List<Collector> finalUnworkingCollectors = new List<Collector>();
                for (int i = 0; i < needIdleCollectorCount; i++)
                {
                    Collector collector = i < unworkingCollectors.Count
                        ? unworkingCollectors[i]
                        : new Collector(nextCollectorId, MonsterFamily.None);
                    collector.id = nextCollectorId++;
                    collector.monsterType = MonsterFamily.None;
                    finalUnworkingCollectors.Add(collector);
                }

                warehouse.workingCollectorList = workingCollectors;
                warehouse.unworkingCollectorList = finalUnworkingCollectors;
                warehouse.atk = defaultWarehouseAtk + (warehouse.atkLevel - 1) * warehouseAtkLevelStep;

                HashSet<MonsterFamily> targetTypeSet = new HashSet<MonsterFamily>();
                foreach (var targetType in warehouse.targetTypeList)
                {
                    if (!Enum.IsDefined(typeof(MonsterFamily), targetType) || targetType == MonsterFamily.None)
                    {
                        continue;
                    }

                    targetTypeSet.Add(targetType);
                }

                foreach (var collector in warehouse.workingCollectorList)
                {
                    if (collector != null && collector.monsterType != MonsterFamily.None)
                    {
                        targetTypeSet.Add(collector.monsterType);
                    }
                }

                warehouse.targetTypeList = targetTypeSet
                    .OrderBy(value => (int)value)
                    .ToList();

                Dictionary<int, int> mergedOwnItemMap = new Dictionary<int, int>();
                foreach (var item in warehouse.ownItemList.list)
                {
                    if (item == null || item.value <= 0 || !Enum.IsDefined(typeof(DropItemType), item.key))
                    {
                        continue;
                    }

                    if (!mergedOwnItemMap.TryAdd(item.key, item.value))
                    {
                        mergedOwnItemMap[item.key] += item.value;
                    }
                }

                warehouse.ownItemList.list = mergedOwnItemMap
                    .OrderBy(pair => pair.Key)
                    .Select(pair => new IntKeyValue<int> { key = pair.Key, value = pair.Value })
                    .ToList();
            }
        }

        private PlayerData CreatePlayerDataSnapshot()
        {
            if (data == null)
            {
                return null;
            }

            NormalizeBagCapacityData();

            try
            {
                string json = JsonConvert.SerializeObject(data, Formatting.None);
                var settings = new JsonSerializerSettings
                {
                    ObjectCreationHandling = ObjectCreationHandling.Replace
                };
                return JsonConvert.DeserializeObject<PlayerData>(json, settings);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PlayerDataSave] Failed to create snapshot: {ex.Message}");
                return null;
            }
        }

        private void NormalizeBagCapacityData()
        {
            if (data == null)
            {
                return;
            }

            if (!TryGetBagCapacityById(data.currentBag, out float equippedCapacity))
            {
                if (data.currentBag != 1)
                {
                    return;
                }

                equippedCapacity = 0f;
            }

            if (!data.bagCapacityDataMigrated)
            {
                data.bagCapacityDataMigrated = true;
            }

            RefreshTalentDerivedStats();
            data.equippedBagCapacity = equippedCapacity;
        }

        private bool TryGetBagCapacityById(int bagId, out float capacity)
        {
            capacity = 0f;
            if (DataController.Instance == null || DataController.Instance.storageBagDataDic == null)
            {
                return false;
            }

            if (!DataController.Instance.storageBagDataDic.TryGetValue(bagId, out var bagData) || bagData == null)
            {
                return false;
            }

            capacity = bagData.capacity;
            return true;
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

            SyncEmployeeFunctionStateByYunDiGe();
        }

        public void RefreshStructureUnlockData()
        {
            NormalizeStructureUnlockData();
        }

        private void SyncEmployeeFunctionStateByYunDiGe()
        {
            if (data == null || data.structUnLockDataDic == null)
            {
                return;
            }

            bool shouldUnlock = HasUnlockedYunDiGeInRealUnlockedMaps();
            int targetState = shouldUnlock ? 1 : 0;
            if (data.employeeFunction == targetState)
            {
                return;
            }

            data.employeeFunction = targetState;
            EventCenter.Instance.TriggerEvent(EventMessages.UpdateFunctionState);
        }

        private bool HasUnlockedYunDiGeInRealUnlockedMaps()
        {
            if (data?.structUnLockDataDic == null)
            {
                return false;
            }

            var realUnlockedMaps = data.realUnlockMapList;
            if (realUnlockedMaps == null || realUnlockedMaps.Count == 0)
            {
                realUnlockedMaps = new List<int> { Mathf.Max(1, data.currentMapID) };
            }

            foreach (var mapId in realUnlockedMaps)
            {
                if (data.structUnLockDataDic.TryGetValue(mapId, out var unlockedList) &&
                    unlockedList != null &&
                    unlockedList.Contains(BuildingType.YunDiGe))
                {
                    return true;
                }
            }

            return false;
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
            UIController.Instance.Show<UpLevelView>();
            if (data.accountLevel >= 1)
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

        public void GetTaskReward(int rewardId, bool showTip = true)
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
            if (showTip)
            {
                UIController.Instance.Show<TipView>("领取成功！");
            }
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
            if (args == null || args.Length == 0 || args[0] is not BuildingType buildingType)
            {
                Debug.LogWarning("[HandleUpGradeStuctureTask] Missing or invalid buildingType argument.");
                return;
            }

            foreach (var _data in data.listenInTaskList)
            {
                if (_data.type == TaskType.Upgrade)
                {
                    if ((BuildingType)_data.aimId == buildingType)
                    {
                        RefreshUpgradeTaskProgress(_data);
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

        private static string BuildCloudDataDigest(string text)
        {
            if (text == null)
            {
                return "null";
            }

            if (text.Length == 0)
            {
                return "empty";
            }

            int bytes = Encoding.UTF8.GetByteCount(text);
            int openBraceCount = CountChar(text, '{');
            int closeBraceCount = CountChar(text, '}');
            int quoteCount = CountChar(text, '"');
            return $"chars={text.Length} bytes={bytes} sha256={ComputeSha256Prefix(text)} braces={openBraceCount}/{closeBraceCount} quotes={quoteCount} head=\"{GetSnippet(text, true)}\" tail=\"{GetSnippet(text, false)}\"";
        }

        private static int CountChar(string text, char value)
        {
            int count = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == value)
                {
                    count++;
                }
            }

            return count;
        }

        private static string ComputeSha256Prefix(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            byte[] hash;
            using (SHA256 sha256 = SHA256.Create())
            {
                hash = sha256.ComputeHash(bytes);
            }
            StringBuilder sb = new StringBuilder(16);
            for (int i = 0; i < 8 && i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }

            return sb.ToString();
        }

        private static string GetSnippet(string text, bool fromStart, int maxLength = 80)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            string snippet = fromStart
                ? text.Substring(0, Math.Min(maxLength, text.Length))
                : text.Substring(Math.Max(0, text.Length - maxLength));
            return snippet.Replace("\r", "\\r").Replace("\n", "\\n");
        }
    }

}


