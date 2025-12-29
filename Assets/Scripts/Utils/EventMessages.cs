namespace Utils
{
    public static class EventMessages
    {
        public const string CameraBeginShaking = "CameraBeginShaking";       //触发镜头晃动
        public const string MonsterDead = "MonsterDead";                     //触发怪物死亡
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
        public const string FocusNewPosition = "FocusNewPosition";           //让相机平移查看指定物体

        public const string UpdateInfoItem = "UpdateInfoItem"; //更新InfoItem信息

        public const string ShowPlayerInfoViewCartoon = "ShowPlayerInfoViewCartoon"; //播放PlayerInfoView左侧显示动画
        public const string HidePlayerInfoViewCartoon = "HidePlayerInfoViewCartoon"; //播放PlayerInfoView左侧隐藏动画

        public const string UpdateLevelProgress = "UpdateLevelProgress"; //更新等级进度信息
        public const string UpdateFunctionState = "UpdateFunctionState"; //更新当前功能状态（角色、升级录、、、）

        public const string UpdateTaskMainView = "UpdateTaskMainView"; //更新主界面任务显示

        public const string UpdateTaskInfo = "UpdateTaskInfo"; //更新TaskPop中的任务信息

        public const string TriggerSearch = "Trigger Search"; //触发任务目标寻找

        public const string AddYunDiZhe = "AddYunDiZhe";       //增加工作中的云递者
        public const string RemoveYunDiZhe = "RemoveYunDiZhe"; //减少工作中的云递者

        public const string UpdateLingChuGeInfo = "UpdateLingChuGeInfo"; //更新灵储阁信息
        public const string LingChuGeBeginWorking = "LingChuGeBeginWorking"; //灵储阁开始派遣储玄采徒
        public const string LingChuGeEndWorking = "LingChuGeEndWorking"; //清除所有玄采徒
        public const string LingChuGeDelivery = "LingChuGeDelivery"; //灵储阁递送物品
        public const string LingChuGeStopDelivery = "LingChuGeStopDelivery"; //灵储阁停止递送物品


        public const string StructureSpeedUp = "StructureSpeedUp"; //建筑速度加快
        public const string StructureSpeedDown = "StructureSpeedDown"; //建筑速度恢复默认        

        public const string UpdatePlayerEquimentInfo = "UpdatePlayerEquipmentInfo"; //更新玩家装备信息

        public const string CustomerLeave = "CustomerLeave"; //顾客离开售卖摊
    }
}