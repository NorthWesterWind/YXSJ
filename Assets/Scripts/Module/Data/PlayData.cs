using System.Collections.Generic;
using UnityEngine;
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
        public int silverCoin;  //银钱
        public int goldIngot;   //金元宝
        public int lingJing;    //灵晶
        public int jingMangZhu; //金芒珠
        public int lingQiaoShi; //灵窍石
        
  
        public int currentMapID = 1;


        public int accountLevel = 1;
        public int characterFunction = 0;
        public int cardFunction = 0;
        public int mapFunction = 0;
        public int employeeFunction = 0;
        public int ordenFunction = 0;
        public List<int> levelLockMapList = new (){2,3,4,5};
        public List<int> realUnlockMapList = new (){1};
        
        
        [Header("key = 地图编号 ， value = 当前任务进度")]
        public Dictionary< int  , List<int>> mapTaskRecordDic = new(){{1,new List<int>()},{2,new List<int>()},{3,new List<int>()},{4,new List<int>()},{5,new List<int>()}};
        
        
        #endregion
        
    }
}
