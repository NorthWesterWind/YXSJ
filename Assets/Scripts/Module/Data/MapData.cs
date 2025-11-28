using System.Collections.Generic;

namespace Module.Data
{
	public class MapData
	{
		public int id;                     // 地图编号
		public string name;                // 地图名称
		public int unlockLevel;            // 解锁等级
		public int unlockCost;             // 解锁费用
		public List<int> monsterTypeList;  // 地图中的怪物种类
		public List<int> customerTypeList; // 地图中的顾客类型
		public int taskGroupSize;          // 每张地图任务每组大小
		public int taskNum;				   // 任务数量
		public int taskGroupNum;		   // 任务组数
		
	}
}

