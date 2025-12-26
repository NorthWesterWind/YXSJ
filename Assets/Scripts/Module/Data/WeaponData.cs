//
// Auto Generated Code By excel2json
// https://neil3d.gitee.io/coding/excel2json.html
// 1. 每个 Sheet 形成一个 Struct 定义, Sheet 的名称作为 Struct 的名称
// 2. 表格约定：第一行是变量名称，第二行是变量类型

// Generate From F:\WorkSpace\YXSJ\Assets\Excel\Weapon.xlsx.xlsx

using Module.Data;

public class WeaponData
{
	public int id; // 标识ID
	public string name; // 武器名称
	public int atkValue; // 增加的攻击力数值
	public string unlockStr; // 解锁条件
	public UnlockType lockType; // 解锁类型
	public int value; // 条件值
	public string attachmentName; // 插槽图片字段
	public string slotName; // 插槽字段
}


// End of Auto Generated Code
