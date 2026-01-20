using Controller;
using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using View.EmployeeFunction;

public enum EmployeeType
{
    YunDiZhe,
    XuanCaiTu
}
public class EmployeeContent : MonoBehaviour
{

    public TextMeshProUGUI peopleCountTxt;
    public AssetHandle assetHandle;
    public Transform content;
    public EmployeeType employeeType;
    public BuildingType buildingType;

    public UIButton addbtn;
    public Image addMask;
    public TextMeshProUGUI progressTxt;
    public UIButton removebtn;
    public Image removeMask;




    public void Init(EmployeeType employeeType, BuildingType buildingType = BuildingType.None)
    {
        this.employeeType = employeeType;
        this.buildingType = buildingType;
        UpdatePeopleInfo();
        Extensions.ClearChildren(content);
        PlayerData playerData = ModuleMgr.Instance.GetModule<PlayerDataModule>().data;
        if (employeeType == EmployeeType.YunDiZhe)
        {
            for (int i = 0; i < GameController.Instance.salesStallList.Count; i++)
            {
                GameObject obj = Instantiate(assetHandle.Get<GameObject>("EmployeeInfoItem"), content, false);
                obj.GetComponent<EmployeeInfoItem>().Init(GameController.Instance.salesStallList[i].buildingType, 1);
            }
        }
        else
        {
            if (buildingType == BuildingType.LingChuGe_1)
            {
                MapData mapData = DataController.Instance.mapDataDic[playerData.currentMapID];
                for (int i = 0; i < mapData.monsterFamilyList.Count; i++)
                {
                    GameObject obj = Instantiate(assetHandle.Get<GameObject>("EmployeeInfoItem"), content, false);

                    obj.GetComponent<EmployeeInfoItem>().Init((MonsterFamily)mapData.monsterFamilyList[i], 2);
                }
            }
            else
            {
                MapData mapData = DataController.Instance.mapDataDic[playerData.currentMapID];
                for (int i = 0; i < mapData.monsterFamilyList.Count; i++)
                {
                    GameObject obj = Instantiate(assetHandle.Get<GameObject>("EmployeeInfoItem"), content, false);

                    obj.GetComponent<EmployeeInfoItem>().Init((MonsterFamily)mapData.monsterFamilyList[i], 3);
                }
            }
        }


        if (employeeType == EmployeeType.YunDiZhe)
        {
            progressTxt.text = playerData.deliverData.workingNum + "/" + playerData.deliverData.totalNum;
            if (playerData.deliverData.totalNum > playerData.deliverData.workingNum)
            {
                addMask.gameObject.SetActive(false);
            }
            else
            {
                addMask.gameObject.SetActive(true);
            }
            if (playerData.deliverData.workingNum > 0)
            {
                removeMask.gameObject.SetActive(false);
            }
            else
            {
                removeMask.gameObject.SetActive(true);
            }
            addbtn.onClick.RemoveAllListeners();
            addbtn.onClick.AddListener(() =>
            {

                playerData.deliverData.workingNum += 1;
                EventCenter.Instance.TriggerEvent(EventMessages.UpdateYunDiGeWorkingState);

                if (playerData.deliverData.totalNum > playerData.deliverData.workingNum)
                {
                    addMask.gameObject.SetActive(false);
                }
                else
                {
                    addMask.gameObject.SetActive(true);
                }
            });
            removebtn.onClick.RemoveAllListeners();
            removebtn.onClick.AddListener(() =>
            {
                playerData.deliverData.workingNum -= 1;
                EventCenter.Instance.TriggerEvent(EventMessages.UpdateYunDiGeWorkingState);
                if (playerData.deliverData.workingNum > 0)
                {
                    removeMask.gameObject.SetActive(false);
                }
                else
                {
                    removeMask.gameObject.SetActive(true);
                }
            });
        }
    }

    public void UpdatePeopleInfo(params object[] args)
    {
        if (employeeType == EmployeeType.YunDiZhe)
        {
            peopleCountTxt.text = "空闲人数: " + (ModuleMgr.Instance.GetModule<PlayerDataModule>().data.deliverData.totalNum - ModuleMgr.Instance.GetModule<PlayerDataModule>().data.deliverData.workingNum) + "/" + ModuleMgr.Instance.GetModule<PlayerDataModule>().data.deliverData.totalNum;
        }
        else
        {
            switch (buildingType)
            {
                case BuildingType.LingChuGe_1:
                    WarehouseCategory data1 = ModuleMgr.Instance.GetModule<PlayerDataModule>().data.warehouselist.Find(x => x.warehouseCategoryType == WarehouseCategoryType.LingChuGe_1);
                    peopleCountTxt.text = "空闲人数: " + data1.unworkingCollectorList.Count + "/" + (data1.workingCollectorList.Count + data1.unworkingCollectorList.Count);
                    break;
                case BuildingType.LingChuGe_2:
                    WarehouseCategory data2 = ModuleMgr.Instance.GetModule<PlayerDataModule>().data.warehouselist.Find(x => x.warehouseCategoryType == WarehouseCategoryType.LingChuGe_2);
                    peopleCountTxt.text = "空闲人数: " + data2.unworkingCollectorList.Count + "/" + (data2.workingCollectorList.Count + data2.unworkingCollectorList.Count);
                    break;
            }
        }
    }
}
