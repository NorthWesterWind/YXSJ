using System.Collections.Generic;

namespace Module.Data
{
    public static class WorldData
    {
        public static Dictionary<int, int> taskboxNeedDic = new(){{1,12} ,{2,36},{3,36} ,{4,50} , {5 , 66}};
        public static Dictionary<int, int> LevelRequirementDic = new(){{1,12} ,{2,36},{3,36} ,{4,50} , {5 , 66}};

        public static int[] cardUpLevelArr = new[] { 2, 6, 10, 20, 50, 90, 150, 220, 300}; //升级令每一等级需要的碎片数量 ，从1 级升到2级开始
        public static int[] cardUpgradeCostArr2 = new[] { 100, 200, 400, 700, 1100, 1600, 2200, 3000, 4000 };
        public static int[] cardUpgradeCostArr1 = new[] { 100, 200, 200, 300, 300, 400, 400, 500, 600};
        public static int[] cardUpgradeCostArr3 = new[] { 1000, 2000, 3000, 4000, 5000, 6000, 7000, 8000, 10000};

        /// <summary>
        /// 商品基础价格数据
        /// </summary>
        public static Dictionary<GoodsType, int> goodsPriceDic = new Dictionary<GoodsType, int>()
        {
            { GoodsType.YunZhiCha  , 100},
            { GoodsType.YueLuCha  , 150},
            { GoodsType.ZiXinCha  , 200},
            { GoodsType.YuHeCha  , 250},
            { GoodsType.XingWenCha  , 300},
            { GoodsType.WuRongCha  , 350},
            { GoodsType.LingXuCha  , 350},
            { GoodsType.XueBanCha  , 400},
            { GoodsType.MuLingCha  , 450},
            { GoodsType.JingRuiCha  , 500},
        };
        
        /// <summary>
        /// 生产台每一等级生产商品需要的时间
        /// </summary>
        public static Dictionary<int  , float> productStationWorkingTimeDic = new()
        {
            {1,7f},
            {2,5.5f},
            {3,4f},
            {4,3.5f},
            {5,3f},
            {6,2.6f},
            {7,2.3f},
            {8,2.1f},
            {9,1.9f},
            {10,1.7f},
           
        };
    }
}
