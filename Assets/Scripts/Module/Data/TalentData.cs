
using System;

namespace Module.Data
{
	[Serializable]
	public class TalentData
	{
		public int id;          // 编号
		public string resName;  // 资源名称
		public TalentType type; // 天赋类型
		public float value;     // 数值
		public string info;     // 信息描述
	}
}

