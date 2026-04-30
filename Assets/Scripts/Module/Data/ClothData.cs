
using Module.Data;

namespace Module.Data
{
	public class ClothData
	{
		public int id; // 标识ID
		public string name; // 服饰名称
		public int hpValue; // 增加的生命值上限
		public string unlockStr; // 解锁条件
		public UnlockType lockType; // 解锁类型
		public int value; // 条件值
		public string attachmentName; // 插槽图片字段
		public string slotName; // 插槽字段
	}
}

