using System.Collections.Generic;

namespace Module.Data
{
    public static class WorldData
    {
        public static Dictionary<int, int> LevelRequirementDic = new(){{1,12} ,{2,36},{3,36} ,{4,50} , {5 , 66}};

        public static int[] cardUpLevelArr = new[] {1, 2, 6, 10, 20, 50, 90, 150, 220, 300}; //升级令每一等级需要的碎片数量 ，从0 级升到1级开始
    }
}
