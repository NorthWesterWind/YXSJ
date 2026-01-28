using System;
using System.Collections.Generic;
using Controller;
using Unity.VisualScripting;
using UnityEngine;
using Utils;

namespace Module.Data
{

    public enum WarehouseCategoryType
    {
        LingChuGe_1 = 1,  //一号灵储阁
        LingChuGe_2 = 2,  //二号灵储阁
    }
    public class PlayerData : BaseData
    {
        #region 基础属性
        public float hp = 30;
        public float atk = 20;
        public float bagCapacity = 20;

        #endregion

        #region 天赋属性
        public float moveSpeed = 5f;   //移动速度
        public float pickUpRange = 4f;  //拾取物品距离
        public float slowDownValue = 3f; //降低速度值
        public float weaponSize = 1f;   //武器尺寸
        public float hpRecover = 0f;       //生命值回复


        /// <summary>
        /// 天赋附加属性
        /// </summary>

        public float addHp;
        public float addAtk;
        public float addBagCapacity;
        public float addMoveSpeed;
        public float addPickUpRange;
        public float addSlowDownValue;
        public float addweaponSize;
        public float addhpRecover;

        #endregion

        #region 账号信息

        public string userName = "未定义";
        public string userAccount;
        public string userPassword;
        public int age;
        public int oncePurchaseLimit;
        public int monthPurchaseLimit;
        public int mothTotalSpending;

        public bool isCreated;
        public int user_id;
        #endregion

        #region 游玩数据

        public List<int> guidIdList = new List<int>() { };
        public GuideStep guideStep = GuideStep.BuildYushaPot;

        #region  每日限制数据
        public int todayUseZhuanPanNum = 0; //今日转盘使用次数
        public int currentUseNum; //当前累计使用次数

        #endregion

        public int monthlyLimitMoney; //每月限制消费金额
        public DateTime lastTime;  //判断是否跨月
        public int tongbi = 3000;  //铜币
        public int goldIngot = 0;   //金元宝
        public int lingJing = 0;    //灵晶
        public int jingMangZhu = 0; //金芒珠
        public int currentMapID = 1;

        public float speedTime = 0; //生产台加速时长

        public int accountLevel = 1;
        public int characterFunction = 0;
        public int cardFunction = 0;
        public int mapFunction = 0;
        public int employeeFunction = 0;
        public int ordenFunction = 0;
        public List<int> levelLockMapList = new() { 2, 3, 4, 5 };
        public List<int> realUnlockMapList = new() { 1 };

        public List<int> mapCompletedTaskRecordList_1 = new List<int>() { };
        public List<int> mapCompletedTaskRecordList_2 = new List<int>() { };
        public List<int> mapCompletedTaskRecordList_3 = new List<int>() { };
        public List<int> mapCompletedTaskRecordList_4 = new List<int>() { };
        public List<int> mapCompletedTaskRecordList_5 = new List<int>() { };

        public int nowTaskId = 0; //当前主界面显示的任务信息
        public List<TaskData> listenInTaskList = new();//监听的任务
        public Dictionary<int, int> taskProgressDic = new Dictionary<int, int>() { { 1000000, 1000000 } }; //任务Id - 任务进度 地图一初始
        public int taskPopCompleted = 0; //任务弹窗中用于记录获取的金芒珠 用于宝箱领取
        public string FanPingBaoXiaoTime;
        public string LingShunLingTime;

        public string GetLingJingTime;
        public int GetLingJingCount = 50;

        public List<int> sevenDayRecordList = new List<int>();
        public string sevenDayRecordTime;
        public int GetSevenDayRewardIndex = 0;
        public int SeventRecentlyWeek;
        public int talentLevel;


        public List<int> ownWeaponList = new List<int>() { 1 };
        public int currentWeapon = 1;
        public List<int> ownBagList = new List<int>() { 1 };
        public int currentBag = 1;

        public int cardLevelMax = 0; //最大升级令等级
        public int useLingJingTotalValue = 0; //累积使用灵晶数
        public int useZhuanPanTotalValue = 0; //累积使用转盘次数

        public List<CardUpProgress> cardUpProgressesList = new List<CardUpProgress>();


        /// <summary>
        /// 每个地图中处于锁定的建筑
        /// </summary>
        public Dictionary<int, List<BuildingType>> structLockDataDic = new Dictionary<int, List<BuildingType>>
        {
            {1, new List<BuildingType>(){}},{2,new List<BuildingType>()},{3,new List<BuildingType>()},{4,new List<BuildingType>()},{5,new List<BuildingType>()}
        };

        /// <summary>
        /// 每个地图中已解锁的建筑
        /// </summary>
        public Dictionary<int, List<BuildingType>> structUnLockDataDic = new Dictionary<int, List<BuildingType>>()
        {
            {1, new List<BuildingType>(){}},
            {2,new List<BuildingType>(){BuildingType.LingZhangTai , BuildingType.LingChaJia_1 , BuildingType.YuShaHu_1 }},
            {3,new List<BuildingType>(){BuildingType.LianQiLu_1 , BuildingType.LingQiJia_1 , BuildingType.LingZhangTai ,BuildingType.YunDiGe , BuildingType.LingChuGe_1}},
            {4,new List<BuildingType>(){BuildingType.LianQiLu_1 , BuildingType.LingQiJia_1 , BuildingType.LingZhangTai ,BuildingType.YunDiGe , BuildingType.LingChuGe_1 , BuildingType.LingChuGe_2 }},
            {5,new List<BuildingType>(){BuildingType.LianQiLu_1 , BuildingType.LingQiJia_1 , BuildingType.LingZhangTai ,BuildingType.YunDiGe, BuildingType.LingChuGe_1 , BuildingType.LingChuGe_2 }}
        };
        /// <summary>
        /// 每个地图中处于可解锁状态的建筑
        /// </summary>
        public Dictionary<int, List<BuildingType>> structCanUnLockDataDic = new Dictionary<int, List<BuildingType>>()
        {
            {1, new List<BuildingType>(){}},{2,new List<BuildingType>()},{3,new List<BuildingType>()},{4,new List<BuildingType>()},{5,new List<BuildingType>()}
        };

        public CashierData cashierData;

        #region 云递者数据
        public DeliverData deliverData;

        #endregion


        #region 玄采徒数据
        public List<WarehouseCategory> warehouselist = new List<WarehouseCategory>();

        #endregion


        public List<ProductStationData> ProductStationDataList = new List<ProductStationData>()
        {

        };
        #endregion      


        #region 订单数据缓存
        public List<OrderDataProgress> orderDataprogressList = new List<OrderDataProgress>()
        {

        };

        #endregion


        #region 地图解锁数据缓存
        /// <summary>
        /// 区域解锁数据
        /// </summary>
        public List<MapLockDataProgress> mapLockDataProgressList = new List<MapLockDataProgress>()
        {

        };


        /// <summary>
        /// 建筑解锁数据
        /// </summary>
        public List<StructureLockProgressData> structureLockProgressDataList = new List<StructureLockProgressData>()
        {

        };
        #endregion


        #region 元宝矿洞数据
        public int remainCount = 30;
        public string lastRefrashTime = "";



        #endregion


    }

    public class StructureLockProgressData
    {
        public BuildingType buildType;
        public int needMoney;

        public int lockId;
        public bool isUnlock;
        public float currentOwnMoney;
        public bool canShowBg;
        public int mapId;
        public StructureLockProgressData(BuildingType type, int money, int lockId, int mapId)
        {
            this.buildType = type;
            this.needMoney = money;
            this.lockId = lockId;
            this.mapId = mapId;
            isUnlock = false;
            currentOwnMoney = 0f;
            canShowBg = false;
        }
    }





    public class MapLockDataProgress
    {
        public MonsterType monsterType;
        public int mapId;
        public int lockId;
        public bool isUnlock;
        public float currentOwnMoney;
        public bool canShowBg;

        public MapLockDataProgress(MonsterType monsterType, int mapId, int lockId, bool isUnlock, float currentOwnMoney, bool canShowBg)
        {
            this.monsterType = monsterType;
            this.mapId = mapId;
            this.lockId = lockId;
            this.isUnlock = isUnlock;
            this.currentOwnMoney = currentOwnMoney;
            this.canShowBg = canShowBg;
        }
    }





    public class OrderDataProgress
    {
        public int orderId;
        public Dictionary<GoodsType, (int, int)> goodDic = new Dictionary<GoodsType, (int, int)>() { { GoodsType.None, (0, 0) } };
        public Dictionary<DropItemType, (int, int)> dropDic = new Dictionary<DropItemType, (int, int)>() { { DropItemType.None, (0, 0) } };


        public OrderDataProgress(int orderId, Dictionary<GoodsType, (int, int)> goodDic, Dictionary<DropItemType, (int, int)> dropDic)
        {
            this.orderId = orderId;
            this.goodDic = goodDic;
            this.dropDic = dropDic;
        }
    }






    [Serializable]

    public class CardUpProgress  //卡片养成数据类
    {
        public CardLevelType levelType;
        public CardDevelopType developType;
        public int level;
        public int id;
        public int currentNum;


        public CardUpProgress(int id, int num)
        {
            this.id = id;
            currentNum = num - 1;
            level = 1;
            foreach (var data in DataController.Instance.cardLevelDataList)
            {
                if (data.id == id)
                {
                    levelType = data.levelType;
                    developType = data.developType;
                }
            }
        }
    }

    [Serializable]
    public class Collector
    {
        public int id;
        public float atk = 10;
        public float bagCapacity = 20;
        public float moveSpeed = 8f;     //移动速度
        public float pickUpRange = 5f;   //拾取物品距离
        public float slowDownValue = 3f; //降低速度值
        public float weaponSize = 1f;    //武器尺寸
        public MonsterFamily monsterType; //当前的目标怪物
        public CollectorType collectorType;
        public float maxHp = 30;
        public float hpRecover;
        public Collector(int id, MonsterFamily monsterType)
        {
            this.id = id;
            this.monsterType = monsterType;
        }
    }

    /// <summary>
    /// 云递阁数据
    /// </summary>
    [Serializable]
    public class DeliverData
    {
        public int workingNum = 0; //正在工作人数
        public int totalNum = 0;    //总人数
        public float currentMoveSpeed = 4f;
        public int capacity = 1;
        public int speedLevel = 1;
        public int peopleLevel = 1;
        public int maxSpeedLevel = 60;
        public int maxpeopleLevel = 3;
        public List<BuildingType> yunDiZheWorkingBuildList = new List<BuildingType>();
    }


    /// <summary>
    /// 灵储阁数据类
    /// </summary>
    [Serializable]
    public class WarehouseCategory
    {
        public WarehouseCategoryType warehouseCategoryType;
        public List<Collector> workingCollectorList = new();
        public int capacity = 60;
        public List<MonsterFamily> targetTypeList = new();
        public List<Collector> unworkingCollectorList = new();
        public float atk = 20;
        public int peopleNum = 1;
        public int atkLevel = 1;
        public int maxAtkLevel = 60;
        public int numLevel = 1;
        public int maxNumLevel = 3;
        public SerializableIntDictionary<int> ownItemList = new SerializableIntDictionary<int>();
        public WarehouseCategory(WarehouseCategoryType type)
        {
            warehouseCategoryType = type;
            unworkingCollectorList.Add(new Collector(1, MonsterFamily.None));

        }

    }


    /// <summary>
    /// 生产台数据类
    /// </summary>
    public class ProductStationData
    {
        public BuildingType buildingType;
        public int priceLevel;
        public int maxPriceLevel = 100;
        public int timelevel;
        public int maxTimeLevel = 40;
        public ProductStationData(BuildingType buildingType)
        {
            this.buildingType = buildingType;
            priceLevel = 1;
            timelevel = 1;
        }
    }

    /// <summary>
    /// 灵账台数据
    /// </summary>
    [Serializable]
    public class CashierData
    {
        public int workingNum; //正在工作人数
        public int totalNum = 1;    //总人数
        public float currentWorkingSpeed = 5f;
        public float earning = 1f;
        public int workspeedLevel = 1;
        public int peopleLevel = 1;
        public int maxworkspeedLevel = 50;
        public int maxpeopleLevel = 3;
        public CashierData()
        {
            
        }
    }

}
