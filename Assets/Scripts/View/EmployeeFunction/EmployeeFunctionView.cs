using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

namespace View.EmployeeFunction
{
    public class EmployeeFunctionView : BaseView
    {
        public UIButton closeBtn;
        public GameObject yundizheBtnObj;
        public UIButton yundizheBtn;
        public GameObject yundizheBtnMask;
        public GameObject content_1;


        public GameObject xuancaituBtnObj_1;
        public UIButton xuancaituBtn_1;
        public GameObject xuancaituBtn_1Mask;
        public GameObject content_2;
        public GameObject xuancaituBtnObj_2;
        public UIButton xuancaituBtn_2;
        public GameObject xuancaituBtn_2Mask;
        public GameObject content_3;

        public override void UpdateViewWithArgs(params object[] args)
        {
            base.UpdateViewWithArgs(args);
            UpdateInfo();
        }

        protected override void AddEventListener()
        {
            base.AddEventListener();
            closeBtn.onClick.RemoveAllListeners();
            closeBtn.onClick.AddListener((() =>
            {
                Hide();
            }));
           
        }

        protected override void OnHideComplete()
        {
            base.OnHideComplete();
            EventCenter.Instance.TriggerEvent(EventMessages.ShowPlayerInfoViewCartoon);
        }

        public void UpdateInfo()
        {
            PlayerData playerData = ModuleMgr.Instance.GetModule<PlayerDataModule>().data;
        
        }

        //public 
    }
}
