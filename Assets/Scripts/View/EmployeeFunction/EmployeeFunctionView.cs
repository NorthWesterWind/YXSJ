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
        public TextMeshProUGUI usepeopletxt;
        public UIButton addBtn1;
        public Image addImg1;
        public UIButton removeBtn1;
        public Image removeImg1;
        public TextMeshProUGUI progresstxt1    ;

        public Transform storageContent;
        
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
            addBtn1.onClick.RemoveAllListeners();
            addBtn1.onClick.AddListener((() =>
            {
                if (ModuleMgr.Instance.GetModule<PlayerDataModule>().data.workingNum == ModuleMgr.Instance.GetModule<PlayerDataModule>().data.totalNum)
                {
                    return;
                }
                else
                {
                    ModuleMgr.Instance.GetModule<PlayerDataModule>().data.workingNum += 1;
                    EventCenter.Instance.TriggerEvent(EventMessages.AddYunDiZhe);
                }

                UpdateInfo();
            }));
            removeBtn1.onClick.RemoveAllListeners();
            removeBtn1.onClick.AddListener((() =>
            {
                
                if (ModuleMgr.Instance.GetModule<PlayerDataModule>().data.workingNum < 1)
                {
                    return;
                }
                else
                {
                    ModuleMgr.Instance.GetModule<PlayerDataModule>().data.workingNum -= 1;
                    EventCenter.Instance.TriggerEvent(EventMessages.RemoveYunDiZhe);
                }

                UpdateInfo();
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
            usepeopletxt.text = playerData.workingNum + "/" + playerData.totalNum;
            progresstxt1.text = playerData.workingNum + "/" + playerData.totalNum;
        }
    }
}
