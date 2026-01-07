using System;
using System.Collections.Generic;
using Controller;
using UnityEngine;
using Utils;

namespace Module.Data
{
    public class PlayerData : BaseData
    {
        #region 基础属性
        public float hp = 30;
        public float atk = 10;
        public float bagCapacity = 20;
        
        #endregion

        #region 天赋属性
        public float moveSpeed = 5f;   //移动速度
        public float pickUpRange = 5f;  //拾取物品距离
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

        public string userName;
        public string userAccount;
        public string userPassword;
        public string age;
        public int oncePurchaseLimit;
        public int monthPurchaseLimit;
        public int mothTotalSpending;

        #endregion

        #region 游玩数据

        #region  每日限制数据
        public int todayUseZhuanPanNum = 0; //今日转盘使用次数
        #endregion


        public List<int> unlockMapList = new List<int>(){1};
        public Dictionary<int ,List<int>> mapPlayRecordDic = new Dictionary<int ,List<int>>();
        public int tongbi;  //铜币
        public int goldIngot;   //金元宝
        public int lingJing;    //灵晶
        public int jingMangZhu; //金芒珠
        public int lingQiaoShi; //灵窍石
        public int currentMapID = 3;

        
        public int accountLevel = 1;
        public int characterFunction = 1;
        public int cardFunction = 1;
        public int mapFunction = 1;
        public int employeeFunction = 1;
        public int ordenFunction = 0;
        public List<int> levelLockMapList = new (){2,3,4,5};
        public List<int> realUnlockMapList = new (){1};
        
        [Header("key = 地图编号 ， value = 当前任务进度")]
        public Dictionary< int  , List<int>> mapTaskRecordDic = new(){{1,new List<int>()},{2,new List<int>()},{3,new List<int>()},{4,new List<int>()},{5,new List<int>()}};
        public int nowTaskId = 1; //当前主界面显示的任务信息
        public List<TaskData> listenInTaskList = new ();//监听的任务
        public Dictionary<int  , int> taskProgressDic = new (); //进行中任务
        public List<int> completedTaskIdList = new List<int>(); //已完成任务
        
        public string FanPingBaoXiaoTime;
        public string LingShunLingTime;
        public string PurchaseVipTime;
        
        public List<int> sevenDayRecordList = new List<int>();
        public string sevenDayRecordTime;
        public int GetSevenDayRewardIndex = 0   ;

        public int talentLevel;
        
        
        public List<int> ownWeaponList = new List<int>(){1};
        public int currentWeapon = 1;
        public List<int> ownBagList = new List<int>(){1};
        public int currentBag = 1;

        public int cardLevelMax = 0; //最大升级令等级
        public int useLingJingTotalValue = 0; //累积使用灵晶数
        public int useZhuanPanTotalValue = 0; //累积使用转盘次数

        public int useZhuanPanTodayValue = 0; //今日使用转盘次数
        
        public List<CardUpProgress>  cardUpProgressesList = new List<CardUpProgress>();


        #region 云递者数据

        public int workingNum; //正在工作人数
        public int totalNum = 1;    //总人数
        public int currentMoveSpeed;
        public int capacity = 1;
        #endregion
        

        #region 玄采徒数据
        public  List<WarehouseCategory> warehouselist = new List<WarehouseCategory>();
        
        #endregion

        
        public List<ProductStationData> ProductStationDataList = new List<ProductStationData>()
        {
            
        };
        #endregion      


        #region 订单数据缓存
        public List<OrderDataProgress> orderDataprogressList = new List<OrderDataProgress>(){
           
        };

        #endregion


        #region 地图解锁数据缓存
        /// <summary>
        /// 区域解锁数据
        /// </summary>
        public List<MapLockDataProgress> mapLockDataProgressList = new List<MapLockDataProgress>(){
           
        };


        /// <summary>
        /// 建筑解锁数据
        /// </summary>
        public List<StructureLockProgressData> structureLockDataList = new List<StructureLockProgressData>(){
           
        };
        #endregion


        #region 元宝矿洞数据
        public int remainCount = 0;
        public string lastRefrashTime;


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
        public StructureLockProgressData(BuildingType type, int money , int lockId, int mapId)
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
        public int mapId;
        public int lockId;
        public bool isUnlock;
        public float currentOwnMoney;
        public bool canShowBg;

        public MapLockDataProgress(int mapId, int lockId, bool isUnlock, float currentOwnMoney , bool canShowBg)
        {
            this.mapId = mapId;
            this.lockId = lockId;
            this.isUnlock = isUnlock;
            this.currentOwnMoney = currentOwnMoney;
            this.canShowBg = canShowBg;
        }
    }





    public class  OrderDataProgress
    {
        public int orderId;
        public Dictionary< GoodsType , int> needDic = new Dictionary< GoodsType , int> ();
        public Dictionary<MonsterType, int> monsterDic = new Dictionary<MonsterType, int>();

        public OrderDataProgress(int orderId , Dictionary< GoodsType , int> needDic, Dictionary<MonsterType, int> monsterDic)
        {
            this.orderId = orderId;
            this.needDic = needDic;
            this.monsterDic = monsterDic;
        }
    }






    [Serializable]
    
    public  class CardUpProgress  //卡片养成数据类
    {
        public CardLevelType levelType;
        public CardDevelopType developType;
        public int level;
        public int id;
        public int currentNum;
        
        
        public CardUpProgress(int id , int num)
        {
            this.id = id;
            currentNum = num-1;
            level = 1;
            foreach (var data in DataController.Instance.cardLevelDataList)
            {
                if (data.id == id)
                {
                    levelType =  data.levelType;
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
        public Collector(int id  ,  MonsterFamily monsterType)
        {
            this.id = id;
            this.monsterType = monsterType;
        }
    }
    
    /// <summary>
    /// 灵储阁数据类
    /// </summary>
    [Serializable]
    public class WarehouseCategory
    {
        public int id;
        public List<Collector> workingCollectorList = new ();
        public int capacity ;
        public List<MonsterType> targetTypeList = new ();
        public List<Collector> unworkingCollectorList = new();
        public SerializableIntDictionary<int> ownItemList = new SerializableIntDictionary<int>();
        public WarehouseCategory(int id)
        {
            this.id = id;
            capacity = 60;
            unworkingCollectorList.Add(new Collector(1,MonsterFamily.None));
            
        }
        
        
    }


    /// <summary>
    /// 生产台数据类
    /// </summary>
    public class ProductStationData
    {
        public BuildingType buildingType;
        public int price;
        public int priceLevel;

        public float workingtime;
        public int timelevel;
    }
    
}
