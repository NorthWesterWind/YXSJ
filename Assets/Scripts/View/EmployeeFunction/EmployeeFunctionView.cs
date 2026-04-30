using Module;
using Module.Data;
using UnityEngine;
using Utils;

namespace View.EmployeeFunction
{
    public class EmployeeFunctionView : BaseView
    {
        public UIButton closeBtn;
        public GameObject yundizheObj;
        public UIButton yundizheBtn;
        public GameObject yundizheBtnMask;
        public GameObject content_1;
        public GameObject xuancaituObj_1;
        public UIButton xuancaituBtn_1;
        public GameObject xuancaituBtn_1Mask;
        public GameObject content_2;
        public GameObject xuancaituObj_2;

        public UIButton xuancaituBtn_2;
        public GameObject xuancaituBtn_2Mask;
        public GameObject content_3;
        public EmployeeContent employeeContent_1;
        public EmployeeContent employeeContent_2;
        public EmployeeContent employeeContent_3;

        public override void UpdateViewWithArgs(params object[] args)
        {
            base.UpdateViewWithArgs(args);
            UpdateInfo();
            ShowContent_1();
        }

        protected override void AddEventListener()
        {
            base.AddEventListener();
            closeBtn.onClick.RemoveAllListeners();
            closeBtn.onClick.AddListener((() =>
            {
                Hide();
            }));

            yundizheBtn.onClick.RemoveAllListeners();
            yundizheBtn.onClick.AddListener(ShowContent_1);
            xuancaituBtn_1.onClick.RemoveAllListeners();
            xuancaituBtn_1.onClick.AddListener(ShowContent_2);
            xuancaituBtn_2.onClick.RemoveAllListeners();
            xuancaituBtn_2.onClick.AddListener(ShowContent_3);

            EventCenter.Instance.AddListener(EventMessages.UpdateSturctureLockInfo, HandleStructureRefresh);
            EventCenter.Instance.AddListener(EventMessages.UpdateFunctionState, HandleStructureRefresh);
        }

        public override void RemoveEventListener()
        {
            base.RemoveEventListener();
            EventCenter.Instance.RemoveListener(EventMessages.UpdateSturctureLockInfo, HandleStructureRefresh);
            EventCenter.Instance.RemoveListener(EventMessages.UpdateFunctionState, HandleStructureRefresh);
        }

        protected override void OnHideComplete()
        {
            base.OnHideComplete();
            EventCenter.Instance.TriggerEvent(EventMessages.ShowPlayerInfoViewCartoon);
        }

        public void UpdateInfo()
        {
            PlayerData playerData = PlayerDataModule.Instance.data;
            bool hasDeliver = playerData.deliverData != null;
            bool hasLingChuGe1 = playerData.warehouselist != null &&
                                 playerData.warehouselist.Find(x => x.warehouseCategoryType == WarehouseCategoryType.LingChuGe_1) != null;
            bool hasLingChuGe2 = playerData.warehouselist != null &&
                                 playerData.warehouselist.Find(x => x.warehouseCategoryType == WarehouseCategoryType.LingChuGe_2) != null;

            yundizheObj.SetActive(hasDeliver);
            xuancaituObj_1.SetActive(hasLingChuGe1);
            xuancaituObj_2.SetActive(hasLingChuGe2);

            if (!hasDeliver && content_1.activeSelf)
            {
                if (hasLingChuGe1)
                {
                    ShowContent_2();
                }
                else if (hasLingChuGe2)
                {
                    ShowContent_3();
                }
            }
        }

        private void HandleStructureRefresh(params object[] args)
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            UpdateInfo();
        }

        public void ShowContent_1()
        {
            content_1.SetActive(true);
            content_2.SetActive(false);
            content_3.SetActive(false);
            yundizheBtnMask.SetActive(false);
            xuancaituBtn_1Mask.SetActive(true);
            xuancaituBtn_2Mask.SetActive(true);
            employeeContent_1.Init(EmployeeType.YunDiZhe);
        }
        public void ShowContent_2()
        {
            content_1.SetActive(false);
            content_2.SetActive(true);
            content_3.SetActive(false);
            yundizheBtnMask.SetActive(true);
            xuancaituBtn_1Mask.SetActive(false);
            xuancaituBtn_2Mask.SetActive(true);
            employeeContent_2.Init(EmployeeType.XuanCaiTu, BuildingType.LingChuGe_1);
        }

        public void ShowContent_3()
        {
            content_1.SetActive(false);
            content_2.SetActive(false);
            content_3.SetActive(true);
            yundizheBtnMask.SetActive(true);
            xuancaituBtn_1Mask.SetActive(true);
            xuancaituBtn_2Mask.SetActive(false);
            employeeContent_3.Init(EmployeeType.XuanCaiTu, BuildingType.LingChuGe_2);
        }
    }
}
