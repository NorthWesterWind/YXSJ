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
            if (yundizheObj.activeSelf)
            {
                 ShowContent_1();
            }
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
        }

        protected override void OnHideComplete()
        {
            base.OnHideComplete();
            EventCenter.Instance.TriggerEvent(EventMessages.ShowPlayerInfoViewCartoon);
        }

        public void UpdateInfo()
        {
            PlayerData playerData = PlayerDataModule.Instance.data;
            if(playerData.deliverData == null)
            {
                yundizheObj.SetActive(false);
            }
            if(playerData.warehouselist.Find(x => x.warehouseCategoryType == WarehouseCategoryType.LingChuGe_1) == null)
            {
                xuancaituObj_1.SetActive(false);
            }
            if(playerData.warehouselist.Find(x => x.warehouseCategoryType == WarehouseCategoryType.LingChuGe_2) == null)
            {
                xuancaituObj_2.SetActive(false);
            }
        }

        public void ShowContent_1()
        {
            if (content_1.activeSelf)
                return;
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
            if (content_2.activeSelf)
                return;
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
            if (content_3.activeSelf)
                return;
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
