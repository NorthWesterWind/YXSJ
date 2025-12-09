//
// Auto Generated Code By excel2json
// https://neil3d.gitee.io/coding/excel2json.html
// 1. 每个 Sheet 形成一个 Struct 定义, Sheet 的名称作为 Struct 的名称
// 2. 表格约定：第一行是变量名称，第二行是变量类型

// Generate From D:\DAGame\MyFruit\Assets\Excel\MapData.xlsx.xlsx

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
		public int taskNum;                // 任务数量
		public int taskGroupNum;           // 任务组数
		public int  price;                 // 价格倍率
		public List<int> buildTypeList;    // 地图中的新设施
	}
}


// End of Auto Generated Code
