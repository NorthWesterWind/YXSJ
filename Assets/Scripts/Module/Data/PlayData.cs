using System.Collections.Generic;
using Utils;

namespace Module.Data
{
    public class PlayerData : BaseData
    {
        #region 基础属性
        public int hp = 30;
        public int atk = 10;
        public int bagCapacity = 20;
        #endregion

        #region 天赋属性
        public float moveSpeed = 8f;   //移动速度
        public float pickUpRange = 5f;  //拾取物品距离
        public float slowDownValue = 3f; //降低速度值
        public float weaponSize = 1f;   //武器尺寸
        public float hpRecover = 8f;       //生命值回复

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

        public Dictionary<int ,List<int>> mapPlayRecordDic = new Dictionary<int ,List<int>>();
        public int silverCoin;
        public int goldIngot;

        public int recentMapID = 1;
        public int currentMapID = 1;


        public int accountLevel = 1;

        #endregion

    }
}
