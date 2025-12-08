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
        public float moveSpeed = 8f;   //移动速度
        public float pickUpRange = 5f;  //拾取物品距离
        public float slowDownValue = 3f; //降低速度值
        public float weaponSize = 1f;   //武器尺寸
        public float hpRecover = 8f;       //生命值回复

        
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

        public Dictionary<int ,List<int>> mapPlayRecordDic = new Dictionary<int ,List<int>>();
        public int silverCoin;  //银钱
        public int goldIngot;   //金元宝
        public int lingJing;    //灵晶
        public int jingMangZhu; //金芒珠
        public int lingQiaoShi; //灵窍石
        public int currentMapID = 1;

        
        public int accountLevel = 1;
        public int characterFunction = 1;
        public int cardFunction = 1;
        public int mapFunction = 0;
        public int employeeFunction = 0;
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
        
        
        #endregion

    }
    
    [Serializable]
    public  class CardUpProgress
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
}
