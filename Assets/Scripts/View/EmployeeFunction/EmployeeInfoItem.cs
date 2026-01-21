using System;
using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace View.EmployeeFunction
{
    public class EmployeeInfoItem : MonoBehaviour
    {
        public Image image;
        public UIButton removeBtn;
        public UIButton addBtn;
        public TextMeshProUGUI progresstxt;
        public TextMeshProUGUI nametxt;
        public Image addBtnMask;
        public Image removeBtnMask;
        private AssetHandle _assetHandle;
        public MonsterFamily monstertype;
        public BuildingType buildingType;
        //public int workingnum;
        // public WarehouseCategory category;
        public int value;
        PlayerData playerData;

        public void Init(System.Object type, int value)
        {
            if (_assetHandle == null)
            {
                _assetHandle = GetComponent<AssetHandle>();
            }
            this.value = value;
            playerData = PlayerDataModule.Instance.data;
            if (type is MonsterFamily familyType)
            {
                monstertype = familyType;
                image.sprite = _assetHandle.Get<Sprite>(Extensions.GetMonsterPictureNameByType(monstertype));
                switch (type)
                {
                    case MonsterFamily.ShuangYunZhi:
                        nametxt.text = "霜云芝";
                        break;
                    case MonsterFamily.YueLuCao:
                        nametxt.text = "月露草";
                        break;

                    case MonsterFamily.ZiXinHua:
                        nametxt.text = "栀心花";
                        break;

                    case MonsterFamily.YuHuiHe:
                        nametxt.text = "玉穗禾";
                        break;

                    case MonsterFamily.XingWenGuo:
                        nametxt.text = "星纹果";
                        break;

                    case MonsterFamily.WuRongJun:
                        nametxt.text = "雾茸菌";
                        break;

                    case MonsterFamily.LingXuSheng:
                        nametxt.text = "灵须参";
                        break;

                    case MonsterFamily.XueBanHua:
                        nametxt.text = "雪瓣花";
                        break;

                    case MonsterFamily.MuLingYa:
                        nametxt.text = "木灵芽";
                        break;

                    case MonsterFamily.JingRuiCao:
                        nametxt.text = "晶蕊草";
                        break;

                    case MonsterFamily.TieKuangShi:
                        nametxt.text = "铁矿石";
                        break;

                    case MonsterFamily.YinKuangShi:
                        nametxt.text = "银矿石";
                        break;

                    case MonsterFamily.TongKuangShi:
                        nametxt.text = "铜矿石";
                        break;

                    case MonsterFamily.ZiJingShi:
                        nametxt.text = "紫晶石";
                        break;

                    case MonsterFamily.YueJingShi:
                        nametxt.text = "月晶石";
                        break;

                }
            }
            else
            {
                buildingType = (BuildingType)type;
                if (buildingType == BuildingType.LingChaJia_1 || buildingType == BuildingType.LingChaJia_2 ||
                    buildingType == BuildingType.LingChaJia_3 || buildingType == BuildingType.LingChaJia_4)
                {
                    image.sprite = _assetHandle.Get<Sprite>("LingChaJia");
                }
                else
                {
                    image.sprite = _assetHandle.Get<Sprite>("LianQiJia");
                }
                switch (buildingType)
                {
                    case BuildingType.LingChaJia_1:
                        nametxt.text = "一号灵茶架";
                        break;
                    case BuildingType.LingChaJia_2:
                        nametxt.text = "二号灵茶架";
                        break;
                    case BuildingType.LingChaJia_3:
                        nametxt.text = "三号灵茶架";
                        break;
                    case BuildingType.LingChaJia_4:
                        nametxt.text = "四号灵茶架";
                        break;
                    case BuildingType.LingQiJia_1:
                        nametxt.text = "一号灵器架";
                        break;
                    case BuildingType.LingQiJia_2:
                        nametxt.text = "二号炼器炉";
                        break;
                    case BuildingType.LingQiJia_3:
                        nametxt.text = "三号炼器炉";
                        break;
                }

            }
            UpdateInfo();
        }

        private void Start()
        {
            removeBtn.onClick.RemoveAllListeners();
            removeBtn.onClick.AddListener((() =>
            {

                //    if (workingnum<1)
                //    {
                //        return;
                //    }
                //    else
                //    {
                //        foreach (var data in category. workingCollectorList)
                //        {
                //            if (data.monsterType == monstertype)
                //            {
                //                data.monsterType = MonsterFamily.None;
                //                var temp = data;
                //                category.unworkingCollectorList.Add(temp);
                //                break;
                //            }
                //        }
                //        UpdateInfo();
                //        EventCenter.Instance.TriggerEvent(EventMessages. LingChuGeStopDelivery);
                //    }

                switch (value)
                {
                    case 1:
                        playerData.deliverData.yunDiZheWorkingBuildList.Remove(buildingType);
                        playerData.deliverData.workingNum -= 1;
                        UpdateInfo();
                        EventCenter.Instance.TriggerEvent(EventMessages.UpdateYunDiZheInfo);
                        break;

                    case 2:
                        WarehouseCategory warehouseCategory1 = playerData.warehouselist.Find(x => x.warehouseCategoryType == WarehouseCategoryType.LingChuGe_1);
                        var collector1 = warehouseCategory1.workingCollectorList.Find(x => x.monsterType == monstertype);
                        warehouseCategory1.workingCollectorList.RemoveAt(warehouseCategory1.unworkingCollectorList.Count - 1);
                        collector1.monsterType = MonsterFamily.None;
                        warehouseCategory1.unworkingCollectorList.Add(collector1);
                        UpdateInfo();
                        EventCenter.Instance.TriggerEvent(EventMessages.UpdateLingChuGeWorkingInfo);
                        break;

                    case 3:
                        WarehouseCategory warehouseCategory2 = playerData.warehouselist.Find(x => x.warehouseCategoryType == WarehouseCategoryType.LingChuGe_2);
                        var collector2 = warehouseCategory2.workingCollectorList.Find(x => x.monsterType == monstertype);
                        warehouseCategory2.workingCollectorList.RemoveAt(warehouseCategory2.unworkingCollectorList.Count - 1);
                        collector2.monsterType = MonsterFamily.None;
                        warehouseCategory2.unworkingCollectorList.Add(collector2);
                        UpdateInfo();
                        EventCenter.Instance.TriggerEvent(EventMessages.UpdateLingChuGeWorkingInfo);
                        break;
                }


            }));

            addBtn.onClick.RemoveAllListeners();
            addBtn.onClick.AddListener((() =>
            {
                //    if (workingnum > 0)
                //    {
                //        UIController.Instance.Show<TipView>("当前已派遣玄采徒进行采集！");
                //        return;
                //    }
                //    if (category.unworkingCollectorList.Count < 1)
                //    {
                //        UIController.Instance.Show<TipView>("当前无可以派遣的玄采徒！");
                //        return;
                //    }   
                //    else
                //    {
                //        var temp = category.unworkingCollectorList[0];
                //        temp.monsterType = monstertype;
                //        category.unworkingCollectorList.RemoveAt(0);
                //        category.workingCollectorList.Add(temp);
                //        UpdateInfo();
                //       EventCenter.Instance.TriggerEvent(EventMessages.LingChuGeBeginWorking);
                //    }
                switch (value)
                {
                    case 1:
                        playerData.deliverData.yunDiZheWorkingBuildList.Add(buildingType);
                        playerData.deliverData.workingNum += 1;
                        UpdateInfo();
                        EventCenter.Instance.TriggerEvent(EventMessages.UpdateYunDiZheInfo);
                        break;
                    case 2:
                        WarehouseCategory warehouseCategory1 = playerData.warehouselist.Find(x => x.warehouseCategoryType == WarehouseCategoryType.LingChuGe_1);
                        var collector1 = warehouseCategory1.unworkingCollectorList[warehouseCategory1.unworkingCollectorList.Count - 1];
                        warehouseCategory1.unworkingCollectorList.RemoveAt(warehouseCategory1.unworkingCollectorList.Count - 1);
                        collector1.monsterType = monstertype;
                        warehouseCategory1.workingCollectorList.Add(collector1);
                        UpdateInfo();
                        EventCenter.Instance.TriggerEvent(EventMessages.UpdateLingChuGeWorkingInfo);
                        break;

                    case 3:
                        WarehouseCategory warehouseCategory2 = playerData.warehouselist.Find(x => x.warehouseCategoryType == WarehouseCategoryType.LingChuGe_2);
                        var collector2 = warehouseCategory2.unworkingCollectorList[warehouseCategory2.unworkingCollectorList.Count - 1];
                        warehouseCategory2.unworkingCollectorList.RemoveAt(warehouseCategory2.unworkingCollectorList.Count - 1);
                        collector2.monsterType = monstertype;
                        warehouseCategory2.workingCollectorList.Add(collector2);
                        UpdateInfo();
                        EventCenter.Instance.TriggerEvent(EventMessages.UpdateLingChuGeWorkingInfo);
                        break;
                }

            }));
        }

        public void UpdateInfo()
        {
            //    progresstxt.text = "0/1";
            //    workingnum = 0;
            //    foreach (var data in category.workingCollectorList)
            //    {
            //        if (data.monsterType == monstertype)
            //        {
            //            progresstxt.text = "1/1";
            //            workingnum = 1;
            //            break;
            //        }
            //    }
            //    if (workingnum == 0)
            //    {
            //        mask1.enabled = false;
            //    }
            //    else
            //    {
            //        mask1.enabled = true;
            //    }

            switch (value)
            {
                case 1:
                    if (playerData.deliverData.yunDiZheWorkingBuildList.Contains(buildingType))
                    {
                        addBtnMask.gameObject.SetActive(true);
                        removeBtnMask.gameObject.SetActive(false);
                        progresstxt.text = "1/1";
                    }
                    else
                    {
                        progresstxt.text = "0/1";
                        if (playerData.deliverData.totalNum - playerData.deliverData.workingNum > 0)
                        {
                            addBtnMask.gameObject.SetActive(false);
                            removeBtnMask.gameObject.SetActive(true);
                        }
                        else
                        {
                            addBtnMask.gameObject.SetActive(true);
                            removeBtnMask.gameObject.SetActive(true);
                        }
                    }
                    break;
                case 2:
                    WarehouseCategory warehouseCategory1 = playerData.warehouselist.Find(x => x.warehouseCategoryType == WarehouseCategoryType.LingChuGe_1);
                    var collector1 = warehouseCategory1.workingCollectorList.Find(x => x.monsterType == monstertype);
                    if (collector1 != null)
                    {
                        addBtnMask.gameObject.SetActive(true);
                        removeBtnMask.gameObject.SetActive(false);
                        progresstxt.text = "1/1";
                    }
                    else
                    {
                        progresstxt.text = "0/1";
                        if (warehouseCategory1.unworkingCollectorList.Count > 0)
                        {
                            addBtnMask.gameObject.SetActive(false);
                            removeBtnMask.gameObject.SetActive(true);
                        }
                        else
                        {
                            addBtnMask.gameObject.SetActive(true);
                            removeBtnMask.gameObject.SetActive(true);
                        }
                    }
                    break;
                case 3:
                    WarehouseCategory warehouseCategory2 = playerData.warehouselist.Find(x => x.warehouseCategoryType == WarehouseCategoryType.LingChuGe_2);
                    var collector2 = warehouseCategory2.workingCollectorList.Find(x => x.monsterType == monstertype);
                    if (collector2 != null)
                    {
                        addBtnMask.gameObject.SetActive(true);
                        removeBtnMask.gameObject.SetActive(false);
                        progresstxt.text = "1/1";
                    }
                    else
                    {
                        progresstxt.text = "0/1";
                        if (warehouseCategory2.unworkingCollectorList.Count > 0)
                        {
                            addBtnMask.gameObject.SetActive(false);
                            removeBtnMask.gameObject.SetActive(true);
                        }
                        else
                        {
                            addBtnMask.gameObject.SetActive(true);
                            removeBtnMask.gameObject.SetActive(true);
                        }
                    }
                    break;
            }
        }
    }
}
