namespace Utils
{
    public static class EventMessages
    {
        public const string CameraBeginShaking = "CameraBeginShaking";       //触发镜头晃动
        public const string MonsterDead = "MonsterDead";                     //触发怪物死亡
        public const string MonsterBeginCreate = "MonsterBeginCreate";       //通知工厂创建怪物
        public const string CustomerBeginCreate = "CustomerBeginCreate";     //通知顾客数据准备完成
        public const string TriggerDetection = "TriggerDetection";           //玩家停止移动触发检测判定
        public const string NotifyToFlee = "NotifyToFlee";                   //怪物被攻击通知其它怪物
        public const string FocusView = "FocusView";                         //拉进摄像机高度
        public const string RestoreFocusView = "RestoreFocusView";           //恢复摄像机高度
        public const string PlayerTakeDamage = "PlayerTakeDamage";           //玩家受到伤害
        public const string UpdatePlayerMoneyInfo = "UpdatePlayerMoneyInfo"; //更新玩家货币信息
        public const string ProductionComplete = "ProductionComplete";       //产品生产完成
    }
}