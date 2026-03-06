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

    private void OnEnable()
    {
        EventCenter.Instance.AddListener(EventMessages.UpdateYunDiZheInfo, HandleEmployeeStateChanged);
        EventCenter.Instance.AddListener(EventMessages.UpdateLingChuGeWorkingInfo, HandleEmployeeStateChanged);
    }

    private void OnDisable()
    {
        EventCenter.Instance.RemoveListener(EventMessages.UpdateYunDiZheInfo, HandleEmployeeStateChanged);
        EventCenter.Instance.RemoveListener(EventMessages.UpdateLingChuGeWorkingInfo, HandleEmployeeStateChanged);
    }




    public void Init(EmployeeType employeeType, BuildingType buildingType = BuildingType.None)
    {
        this.employeeType = employeeType;
        this.buildingType = buildingType;
        UpdatePeopleInfo();

        PlayerData playerData = PlayerDataModule.Instance.data;
        if (employeeType != EmployeeType.YunDiZhe)
        {
            Extensions.ClearChildren(content);
            if (buildingType == BuildingType.LingChuGe_1)
            {
                MapData mapData = DataController.Instance.mapDataDic[playerData.currentMapID];
                for (int i = 0; i < mapData.monsterFamilyList.Count; i++)
                {
                    GameObject obj = Instantiate(assetHandle.Get<GameObject>("EmployInfoItem"), content, false);

                    obj.GetComponent<EmployeeInfoItem>().Init((MonsterFamily)mapData.monsterFamilyList[i], 2);
                }
            }
            else
            {
                MapData mapData = DataController.Instance.mapDataDic[playerData.currentMapID];
                for (int i = 0; i < mapData.monsterFamilyList.Count; i++)
                {
                    GameObject obj = Instantiate(assetHandle.Get<GameObject>("EmployInfoItem"), content, false);

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
                // 防御：已无空闲云递者则不再增加
                if (playerData.deliverData.workingNum >= playerData.deliverData.totalNum)
                    return;

                playerData.deliverData.workingNum += 1;
                // 更新云递阁工人数对应的搬运工实例
                EventCenter.Instance.TriggerEvent(EventMessages.UpdateYunDiZheInfo);
                // 如有其他地方监听工作状态，可以继续触发
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
                if (playerData.deliverData.workingNum <= 0)
                    return;

                playerData.deliverData.workingNum -= 1;
                EventCenter.Instance.TriggerEvent(EventMessages.UpdateYunDiZheInfo);
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

    private void HandleEmployeeStateChanged(params object[] args)
    {
        UpdatePeopleInfo();

        if (employeeType == EmployeeType.YunDiZhe)
        {
            RefreshYunDiZheState();
            return;
        }

        if (content == null)
        {
            return;
        }

        var items = content.GetComponentsInChildren<EmployeeInfoItem>(true);
        foreach (var item in items)
        {
            item.UpdateInfo();
        }
    }

    private void RefreshYunDiZheState()
    {
        var deliverData = PlayerDataModule.Instance.data?.deliverData;
        if (deliverData == null)
        {
            return;
        }

        if (progressTxt != null)
        {
            progressTxt.text = deliverData.workingNum + "/" + deliverData.totalNum;
        }

        if (addMask != null)
        {
            addMask.gameObject.SetActive(deliverData.totalNum <= deliverData.workingNum);
        }

        if (removeMask != null)
        {
            removeMask.gameObject.SetActive(deliverData.workingNum <= 0);
        }
    }

    public void UpdatePeopleInfo(params object[] args)
    {
        if (employeeType == EmployeeType.YunDiZhe)
        {
            peopleCountTxt.text = "空闲人数: " + (PlayerDataModule.Instance.data.deliverData.totalNum - PlayerDataModule.Instance.data.deliverData.workingNum);
        }
        else
        {
            switch (buildingType)
            {
                case BuildingType.LingChuGe_1:
                    WarehouseCategory data1 = PlayerDataModule.Instance.data.warehouselist.Find(x => x.warehouseCategoryType == WarehouseCategoryType.LingChuGe_1);
                    if (data1 != null)
                    {
                          peopleCountTxt.text = "空闲人数: " + data1.unworkingCollectorList.Count ;
                    }
                    else
                    {
                          peopleCountTxt.text = "空闲人数: 0" ;
                    }
                  
                    break;
                case BuildingType.LingChuGe_2:
                    WarehouseCategory data2 = PlayerDataModule.Instance.data.warehouselist.Find(x => x.warehouseCategoryType == WarehouseCategoryType.LingChuGe_2);
                    if (data2 != null)
                    {
                          peopleCountTxt.text = "空闲人数: " + data2.unworkingCollectorList.Count ;
                    }
                    else
                    {
                          peopleCountTxt.text = "空闲人数: 0" ;
                    }
                    break;
            }
        }
    }
}
