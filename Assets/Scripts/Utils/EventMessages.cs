using System.Linq.Expressions;

namespace Utils
{
    public static class EventMessages
    {

        public const string BeginJugmentRemainTime = "BeginJugmentRemainTime";   ///未成年人可游玩时间倒计时
        public const string CameraBeginShaking = "CameraBeginShaking";       //触发镜头晃动
        public const string MonsterDead = "MonsterDead";                     //触发怪物死亡
        public const string MonsterDead2D = "MonsterDead2D";                     //触发2D怪物死亡
        public const string MonsterBeginCreate = "MonsterBeginCreate";       //通知工厂创建怪物
        public const string CustomerBeginCreate = "CustomerBeginCreate";     //通知顾客数据准备完成
        public const string MapDataPrepared = "MapDataPrepared";             //地图数据准备完成
        public const string MapTaskDataPrepared = "MapTaskDataPrepared";     //地图任务数据准备完成
        public const string TriggerDetection = "TriggerDetection";           //玩家停止移动触发检测判定
        public const string NotifyToFlee = "NotifyToFlee";                   //怪物被攻击通知其它怪物
        public const string FocusView = "FocusView";                         //拉进摄像机高度
        public const string RestoreFocusView = "RestoreFocusView";           //恢复摄像机高度
        public const string PlayerTakeDamage = "PlayerTakeDamage";           //玩家受到伤害
        public const string UpdatePlayerMoneyInfo = "UpdatePlayerMoneyInfo"; //更新玩家货币信息
        public const string ProductionComplete = "ProductionComplete";       //产品生产完成
        public const string CustomerArrived = "CustomerArrived";             //顾客到达收银台
        public const string CustomerArrivedSell = "CustomerArrivedSell";             //顾客到达收银台
        public const string FocusNewPosition = "FocusNewPosition";           //让相机平移查看指定物体

        public const string UpdateInfoItem = "UpdateInfoItem"; //更新InfoItem信息

        public const string ShowPlayerInfoViewCartoon = "ShowPlayerInfoViewCartoon"; //播放PlayerInfoView左侧显示动画
        public const string HidePlayerInfoViewCartoon = "HidePlayerInfoViewCartoon"; //播放PlayerInfoView左侧隐藏动画

        public const string UpdateLevelProgress = "UpdateLevelProgress"; //更新等级进度信息
        public const string UpdateFunctionState = "UpdateFunctionState"; //更新当前功能状态（角色、升级录、、、）

        public const string UpdateTaskMainView = "UpdateTaskMainView"; //更新主界面任务显示

        public const string UpdateTaskInfo = "UpdateTaskInfo"; //更新TaskPop中的任务信息
        public const string HasTaskComplete = "HasTaskComplete"; //有任务结束（指的是领取完奖励的）

        public const string TriggerSearch = "Trigger Search"; //触发任务目标寻找

        public const string UpdateYunDiZheInfo = "UpdateYunDiZheInfo";       //更新云递者信息
        public const string UpdateYunDiGeWorkingState = "UpdateYunDiGeWorkingState";       //更新云递阁工作状态


        public const string UpdateLingChuGeInfo = "UpdateLingChuGeInfo"; //更新灵储阁信息
        public const string LingChuGeBeginWorking = "LingChuGeBeginWorking"; //灵储阁开始派遣储玄采徒
        public const string LingChuGeEndWorking = "LingChuGeEndWorking"; //清除所有玄采徒
        public const string LingChuGeDelivery = "LingChuGeDelivery"; //灵储阁递送物品
        public const string LingChuGeStopDelivery = "LingChuGeStopDelivery"; //灵储阁停止递送物品

        public const string UpdateLingChuGeWorkingInfo = "UpdateLingChuGeWorkingInfo"; //更新灵储阁工作信息


        public const string StructureSpeedUp = "StructureSpeedUp"; //建筑速度加快
        public const string StructureSpeedDown = "StructureSpeedDown"; //建筑速度恢复默认        

        public const string UpdatePlayerEquimentInfo = "UpdatePlayerEquipmentInfo"; //更新玩家装备信息
        public const string UpdatePlayerCarryInfo = "UpdatePlayerCarryInfo"; //更新玩家携带信息

        public const string CustomerLeave = "CustomerLeave"; //顾客离开售卖摊


        public const string ThrowOutTongBi = "ThrowOutTongBi"; //玩家抛出铜币

        public const string MapLockUnlocked = "MapLockUnlocked"; //地图区域锁解锁
        public const string UpdateMapLockState = "UpdateMapLockState"; //更新地图锁定区域状态
        public const string StructureLockUnlocked = "StructureLockUnlocked"; //建筑区域锁解锁
        public const string JingYuanBaoDead = "JingYuanBaoDead"; //金元宝死亡
        public const string ShowOrderDetail = "ShowOrderDetail"; //显示订单详情
        public const string UpdateOrderItem = " UpdateOrderItem"; //更新订单界面

        public const string HidePlayerGuide = "HidePlayerGuide"; //玩家引导完成
        public const string DataPrepared = "DataPrepared"; //数据准备完成
        public const string UpdateSturctureLockInfo = "UpdateSturctureLockInfo"; //更新建筑解锁信息
        public const string ProduceTask = "ProduceTask"; //生产任务
        public const string UpGradeStuctureTask = "UpGradeStuctureTask"; //升级建筑任务
        public const string ConstructTask = "ConstructTask"; //建造任务
        public const string SellTask = "SellTask"; //出售商品任务
        public const string HarvestTask = "HarvestTask"; //收集物品任务
        public const string MakeTongBiTask = "MakeTongBiTask"; //获得铜币任务
        public const string UnLockMapTask = "UnLockMapTask"; //解锁怪物区域任务

        public const string UpdateCardInfo = "UpdateCardInfo"; //更新卡牌信息

        public const string ShowLoadView = "ShowLoadView"; //显示加载UI
        public const string UpdateLoadView = "UpdateLoadView"; //更新加载UI
        public const string UpdatePlayerInfo = "UpdatePlayerInfo"; //更新玩家信息
        public const string UpdatePlayerValueInfo = "UpdatePlayerValueInfo"; //更新玩家数值信息

        public const string HasMonsterArrive = "HasMonsterArrive"; //有怪物到达终点

        public const string BeginCreat2DMonster = "BeginCreat2DMonster"; //开始创建2D怪物
        public const string StopCreat2DMonster = "StopCreat2DMonster"; //停止创建2D怪物

        public const string CloseTrialView = "CloseTrialView"; //关闭试炼界面
        public const string UpdateLingZhangTai = "UpdateLingZhangTai"; //更新灵账台
        public const string UpdateYunDiZheSpeed = "UpdateYunDiZheSpeed"; //更新云递者速度
        public const string UpdateSpeedTime = "UpdateSpeedTime"; //更新加速倒计时
        public const string ShowGuideFinger = "ShowGuideFinger";//显示引导手指
        public const string HideGuideFinger = "HideGuideFinger";//隐藏引导手指
    }
}
