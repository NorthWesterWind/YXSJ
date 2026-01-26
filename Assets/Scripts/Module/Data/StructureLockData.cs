using Module.Data;

[System.Serializable]
public class StructureLockData
{
	public int  lockId; // 编号
	public BuildingType buildingType; // 锁定的建筑类型
	public int needMoney; // 解锁需要的铜钱
}

