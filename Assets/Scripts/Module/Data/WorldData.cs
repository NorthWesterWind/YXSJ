using System.Collections.Generic;

namespace Module.Data
{
    public static class WorldData
    {
        public static Dictionary<int, int> LevelRequirementDic = new(){{1,12} ,{2,36},{3,36} ,{4,50} , {5 , 66}};

        public static int[] cardUpLevelArr = new[] {1, 2, 6, 10, 20, 50, 90, 150, 220, 300}; //升级令每一等级需要的碎片数量 ，从0 级升到1级开始
        public static int[] cardUpgradeCostArr2 = new[] { 100, 200, 400, 700, 1100, 1600, 2200, 3000, 4000 };
        public static int[] cardUpgradeCostArr1 = new[] { 100, 200, 200, 300, 300, 400, 400, 500, 600};
        public static int[] cardUpgradeCostArr3 = new[] { 1000, 2000, 3000, 4000, 5000, 6000, 7000, 8000, 10000};
    }
}
