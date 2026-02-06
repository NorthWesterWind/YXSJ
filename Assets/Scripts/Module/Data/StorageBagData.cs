
using Module.Data;
[System.Serializable]

public class StotageBagData
{
	public int id; // 标识ID
	public string name; // 储物袋名称
	public int capacity; // 储物袋增加的容量
	public string unlockStr; // 解锁条件
	public UnlockType lockType; // 解锁类型
	public int value; // 条件值
	public string attachmentName; // 插槽图片字段
	public string slotName; // 插槽字段
}

