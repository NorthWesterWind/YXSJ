using System;
using Module;
using UnityEngine;
using Utils;

namespace View
{
    public class SevenDayView : BaseView
    {
        public UIButton closeBtn; 
        public UIButton btn_1;
        public UIButton btn_2;
        public UIButton btn_3;
        public UIButton btn_4;
        public UIButton btn_5;
        public UIButton btn_6;
        public UIButton btn_7;

        public GameObject mask_1;
        public GameObject mask_2;
        public GameObject mask_3;
        public GameObject mask_4;
        public GameObject mask_5;
        public GameObject mask_6;
        public GameObject mask_7;
        public GameObject kuang1;
        public GameObject kuang2;
        public GameObject kuang3;
        public GameObject kuang4;
        public GameObject kuang5;
        public GameObject kuang6;
        public GameObject kuang7;
        protected override void AddEventListener()
        {
            base.AddEventListener();
            closeBtn.onClick.RemoveAllListeners();
            closeBtn.onClick.AddListener(OnClickCloseBtn);
            btn_1.onClick.RemoveAllListeners();
            btn_1.onClick.AddListener((() =>
            {
                if (!ModuleMgr.Instance.GetModule<PlayerDataModule>().data.sevenDayRecordList.Contains(1) &&
                    ModuleMgr.Instance.GetModule<PlayerDataModule>().data.sevenDayRecordTime !=
                    DateTime.Now.ToString("yyyy/MM/dd")
                    && ModuleMgr.Instance.GetModule<PlayerDataModule>().data.GetSevenDayRewardIndex==0)
                {
                    ModuleMgr.Instance.GetModule<PlayerDataModule>().GetSevenDayReward(1);
                    mask_1.gameObject.SetActive(true);
                    UIController.Instance.Show<TipView>("领取成功！");
                    ModuleMgr.Instance.GetModule<PlayerDataModule>().data.GetSevenDayRewardIndex += 1;
                    kuang1.SetActive(false);
                }
            }));
            
            btn_2.onClick.RemoveAllListeners();
            btn_2.onClick.AddListener((() =>
            {
                if (!ModuleMgr.Instance.GetModule<PlayerDataModule>().data.sevenDayRecordList.Contains(2) &&
                    ModuleMgr.Instance.GetModule<PlayerDataModule>().data.sevenDayRecordTime !=
                    DateTime.Now.ToString("yyyy/MM/dd")&& ModuleMgr.Instance.GetModule<PlayerDataModule>().data.GetSevenDayRewardIndex == 1)
                {
                    ModuleMgr.Instance.GetModule<PlayerDataModule>().GetSevenDayReward(2);
                    mask_2.gameObject.SetActive(true);
                    UIController.Instance.Show<TipView>("领取成功！");
                       ModuleMgr.Instance.GetModule<PlayerDataModule>().data.GetSevenDayRewardIndex += 1;
                    kuang2.SetActive(false);
                }
            }));
            btn_3.onClick.RemoveAllListeners();
            btn_3.onClick.AddListener((() =>
            {
                if (!ModuleMgr.Instance.GetModule<PlayerDataModule>().data.sevenDayRecordList.Contains(3) &&
                    ModuleMgr.Instance.GetModule<PlayerDataModule>().data.sevenDayRecordTime !=
                    DateTime.Now.ToString("yyyy/MM/dd")&& ModuleMgr.Instance.GetModule<PlayerDataModule>().data.GetSevenDayRewardIndex == 2)
                {
                    ModuleMgr.Instance.GetModule<PlayerDataModule>().GetSevenDayReward(3);
                    mask_3.gameObject.SetActive(true);
                    UIController.Instance.Show<TipView>("领取成功！");
                       ModuleMgr.Instance.GetModule<PlayerDataModule>().data.GetSevenDayRewardIndex += 1;
                    kuang3.SetActive(false);
                }
            }));
            
            btn_4.onClick.RemoveAllListeners();
            btn_4.onClick.AddListener((() =>
            {
                if (!ModuleMgr.Instance.GetModule<PlayerDataModule>().data.sevenDayRecordList.Contains(4) &&
                    ModuleMgr.Instance.GetModule<PlayerDataModule>().data.sevenDayRecordTime !=
                    DateTime.Now.ToString("yyyy/MM/dd")
                    && ModuleMgr.Instance.GetModule<PlayerDataModule>().data.GetSevenDayRewardIndex == 3)
                {
                    ModuleMgr.Instance.GetModule<PlayerDataModule>().GetSevenDayReward(4);
                    mask_4.gameObject.SetActive(true);
                    UIController.Instance.Show<TipView>("领取成功！");
                      kuang4.SetActive(false);
                }
            }));
            
            btn_5.onClick.RemoveAllListeners();
            btn_5.onClick.AddListener((() =>
            {
                if (!ModuleMgr.Instance.GetModule<PlayerDataModule>().data.sevenDayRecordList.Contains(5) &&
                    ModuleMgr.Instance.GetModule<PlayerDataModule>().data.sevenDayRecordTime !=
                    DateTime.Now.ToString("yyyy/MM/dd")  && ModuleMgr.Instance.GetModule<PlayerDataModule>().data.GetSevenDayRewardIndex == 4)
                {
                    ModuleMgr.Instance.GetModule<PlayerDataModule>().GetSevenDayReward(5);
                    mask_5.gameObject.SetActive(true);
                    UIController.Instance.Show<TipView>("领取成功！");
                    ModuleMgr.Instance.GetModule<PlayerDataModule>().data.GetSevenDayRewardIndex += 1;
                    kuang5.SetActive(false);
                }
            }));
            
            btn_6.onClick.RemoveAllListeners();
            btn_6.onClick.AddListener((() =>
            {
                if (!ModuleMgr.Instance.GetModule<PlayerDataModule>().data.sevenDayRecordList.Contains(6) &&
                    ModuleMgr.Instance.GetModule<PlayerDataModule>().data.sevenDayRecordTime !=
                    DateTime.Now.ToString("yyyy/MM/dd") && ModuleMgr.Instance.GetModule<PlayerDataModule>().data.GetSevenDayRewardIndex == 5)
                {
                    ModuleMgr.Instance.GetModule<PlayerDataModule>().GetSevenDayReward(6);
                    mask_6.gameObject.SetActive(true);
                    UIController.Instance.Show<TipView>("领取成功！");
                    ModuleMgr.Instance.GetModule<PlayerDataModule>().data.GetSevenDayRewardIndex += 1;
                    kuang6.SetActive(false);
                }
            }));
            btn_7.onClick.RemoveAllListeners();
            btn_7.onClick.AddListener((() =>
            {
                if (!ModuleMgr.Instance.GetModule<PlayerDataModule>().data.sevenDayRecordList.Contains(7) &&
                    ModuleMgr.Instance.GetModule<PlayerDataModule>().data.sevenDayRecordTime !=
                    DateTime.Now.ToString("yyyy/MM/dd") && ModuleMgr.Instance.GetModule<PlayerDataModule>().data.GetSevenDayRewardIndex == 6)
                {
                    ModuleMgr.Instance.GetModule<PlayerDataModule>().GetSevenDayReward(7);
                    mask_7.gameObject.SetActive(true);
                    UIController.Instance.Show<TipView>("领取成功！");
                    ModuleMgr.Instance.GetModule<PlayerDataModule>().data.GetSevenDayRewardIndex += 1;
                    kuang7.SetActive(false);
                }
            }));
        }

        public override void UpdateViewWithArgs(params object[] args)
        {
            base.UpdateViewWithArgs(args);
            UpdateMaskState();
            kuang1.SetActive(!ModuleMgr.Instance.GetModule<PlayerDataModule>().data.sevenDayRecordList.Contains(1) && ModuleMgr.Instance.GetModule<PlayerDataModule>().data.GetSevenDayRewardIndex==0 && ModuleMgr.Instance.GetModule<PlayerDataModule>().data.sevenDayRecordTime != DateTime.Now.ToString("yyyy/MM/dd"));
            kuang2.SetActive(!ModuleMgr.Instance.GetModule<PlayerDataModule>().data.sevenDayRecordList.Contains(2) && ModuleMgr.Instance.GetModule<PlayerDataModule>().data.GetSevenDayRewardIndex==1 && ModuleMgr.Instance.GetModule<PlayerDataModule>().data.sevenDayRecordTime != DateTime.Now.ToString("yyyy/MM/dd"));
            kuang3.SetActive(!ModuleMgr.Instance.GetModule<PlayerDataModule>().data.sevenDayRecordList.Contains(3) && ModuleMgr.Instance.GetModule<PlayerDataModule>().data.GetSevenDayRewardIndex==2 && ModuleMgr.Instance.GetModule<PlayerDataModule>().data.sevenDayRecordTime != DateTime.Now.ToString("yyyy/MM/dd"));
            kuang4.SetActive(!ModuleMgr.Instance.GetModule<PlayerDataModule>().data.sevenDayRecordList.Contains(4) && ModuleMgr.Instance.GetModule<PlayerDataModule>().data.GetSevenDayRewardIndex==3 && ModuleMgr.Instance.GetModule<PlayerDataModule>().data.sevenDayRecordTime != DateTime.Now.ToString("yyyy/MM/dd"));
            kuang5.SetActive(!ModuleMgr.Instance.GetModule<PlayerDataModule>().data.sevenDayRecordList.Contains(5) && ModuleMgr.Instance.GetModule<PlayerDataModule>().data.GetSevenDayRewardIndex==4 && ModuleMgr.Instance.GetModule<PlayerDataModule>().data.sevenDayRecordTime != DateTime.Now.ToString("yyyy/MM/dd"));
            kuang6.SetActive(!ModuleMgr.Instance.GetModule<PlayerDataModule>().data.sevenDayRecordList.Contains(6) && ModuleMgr.Instance.GetModule<PlayerDataModule>().data.GetSevenDayRewardIndex==5 && ModuleMgr.Instance.GetModule<PlayerDataModule>().data.sevenDayRecordTime != DateTime.Now.ToString("yyyy/MM/dd"));
            kuang7.SetActive(!ModuleMgr.Instance.GetModule<PlayerDataModule>().data.sevenDayRecordList.Contains(7) && ModuleMgr.Instance.GetModule<PlayerDataModule>().data.GetSevenDayRewardIndex==6 && ModuleMgr.Instance.GetModule<PlayerDataModule>().data.sevenDayRecordTime != DateTime.Now.ToString("yyyy/MM/dd"));
        }

        private void OnClickCloseBtn()
        {
            Hide();
        }
        protected override void OnHideComplete()
        {
            base.OnHideComplete();
            EventCenter.Instance.TriggerEvent(EventMessages.ShowPlayerInfoViewCartoon);
        }
        
        
        
        public void UpdateMaskState()
        {
            mask_1.gameObject.SetActive(ModuleMgr.Instance.GetModule<PlayerDataModule>().data.sevenDayRecordList.Contains(1));
            mask_2.gameObject.SetActive(ModuleMgr.Instance.GetModule<PlayerDataModule>().data.sevenDayRecordList.Contains(2));
            mask_3.gameObject.SetActive(ModuleMgr.Instance.GetModule<PlayerDataModule>().data.sevenDayRecordList.Contains(3));
            mask_4.gameObject.SetActive(ModuleMgr.Instance.GetModule<PlayerDataModule>().data.sevenDayRecordList.Contains(4));
            mask_5.gameObject.SetActive(ModuleMgr.Instance.GetModule<PlayerDataModule>().data.sevenDayRecordList.Contains(5));
            mask_6.gameObject.SetActive(ModuleMgr.Instance.GetModule<PlayerDataModule>().data.sevenDayRecordList.Contains(6));
            mask_7.gameObject.SetActive(ModuleMgr.Instance.GetModule<PlayerDataModule>().data.sevenDayRecordList.Contains(7));

        }
    }
}
