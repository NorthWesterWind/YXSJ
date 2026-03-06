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
        public int value;

        private PlayerData playerData;
        private bool clickBound;

        public void Init(MonsterFamily type, int value)
        {
            if (_assetHandle == null)
            {
                _assetHandle = GetComponent<AssetHandle>();
            }

            this.value = value;
            monstertype = type;
            playerData = PlayerDataModule.Instance.data;

            if (image != null && _assetHandle != null)
            {
                image.sprite = _assetHandle.Get<Sprite>(Extensions.GetMonsterPictureNameByType(type));
            }

            if (nametxt != null)
            {
                nametxt.text = GetMonsterDisplayName(type);
            }

            BindButtons();
            UpdateInfo();
        }

        private void OnEnable()
        {
            BindButtons();
        }

        private void BindButtons()
        {
            if (clickBound)
            {
                return;
            }

            if (removeBtn != null)
            {
                removeBtn.onClick.RemoveAllListeners();
                removeBtn.onClick.AddListener(OnClickRemove);
            }

            if (addBtn != null)
            {
                addBtn.onClick.RemoveAllListeners();
                addBtn.onClick.AddListener(OnClickAdd);
            }

            clickBound = true;
        }

        private void OnClickRemove()
        {
            playerData = PlayerDataModule.Instance.data;
            if (playerData == null)
            {
                return;
            }

            bool changed = false;
            switch (value)
            {
                case 1:
                    changed = TryUnassignYunDi();
                    if (changed)
                    {
                        EventCenter.Instance.TriggerEvent(EventMessages.UpdateYunDiZheInfo);
                    }
                    break;
                case 2:
                case 3:
                    changed = TryUnassignCollector();
                    if (changed)
                    {
                        EventCenter.Instance.TriggerEvent(EventMessages.UpdateLingChuGeWorkingInfo);
                    }
                    break;
            }

            if (changed)
            {
                UpdateInfo();
            }
        }

        private void OnClickAdd()
        {
            playerData = PlayerDataModule.Instance.data;
            if (playerData == null)
            {
                return;
            }

            bool changed = false;
            switch (value)
            {
                case 1:
                    changed = TryAssignYunDi();
                    if (changed)
                    {
                        EventCenter.Instance.TriggerEvent(EventMessages.UpdateYunDiZheInfo);
                    }
                    break;
                case 2:
                case 3:
                    changed = TryAssignCollector();
                    if (changed)
                    {
                        EventCenter.Instance.TriggerEvent(EventMessages.UpdateLingChuGeWorkingInfo);
                    }
                    break;
            }

            if (changed)
            {
                UpdateInfo();
            }
        }

        private bool TryAssignYunDi()
        {
            if (playerData?.deliverData == null)
            {
                return false;
            }

            var deliverData = playerData.deliverData;
            if (deliverData.yunDiZheWorkingBuildList.Contains(buildingType))
            {
                return false;
            }

            if (deliverData.workingNum >= deliverData.totalNum)
            {
                return false;
            }

            deliverData.yunDiZheWorkingBuildList.Add(buildingType);
            deliverData.workingNum += 1;
            return true;
        }

        private bool TryUnassignYunDi()
        {
            if (playerData?.deliverData == null)
            {
                return false;
            }

            var deliverData = playerData.deliverData;
            if (!deliverData.yunDiZheWorkingBuildList.Contains(buildingType))
            {
                return false;
            }

            deliverData.yunDiZheWorkingBuildList.Remove(buildingType);
            deliverData.workingNum = Mathf.Max(0, deliverData.workingNum - 1);
            return true;
        }

        private bool TryAssignCollector()
        {
            var warehouse = GetWarehouseByValue();
            if (warehouse == null)
            {
                return false;
            }

            if (warehouse.workingCollectorList.Exists(x => x.monsterType == monstertype))
            {
                return false;
            }

            if (warehouse.unworkingCollectorList.Count <= 0)
            {
                return false;
            }

            var freeCollector = warehouse.unworkingCollectorList[warehouse.unworkingCollectorList.Count - 1];
            warehouse.unworkingCollectorList.RemoveAt(warehouse.unworkingCollectorList.Count - 1);
            freeCollector.monsterType = monstertype;
            warehouse.workingCollectorList.Add(freeCollector);
            return true;
        }

        private bool TryUnassignCollector()
        {
            var warehouse = GetWarehouseByValue();
            if (warehouse == null)
            {
                return false;
            }

            var workingCollector = warehouse.workingCollectorList.Find(x => x.monsterType == monstertype);
            if (workingCollector == null)
            {
                return false;
            }

            warehouse.workingCollectorList.Remove(workingCollector);
            workingCollector.monsterType = MonsterFamily.None;
            warehouse.unworkingCollectorList.Add(workingCollector);
            return true;
        }

        private WarehouseCategory GetWarehouseByValue()
        {
            if (playerData?.warehouselist == null)
            {
                return null;
            }

            if (value == 2)
            {
                return playerData.warehouselist.Find(x => x.warehouseCategoryType == WarehouseCategoryType.LingChuGe_1);
            }

            if (value == 3)
            {
                return playerData.warehouselist.Find(x => x.warehouseCategoryType == WarehouseCategoryType.LingChuGe_2);
            }

            return null;
        }

        public void UpdateInfo()
        {
            playerData = PlayerDataModule.Instance.data;
            if (playerData == null)
            {
                ApplyItemState(false, false, false);
                return;
            }

            switch (value)
            {
                case 1:
                {
                    if (playerData.deliverData == null)
                    {
                        ApplyItemState(false, false, false);
                        return;
                    }

                    bool assigned = playerData.deliverData.yunDiZheWorkingBuildList.Contains(buildingType);
                    bool canAdd = !assigned && (playerData.deliverData.totalNum - playerData.deliverData.workingNum > 0);
                    bool canRemove = assigned;
                    ApplyItemState(assigned, canAdd, canRemove);
                    break;
                }
                case 2:
                case 3:
                {
                    var warehouse = GetWarehouseByValue();
                    if (warehouse == null)
                    {
                        ApplyItemState(false, false, false);
                        return;
                    }

                    bool assigned = warehouse.workingCollectorList.Exists(x => x.monsterType == monstertype);
                    bool canAdd = !assigned && warehouse.unworkingCollectorList.Count > 0;
                    bool canRemove = assigned;
                    ApplyItemState(assigned, canAdd, canRemove);
                    break;
                }
                default:
                    ApplyItemState(false, false, false);
                    break;
            }
        }

        private void ApplyItemState(bool assigned, bool canAdd, bool canRemove)
        {
            if (progresstxt != null)
            {
                progresstxt.text = assigned ? "1/1" : "0/1";
            }

            if (addBtnMask != null)
            {
                addBtnMask.gameObject.SetActive(!canAdd);
            }

            if (removeBtnMask != null)
            {
                removeBtnMask.gameObject.SetActive(!canRemove);
            }

            if (addBtn != null)
            {
                addBtn.interactable = canAdd;
            }

            if (removeBtn != null)
            {
                removeBtn.interactable = canRemove;
            }
        }

        private string GetMonsterDisplayName(MonsterFamily type)
        {
            switch (type)
            {
                case MonsterFamily.ShuangYunZhi: return "霜云芝";
                case MonsterFamily.YueLuCao: return "月露草";
                case MonsterFamily.ZiXinHua: return "栀心花";
                case MonsterFamily.YuHuiHe: return "玉穗禾";
                case MonsterFamily.XingWenGuo: return "星纹果";
                case MonsterFamily.WuRongJun: return "雾茸菌";
                case MonsterFamily.LingXuSheng: return "灵须参";
                case MonsterFamily.XueBanHua: return "雪瓣花";
                case MonsterFamily.MuLingYa: return "木灵芽";
                case MonsterFamily.JingRuiCao: return "晶蕊草";
                case MonsterFamily.TieKuangShi: return "铁矿石";
                case MonsterFamily.YinKuangShi: return "银矿石";
                case MonsterFamily.TongKuangShi: return "铜矿石";
                case MonsterFamily.ZiJingShi: return "紫晶石";
                case MonsterFamily.YueJingShi: return "月晶石";
                default: return type.ToString();
            }
        }
    }
}
