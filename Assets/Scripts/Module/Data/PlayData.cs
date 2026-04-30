using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Controller;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Utils;

namespace Module.Data
{

    public enum WarehouseCategoryType
    {
        LingChuGe_1 = 1,  //一号灵储阁
        LingChuGe_2 = 2,  //二号灵储阁
    }

    [System.Serializable]
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
        public float slowDownValue = 1f; //降低速度值
        public float weaponSize = 1f;   //武器尺寸
        public float hpRecover = 0.05f;       //生命值回复


        /// <summary>
        /// 天赋附加属性
        /// </summary>

        public float addHp;
        public float addAtk;
        public float addBagCapacity;
        public float equippedBagCapacity;
        public bool bagCapacityDataMigrated;
        public float addMoveSpeed;
        public float addPickUpRange;
        public float addSlowDownValue;
        public float addweaponSize;
        public float addhpRecover;

        #endregion

        #region 账号信息

        public string playerName = "未定义";
        public string userAccount;
        public string userPassword;
        public int age;
        public int oncePurchaseLimit;
        public int monthPurchaseLimit;
        public int mothTotalSpending;

        public bool isCreated=false;
        public int user_id;
        public int headId;
        #endregion

        #region 游玩数据

        public int currentClothing = 3;
        public List<int> ownClothingList = new List<int>() { 3 };
        public List<int> guidIdList = new List<int>() { };
        public GuideStep guideStep = GuideStep.BuildYushaPot;

        #region  每日限制数据
        public int todayUseZhuanPanNum = 0; //今日转盘使用次数
        public int currentUseNum; //当前累计使用次数

        public int playLingBaoCount;
        public bool canPlayXuanJing = true;
        public string playXuanJingTime;
        public string lastloginday = "";
        public CurrencyType playTrialCurrencyType;
        #endregion

        public int monthlyLimitMoney; //每月限制消费金额
        public DateTime lastTime;  //判断是否跨月
        public int tongbi = 3000;  //铜币
        public int goldIngot = 0;   //金元宝
        public int lingJing = 0;    //灵晶
        public int star = 0; //星星
        public int talentPoint = 0; //翠芒珠
        public int currentMapID = 1;

        public float speedTime = 0; //生产台加速时长

        public int accountLevel = 0;
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
            {3,new List<BuildingType>(){BuildingType.LingZhangTai ,BuildingType.YunDiGe , BuildingType.LingChuGe_1}},
            {4,new List<BuildingType>(){BuildingType.LingZhangTai ,BuildingType.YunDiGe , BuildingType.LingChuGe_1 , BuildingType.LingChuGe_2 }},
            {5,new List<BuildingType>(){BuildingType.LingZhangTai ,BuildingType.YunDiGe, BuildingType.LingChuGe_1 , BuildingType.LingChuGe_2 }}
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

        #region 场景运行时数据（随玩家存档）

        /// <summary>
        /// 场景中的顾客快照（按地图保存）
        /// </summary>
        public List<RuntimeCustomerData> runtimeCustomerDataList = new List<RuntimeCustomerData>();

        /// <summary>
        /// 场景中的商品/硬币快照（按地图保存）
        /// </summary>
        public List<RuntimeProductionData> runtimeProductionDataList = new List<RuntimeProductionData>();

        /// <summary>
        /// 生产台材料数量快照（按地图保存）
        /// </summary>
        public List<RuntimeProductionStationData> runtimeProductionStationDataList = new List<RuntimeProductionStationData>();

        /// <summary>
        /// 玩家背包材料快照
        /// </summary>
        public List<RuntimeDropItemCount> runtimePlayerDropList = new List<RuntimeDropItemCount>();

        /// <summary>
        /// 玩家背包商品快照
        /// </summary>
        public List<RuntimeGoodsCount> runtimePlayerGoodsList = new List<RuntimeGoodsCount>();
        public List<RuntimeYuanBaoMonsterData> runtimeYuanBaoMonsterDataList = new List<RuntimeYuanBaoMonsterData>();
        public List<RuntimeYuanBaoDropData> runtimeYuanBaoDropDataList = new List<RuntimeYuanBaoDropData>();
        public List<RuntimeYuanBaoStateData> runtimeYuanBaoStateDataList = new List<RuntimeYuanBaoStateData>();

        #endregion
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
        public List<YuanBaoKuangDongStateData> yuanBaoKuangDongStateDataList = new List<YuanBaoKuangDongStateData>();



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





    [JsonConverter(typeof(OrderDataProgressJsonConverter))]
    public class OrderDataProgress
    {
        public int orderId;
        public Dictionary<GoodsType, OrderProgressValue> goodDic = new Dictionary<GoodsType, OrderProgressValue>() { { GoodsType.None, new OrderProgressValue(0, 0) } };
        public Dictionary<DropItemType, OrderProgressValue> dropDic = new Dictionary<DropItemType, OrderProgressValue>() { { DropItemType.None, new OrderProgressValue(0, 0) } };


        public OrderDataProgress(int orderId, Dictionary<GoodsType, OrderProgressValue> goodDic, Dictionary<DropItemType, OrderProgressValue> dropDic)
        {
            this.orderId = orderId;
            this.goodDic = goodDic;
            this.dropDic = dropDic;
        }
    }

    public class OrderDataProgressJsonConverter : JsonConverter<OrderDataProgress>
    {
        public override void WriteJson(JsonWriter writer, OrderDataProgress value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(nameof(OrderDataProgress.orderId));
            writer.WriteValue(value?.orderId ?? 0);
            writer.WritePropertyName(nameof(OrderDataProgress.goodDic));
            WriteProgressDictionary(writer, value?.goodDic, serializer);
            writer.WritePropertyName(nameof(OrderDataProgress.dropDic));
            WriteProgressDictionary(writer, value?.dropDic, serializer);
            writer.WriteEndObject();
        }

        public override OrderDataProgress ReadJson(JsonReader reader, Type objectType, OrderDataProgress existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return new OrderDataProgress(0,
                    new Dictionary<GoodsType, OrderProgressValue>(),
                    new Dictionary<DropItemType, OrderProgressValue>());
            }

            JObject obj = JObject.Load(reader);
            int orderId = obj[nameof(OrderDataProgress.orderId)]?.Value<int>() ?? 0;
            var goodDic = ReadProgressDictionary<GoodsType>(obj[nameof(OrderDataProgress.goodDic)], serializer);
            var dropDic = ReadProgressDictionary<DropItemType>(obj[nameof(OrderDataProgress.dropDic)], serializer);
            return new OrderDataProgress(orderId, goodDic, dropDic);
        }

        private static void WriteProgressDictionary<TEnum>(JsonWriter writer, Dictionary<TEnum, OrderProgressValue> dic, JsonSerializer serializer)
            where TEnum : struct, Enum
        {
            writer.WriteStartObject();
            if (dic != null)
            {
                foreach (var kv in dic)
                {
                    writer.WritePropertyName(kv.Key.ToString());
                    serializer.Serialize(writer, kv.Value);
                }
            }
            writer.WriteEndObject();
        }

        private static Dictionary<TEnum, OrderProgressValue> ReadProgressDictionary<TEnum>(JToken token, JsonSerializer serializer)
            where TEnum : struct, Enum
        {
            var result = new Dictionary<TEnum, OrderProgressValue>();
            if (token == null || token.Type == JTokenType.Null)
            {
                return result;
            }

            if (token.Type == JTokenType.Object)
            {
                foreach (var property in ((JObject)token).Properties())
                {
                    if (!TryParseEnumKey(property.Name, out TEnum key))
                    {
                        continue;
                    }

                    result[key] = property.Value?.ToObject<OrderProgressValue>(serializer) ?? new OrderProgressValue();
                }
                return result;
            }

            if (token.Type == JTokenType.Array)
            {
                foreach (var item in (JArray)token)
                {
                    if (!TryReadArrayEntry(item, serializer, out TEnum key, out var value))
                    {
                        continue;
                    }

                    result[key] = value;
                }
            }

            return result;
        }

        private static bool TryReadArrayEntry<TEnum>(JToken item, JsonSerializer serializer, out TEnum key, out OrderProgressValue value)
            where TEnum : struct, Enum
        {
            key = default;
            value = new OrderProgressValue();

            if (item == null || item.Type == JTokenType.Null)
            {
                return false;
            }

            if (item.Type == JTokenType.Object)
            {
                var obj = (JObject)item;
                var keyToken = obj["Key"] ?? obj["key"];
                var valueToken = obj["Value"] ?? obj["value"];

                if (keyToken != null && TryParseEnumKey(keyToken, out key))
                {
                    value = valueToken?.ToObject<OrderProgressValue>(serializer) ?? new OrderProgressValue();
                    return true;
                }

                if (obj.Properties().Count() == 1)
                {
                    var property = obj.Properties().First();
                    if (TryParseEnumKey(property.Name, out key))
                    {
                        value = property.Value?.ToObject<OrderProgressValue>(serializer) ?? new OrderProgressValue();
                        return true;
                    }
                }

                return false;
            }

            if (item.Type == JTokenType.Array)
            {
                var array = (JArray)item;
                if (array.Count < 2 || !TryParseEnumKey(array[0], out key))
                {
                    return false;
                }

                value = array[1]?.ToObject<OrderProgressValue>(serializer) ?? new OrderProgressValue();
                return true;
            }

            return false;
        }

        private static bool TryParseEnumKey<TEnum>(string raw, out TEnum key)
            where TEnum : struct, Enum
        {
            if (Enum.TryParse(raw, true, out key))
            {
                return true;
            }

            if (int.TryParse(raw, out int intValue))
            {
                key = (TEnum)Enum.ToObject(typeof(TEnum), intValue);
                return true;
            }

            key = default;
            return false;
        }

        private static bool TryParseEnumKey<TEnum>(JToken token, out TEnum key)
            where TEnum : struct, Enum
        {
            if (token == null)
            {
                key = default;
                return false;
            }

            if (token.Type == JTokenType.Integer)
            {
                key = (TEnum)Enum.ToObject(typeof(TEnum), token.Value<int>());
                return true;
            }

            return TryParseEnumKey(token.ToString(), out key);
        }
    }

    [Serializable]
    [JsonConverter(typeof(OrderProgressValueJsonConverter))]
    public class OrderProgressValue
    {
        public int current;
        public int target;

        public OrderProgressValue()
        {
        }

        public OrderProgressValue(int current, int target)
        {
            this.current = current;
            this.target = target;
        }
    }

    public class OrderProgressValueJsonConverter : JsonConverter<OrderProgressValue>
    {
        public override void WriteJson(JsonWriter writer, OrderProgressValue value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(nameof(OrderProgressValue.current));
            writer.WriteValue(value?.current ?? 0);
            writer.WritePropertyName(nameof(OrderProgressValue.target));
            writer.WriteValue(value?.target ?? 0);
            writer.WriteEndObject();
        }

        public override OrderProgressValue ReadJson(JsonReader reader, Type objectType, OrderProgressValue existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return new OrderProgressValue();
            }

            if (reader.TokenType == JsonToken.StartArray)
            {
                JArray array = JArray.Load(reader);
                int current = array.Count > 0 ? array[0]?.Value<int>() ?? 0 : 0;
                int target = array.Count > 1 ? array[1]?.Value<int>() ?? 0 : 0;
                return new OrderProgressValue(current, target);
            }

            if (reader.TokenType == JsonToken.StartObject)
            {
                JObject obj = JObject.Load(reader);
                int current = obj["current"]?.Value<int>()
                              ?? obj["Current"]?.Value<int>()
                              ?? obj["Item1"]?.Value<int>()
                              ?? 0;
                int target = obj["target"]?.Value<int>()
                             ?? obj["Target"]?.Value<int>()
                             ?? obj["Item2"]?.Value<int>()
                             ?? 0;
                return new OrderProgressValue(current, target);
            }

            if (reader.TokenType == JsonToken.Integer)
            {
                int value = Convert.ToInt32(reader.Value);
                return new OrderProgressValue(value, value);
            }

            return new OrderProgressValue();
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
        public float bagCapacity = 10;
        public float moveSpeed = 2f;     //移动速度
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
        public int workingNum = 1; //正在工作人数
        public int totalNum = 1;    //总人数
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
        public float atk = 10;
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
    [Serializable]
    public class ProductStationData
    {
        public int mapId;
        public BuildingType buildingType;
        public int priceLevel;
        public int maxPriceLevel = 100;
        public int timelevel;
        public int maxTimeLevel = 40;
        public GoodsType goodsType = GoodsType.None;
        [JsonConstructor]
        public ProductStationData()
        {
            mapId = 0;
            buildingType = BuildingType.None;
            priceLevel = 1;
            timelevel = 1;
            goodsType = GoodsType.None;
        }

        public ProductStationData(BuildingType buildingType, GoodsType goodsType)
            : this(0, buildingType, goodsType)
        {
        }

        public ProductStationData(int mapId, BuildingType buildingType, GoodsType goodsType)
        {
            this.mapId = mapId;
            this.buildingType = buildingType;
            priceLevel = 1;
            timelevel = 1;
            this.goodsType = goodsType;
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

    /// <summary>
    /// 运行时顾客存档数据
    /// </summary>
    [Serializable]
    public class RuntimeCustomerData
    {
        public int mapId;
        public CustomerType customerType;
        public GoodsType goodsType;
        public BuildingType targetBuildingType;
        public int state;
        public int routeIndex = -1;
        public int routePhase;
        public int routeWaypointIndex = -1;
        public float bornPosX;
        public float bornPosY;
        public float bornPosZ;
        public float posX;
        public float posY;
        public float posZ;
    }

    /// <summary>
    /// 运行时玩家背包材料存档数据
    /// </summary>
    [Serializable]
    public class RuntimeDropItemCount
    {
        public DropItemType itemType;
        public int count;
    }

    /// <summary>
    /// 运行时玩家背包商品存档数据
    /// </summary>
    [Serializable]
    public class RuntimeGoodsCount
    {
        public GoodsType goodsType;
        public int count;
    }

    /// <summary>
    /// 运行时商品存档数据（包含收银台硬币）
    /// </summary>
    [Serializable]
    public class RuntimeProductionData
    {
        public int mapId;
        public GoodsType goodsType;
        public int value;
        public BuildingType stationBuildingType;
        public int state;
        [DefaultValue(1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int count = 1;
    }

    /// <summary>
    /// 运行时生产台材料存档数据
    /// </summary>
    [Serializable]
    public class RuntimeProductionStationData
    {
        public int mapId;
        public BuildingType stationBuildingType;
        public int currentMaterialCount;
    }

    [Serializable]
    public class RuntimeYuanBaoMonsterData
    {
        public int mapId;
        [DefaultValue(1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int count = 1;
    }

    [Serializable]
    public class RuntimeYuanBaoDropData
    {
        public int mapId;
        [DefaultValue(1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int count = 1;
    }

    [Serializable]
    public class RuntimeYuanBaoStateData
    {
        public int mapId;
        public int generatedCount;
        public int remainCount;
        public string lastRefreshTime;
    }

    [Serializable]
    public class YuanBaoKuangDongStateData
    {
        public int mapId;
        public int generatedCount;
        public int remainCount = 30;
        public string lastRefreshTime = "";
    }

}
