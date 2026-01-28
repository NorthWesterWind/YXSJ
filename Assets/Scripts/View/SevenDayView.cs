using System;
using System.Collections.Generic;
using Module;
using Module.Data;
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
                if (!PlayerDataModule.Instance.data.sevenDayRecordList.Contains(1) &&
                    PlayerDataModule.Instance.data.sevenDayRecordTime !=
                    DateTime.Now.ToString("yyyy/MM/dd")
                    && PlayerDataModule.Instance.data.GetSevenDayRewardIndex == 0)
                {
                    PlayerDataModule.Instance.GetSevenDayReward(1);
                    mask_1.gameObject.SetActive(true);
 
                    PlayerDataModule.Instance.data.GetSevenDayRewardIndex += 1;
                    kuang1.SetActive(false);
                    PlayerDataModule.Instance.data.tongbi += 1000;
                    EventCenter.Instance.TriggerEvent(EventMessages.MakeTongBiTask , 1000);
                    UIController.Instance.Show<RewardConfirmView>(new Dictionary<CurrencyType , int>{{CurrencyType.TongBi,1000}});
                    EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
                }
            }));

            btn_2.onClick.RemoveAllListeners();
            btn_2.onClick.AddListener((() =>
            {
                if (!PlayerDataModule.Instance.data.sevenDayRecordList.Contains(2) &&
                    PlayerDataModule.Instance.data.sevenDayRecordTime !=
                    DateTime.Now.ToString("yyyy/MM/dd") && PlayerDataModule.Instance.data.GetSevenDayRewardIndex == 1)
                {
                    PlayerDataModule.Instance.GetSevenDayReward(2);
                    mask_2.gameObject.SetActive(true);
           
                    PlayerDataModule.Instance.data.GetSevenDayRewardIndex += 1;
                    kuang2.SetActive(false);
                    PlayerDataModule.Instance.data.tongbi += 3000;
                    EventCenter.Instance.TriggerEvent(EventMessages.MakeTongBiTask , 3000);
                     UIController.Instance.Show<RewardConfirmView>(new Dictionary<CurrencyType , int>{{CurrencyType.TongBi,3000}});
                    EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
                }
            }));
            btn_3.onClick.RemoveAllListeners();
            btn_3.onClick.AddListener((() =>
            {
                if (!PlayerDataModule.Instance.data.sevenDayRecordList.Contains(3) &&
                    PlayerDataModule.Instance.data.sevenDayRecordTime !=
                    DateTime.Now.ToString("yyyy/MM/dd") && PlayerDataModule.Instance.data.GetSevenDayRewardIndex == 2)
                {
                    PlayerDataModule.Instance.GetSevenDayReward(3);
                    mask_3.gameObject.SetActive(true);
                
                    PlayerDataModule.Instance.data.GetSevenDayRewardIndex += 1;
                    kuang3.SetActive(false);
                    PlayerDataModule.Instance.data.goldIngot += 300;
                     UIController.Instance.Show<RewardConfirmView>(new Dictionary<CurrencyType , int>{{CurrencyType.JingYuanBao,300}});
                    EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
                }
            }));

            btn_4.onClick.RemoveAllListeners();
            btn_4.onClick.AddListener((() =>
            {
                if (!PlayerDataModule.Instance.data.sevenDayRecordList.Contains(4) &&
                    PlayerDataModule.Instance.data.sevenDayRecordTime !=
                    DateTime.Now.ToString("yyyy/MM/dd")
                    && PlayerDataModule.Instance.data.GetSevenDayRewardIndex == 3)
                {
                    PlayerDataModule.Instance.GetSevenDayReward(4);
                    mask_4.gameObject.SetActive(true);
    
                    kuang4.SetActive(false);
                    PlayerDataModule.Instance.data.tongbi += 4000;
                    EventCenter.Instance.TriggerEvent(EventMessages.MakeTongBiTask , 4000);
                    EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
                     UIController.Instance.Show<RewardConfirmView>(new Dictionary<CurrencyType , int>{{CurrencyType.TongBi,4000}});
                }
            }));

            btn_5.onClick.RemoveAllListeners();
            btn_5.onClick.AddListener((() =>
            {
                if (!PlayerDataModule.Instance.data.sevenDayRecordList.Contains(5) &&
                    PlayerDataModule.Instance.data.sevenDayRecordTime !=
                    DateTime.Now.ToString("yyyy/MM/dd") && PlayerDataModule.Instance.data.GetSevenDayRewardIndex == 4)
                {
                    PlayerDataModule.Instance.GetSevenDayReward(5);
                    mask_5.gameObject.SetActive(true);
 
                    PlayerDataModule.Instance.data.GetSevenDayRewardIndex += 1;
                    kuang5.SetActive(false);
                    PlayerDataModule.Instance.data.tongbi += 5000;
                    EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
                    EventCenter.Instance.TriggerEvent(EventMessages.MakeTongBiTask , 5000);
                     UIController.Instance.Show<RewardConfirmView>(new Dictionary<CurrencyType , int>{{CurrencyType.TongBi,5000}});
                }
            }));

            btn_6.onClick.RemoveAllListeners();
            btn_6.onClick.AddListener((() =>
            {
                if (!PlayerDataModule.Instance.data.sevenDayRecordList.Contains(6) &&
                    PlayerDataModule.Instance.data.sevenDayRecordTime !=
                    DateTime.Now.ToString("yyyy/MM/dd") && PlayerDataModule.Instance.data.GetSevenDayRewardIndex == 5)
                {
                    PlayerDataModule.Instance.GetSevenDayReward(6);
                    mask_6.gameObject.SetActive(true);
                
                    PlayerDataModule.Instance.data.GetSevenDayRewardIndex += 1;
                    kuang6.SetActive(false);
                    PlayerDataModule.Instance.data.goldIngot += 400;
                    EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
                     UIController.Instance.Show<RewardConfirmView>(new Dictionary<CurrencyType , int>{{CurrencyType.JingYuanBao,400}});
      
                }
            }));
            btn_7.onClick.RemoveAllListeners();
            btn_7.onClick.AddListener((() =>
            {
                if (!PlayerDataModule.Instance.data.sevenDayRecordList.Contains(7) &&
                    PlayerDataModule.Instance.data.sevenDayRecordTime !=
                    DateTime.Now.ToString("yyyy/MM/dd") && PlayerDataModule.Instance.data.GetSevenDayRewardIndex == 6)
                {
                    PlayerDataModule.Instance.GetSevenDayReward(7);
                    mask_7.gameObject.SetActive(true);
                  
                    PlayerDataModule.Instance.data.GetSevenDayRewardIndex += 1;
                    PlayerDataModule.Instance.data.tongbi += 5000;
                    PlayerDataModule.Instance.data.goldIngot += 400;
                    PlayerDataModule.Instance.data.lingJing += 200;
                    kuang7.SetActive(false);
                    EventCenter.Instance.TriggerEvent(EventMessages.MakeTongBiTask , 5000);
                    EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
                    UIController.Instance.Show<RewardConfirmView>(new Dictionary<CurrencyType , int>{{CurrencyType.TongBi,1000},{CurrencyType.JingYuanBao,400},{CurrencyType.LingJing,400} ,{CurrencyType.LingJing,200}});
                   
                }
            }));
        }

        public override void UpdateViewWithArgs(params object[] args)
        {
            base.UpdateViewWithArgs(args);
            UpdateMaskState();
            kuang1.SetActive(!PlayerDataModule.Instance.data.sevenDayRecordList.Contains(1) && PlayerDataModule.Instance.data.GetSevenDayRewardIndex == 0 && PlayerDataModule.Instance.data.sevenDayRecordTime != DateTime.Now.ToString("yyyy/MM/dd"));
            kuang2.SetActive(!PlayerDataModule.Instance.data.sevenDayRecordList.Contains(2) && PlayerDataModule.Instance.data.GetSevenDayRewardIndex == 1 && PlayerDataModule.Instance.data.sevenDayRecordTime != DateTime.Now.ToString("yyyy/MM/dd"));
            kuang3.SetActive(!PlayerDataModule.Instance.data.sevenDayRecordList.Contains(3) && PlayerDataModule.Instance.data.GetSevenDayRewardIndex == 2 && PlayerDataModule.Instance.data.sevenDayRecordTime != DateTime.Now.ToString("yyyy/MM/dd"));
            kuang4.SetActive(!PlayerDataModule.Instance.data.sevenDayRecordList.Contains(4) && PlayerDataModule.Instance.data.GetSevenDayRewardIndex == 3 && PlayerDataModule.Instance.data.sevenDayRecordTime != DateTime.Now.ToString("yyyy/MM/dd"));
            kuang5.SetActive(!PlayerDataModule.Instance.data.sevenDayRecordList.Contains(5) && PlayerDataModule.Instance.data.GetSevenDayRewardIndex == 4 && PlayerDataModule.Instance.data.sevenDayRecordTime != DateTime.Now.ToString("yyyy/MM/dd"));
            kuang6.SetActive(!PlayerDataModule.Instance.data.sevenDayRecordList.Contains(6) && PlayerDataModule.Instance.data.GetSevenDayRewardIndex == 5 && PlayerDataModule.Instance.data.sevenDayRecordTime != DateTime.Now.ToString("yyyy/MM/dd"));
            kuang7.SetActive(!PlayerDataModule.Instance.data.sevenDayRecordList.Contains(7) && PlayerDataModule.Instance.data.GetSevenDayRewardIndex == 6 && PlayerDataModule.Instance.data.sevenDayRecordTime != DateTime.Now.ToString("yyyy/MM/dd"));
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
            mask_1.gameObject.SetActive(PlayerDataModule.Instance.data.sevenDayRecordList.Contains(1));
            mask_2.gameObject.SetActive(PlayerDataModule.Instance.data.sevenDayRecordList.Contains(2));
            mask_3.gameObject.SetActive(PlayerDataModule.Instance.data.sevenDayRecordList.Contains(3));
            mask_4.gameObject.SetActive(PlayerDataModule.Instance.data.sevenDayRecordList.Contains(4));
            mask_5.gameObject.SetActive(PlayerDataModule.Instance.data.sevenDayRecordList.Contains(5));
            mask_6.gameObject.SetActive(PlayerDataModule.Instance.data.sevenDayRecordList.Contains(6));
            mask_7.gameObject.SetActive(PlayerDataModule.Instance.data.sevenDayRecordList.Contains(7));

        }
    }
}
