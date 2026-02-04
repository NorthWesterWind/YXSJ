using System.Collections.Generic;

namespace Module.Data
{
    public static class WorldData
    {
        public static Dictionary<int, int> taskboxNeedDic = new() { { 1, 12 }, { 2, 36 }, { 3, 36 }, { 4, 50 }, { 5, 66 } };
        public static Dictionary<int, int> LevelRequirementDic = new() { { 1, 12 }, { 2, 36 }, { 3, 36 }, { 4, 50 }, { 5, 66 } };

        public static int[] cardUpLevelArr = new[] { 2, 6, 10, 20, 50, 90, 150, 220, 300, 400, 550, 750, 1000, 1300, 1650, 2050, 2500, 3050, 3650 }; //生产台每一等级需要的碎片数量 ，从1 级升到2级开始
        public static int[] cardUpLevelArr_LingChouLing = new[] { 20, 60, 120 };
        public static int[] cardUpLevelArr_LingChuGe_YunDiGe = new[] { 20, 60, 120, 200, 300 };
        public static int[] cardUpLevelArr_WuQiLing_LingLiLingr_YuanBaoLing = new[] { 20, 60, 120, 200, 300, 420, 560, 740, 1000 }; //武器令——灵力令--元宝令  每一等级需要的碎片数量 


        public static int[] cardUpgradeCostArr_FanPin = new[] { 100, 200, 200, 300, 300,
        400, 400, 500, 600,800,
        1000,1500,2000,2800,3600,
        5000 , 7000  , 9000 , 10000};
        public static int[] cardUpgradeCostArr_LingYun = new[] { 100, 200, 400, 700, 1100,
        1600, 2200, 3000, 4000 ,5000,
         6000, 8000, 10000,15000,20000,
         25000,30000 , 40000 , 50000};
        public static int[] cardUpgradeCostArr_XianYun = new[] { 1000, 2000, 3000, 4000, 5000,
        6000, 7000, 8000, 10000 };

        /// <summary>
        /// 商品基础价格数据
        /// </summary>
        public static Dictionary<GoodsType, int> goodsPriceDic = new Dictionary<GoodsType, int>()
        {
            { GoodsType.YunZhiCha  , 100},
            { GoodsType.YueLuCha  , 200},
            { GoodsType.ZiXinCha  , 200},
            { GoodsType.YuHuiCha  , 300},
            { GoodsType.XingWenCha  , 300},
            { GoodsType.WuRongCha  , 300},
            { GoodsType.LingXuCha  , 400},
            { GoodsType.XueBanCha  , 400},
            { GoodsType.MuLingCha  , 500},
            { GoodsType.JingRuiCha  , 500},
            { GoodsType.QingYanJian  , 500},
            { GoodsType.YinSiDao  , 600},
            { GoodsType.TongWenDao  , 600},
            { GoodsType.ZiWuJian  , 600},
            { GoodsType.YueXinJing  , 700},
        };

        /// <summary>
        /// 生产台每一等级生产商品需要的时间
        /// </summary>
        public static Dictionary<int, float> productStationWorkingTimeDic = new()
{
    {1, 8f},
    {2, 7.2f},
    {3, 6.5f},
    {4, 5.9f},
    {5, 5.4f},
    {6, 4.9f},
    {7, 4.5f},
    {8, 4.1f},
    {9, 3.8f},
    {10, 3.5f},

    {11, 3.3f},
    {12, 3.1f},
    {13, 2.9f},
    {14, 2.7f},
    {15, 2.6f},
    {16, 2.5f},
    {17, 2.4f},
    {18, 2.3f},
    {19, 2.2f},
    {20, 2.1f},

    {21, 2f},
    {22, 1.95f},
    {23, 1.9f},
    {24, 1.85f},
    {25, 1.8f},

    {26, 1.75f},
    {27, 1.7f},
    {28, 1.65f},
    {29, 1.6f},
    {30, 1.55f},

    {31, 1.5f},
    {32, 1.45f},
    {33, 1.4f},
    {34, 1.35f},
    {35, 1.3f},

    {36, 1.25f},
    {37, 1.2f},
    {38, 1.15f},
    {39, 1.1f},
    {40, 1f},
};


       public static Dictionary<int, float> speedLevelDic = new()
{
    {1, 3.00f},
    {2, 3.05f},
    {3, 3.10f},
    {4, 3.15f},
    {5, 3.20f},
    {6, 3.25f},
    {7, 3.31f},
    {8, 3.36f},
    {9, 3.41f},
    {10, 3.46f},

    {11, 3.51f},
    {12, 3.56f},
    {13, 3.61f},
    {14, 3.66f},
    {15, 3.71f},
    {16, 3.76f},
    {17, 3.81f},
    {18, 3.86f},
    {19, 3.92f},
    {20, 3.97f},

    {21, 4.02f},
    {22, 4.07f},
    {23, 4.12f},
    {24, 4.17f},
    {25, 4.22f},
    {26, 4.27f},
    {27, 4.32f},
    {28, 4.37f},
    {29, 4.42f},
    {30, 4.47f},

    {31, 4.53f},
    {32, 4.58f},
    {33, 4.63f},
    {34, 4.68f},
    {35, 4.73f},
    {36, 4.78f},
    {37, 4.83f},
    {38, 4.88f},
    {39, 4.93f},
    {40, 4.98f},

    {41, 5.03f},
    {42, 5.08f},
    {43, 5.14f},
    {44, 5.19f},
    {45, 5.24f},
    {46, 5.29f},
    {47, 5.34f},
    {48, 5.39f},
    {49, 5.44f},
    {50, 5.49f},

    {51, 5.54f},
    {52, 5.59f},
    {53, 5.64f},
    {54, 5.69f},
    {55, 5.75f},
    {56, 5.80f},
    {57, 5.85f},
    {58, 5.90f},
    {59, 5.95f},
    {60, 6.00f},
};

    }
}
