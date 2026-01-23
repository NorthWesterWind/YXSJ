using System;
using System.Collections.Generic;
using Controller;
using Module;
using Module.Data;
using UnityEngine;
using Utils;

namespace View
{
    public enum PurchaseType
    {
        Free,
        LingJing,
        Rmb,
    }

    public enum RewardType
    {
        FanPingBaoXia,
        LingYunBaoXia,
        XianYunBaoXia,
        LingShunLing,
        XuanSuLing,
        TianChiLing,
        JingYuanBao,
    }

    public class StoreBtnController : MonoBehaviour
    {
        private UIButton _btn;
        public PurchaseType purchaseType;
        public RewardType rewardType;
        public int rewardValue;
        public int costValue;

        public GameObject mask;
        PlayerData playerData;

        void Start()
        {
            _btn = GetComponent<UIButton>();
            _btn.onClick.AddListener((() => { OnClick(); }));
            playerData = PlayerDataModule.Instance.data;
            if (mask != null)
            {
                if (rewardType == RewardType.FanPingBaoXia)
                {
                    mask?.SetActive(playerData.FanPingBaoXiaoTime == DateTime.Now.ToString("yyyy-MM-dd"));
                }
                else if (rewardType == RewardType.LingShunLing)
                {
                    mask?.SetActive(playerData.LingShunLingTime == DateTime.Now.ToString("yyyy-MM-dd"));
                }
            }
        }

        Action _callback;

        private void OnClick()
        {
            if (purchaseType == PurchaseType.LingJing)
            {
                if (PlayerDataModule.Instance.data.lingJing < costValue)
                {
                    UIController.Instance.Show<TipView>("灵晶数量不足！");
                }
                else
                {
                    _callback = null;

                    switch (rewardType)
                    {
                        case RewardType.XianYunBaoXia:
                            _callback = () =>
                            {
                                UIController.Instance.Show<TipView>("兑换成功！");
                                PlayerDataModule.Instance.data.lingJing -= costValue;
                                PlayerDataModule.Instance.data.goldIngot += DataController.Instance.giftpackDataDic[3].JinYuanBao;
                                var dic = PlayerDataModule.Instance.LotteryCard(DataController.Instance.giftpackDataDic[3]);
                                UIController.Instance.Show<RewardConfirmView>(dic, new Dictionary<CurrencyType, int> { { CurrencyType.JingYuanBao, DataController.Instance.giftpackDataDic[3].JinYuanBao } });
                                EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
                            };
                            UIController.Instance.Show<ExchangeView>($"是否消耗{costValue}灵晶兑换仙韵宝匣？", _callback);
                            break;
                        case RewardType.LingYunBaoXia:
                            _callback = () =>
                            {
                                UIController.Instance.Show<TipView>("兑换成功！");
                                PlayerDataModule.Instance.data.lingJing -= costValue;
                                PlayerDataModule.Instance.data.goldIngot += DataController.Instance.giftpackDataDic[2].JinYuanBao;
                                var dic = PlayerDataModule.Instance.LotteryCard(DataController.Instance.giftpackDataDic[2]);
                                UIController.Instance.Show<RewardConfirmView>(dic, new Dictionary<CurrencyType, int> { { CurrencyType.JingYuanBao, DataController.Instance.giftpackDataDic[2].JinYuanBao } });
                                EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
                            };
                            UIController.Instance.Show<ExchangeView>($"是否消耗{costValue}灵晶兑换灵韵宝匣？", _callback);

                            break;
                        case RewardType.XuanSuLing:
                            _callback = () =>
                            {
                                UIController.Instance.Show<TipView>("兑换成功！");
                                PlayerDataModule.Instance.data.lingJing -= costValue;
                                EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
                            };
                            UIController.Instance.Show<ExchangeView>($"是否消耗{costValue}灵晶兑换玄速令？", _callback);
                            break;
                        case RewardType.TianChiLing:
                            _callback = () =>
                            {
                                UIController.Instance.Show<TipView>("兑换成功！");
                                PlayerDataModule.Instance.data.lingJing -= costValue;
                                EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
                            };
                            UIController.Instance.Show<ExchangeView>($"是否消耗{costValue}灵晶兑换天驰令？", _callback);
                            break;
                        case RewardType.JingYuanBao:
                            _callback = () =>
                            {
                                UIController.Instance.Show<TipView>("兑换成功！");
                                PlayerDataModule.Instance.data.lingJing -= costValue;
                                PlayerDataModule.Instance.data.goldIngot += rewardValue;
                                EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
                            };
                            UIController.Instance.Show<ExchangeView>($"是否消耗{costValue}灵晶兑换{rewardValue}金元宝？",
                                _callback);
                            break;
                    }
                }
            }
            else if (purchaseType == PurchaseType.Rmb)
            {
                _callback = null;
                _callback = () =>
                {
                    if (PlayerDataModule.Instance.AddRecordMoney(costValue))
                    {
                        UIController.Instance.Show<TipView>("兑换成功！");
                        PlayerDataModule.Instance.data.lingJing += rewardValue;
                        EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
                    }
                };
                UIController.Instance.Show<ExchangeView>($"是否消耗{costValue}元兑换{rewardValue}灵晶？", _callback);
            }
            else if (purchaseType == PurchaseType.Free)
            {
                switch (rewardType)
                {
                    case RewardType.LingShunLing:
                        playerData.LingShunLingTime = DateTime.Now.ToString("yyyy-MM-dd");
                        mask?.SetActive(true);
                        UIController.Instance.Show<TipView>("领取成功！");
                        EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
                        break;
                    case RewardType.FanPingBaoXia:
                        playerData.FanPingBaoXiaoTime = DateTime.Now.ToString("yyyy-MM-dd");
                        mask?.SetActive(true);
                        UIController.Instance.Show<TipView>("领取成功！");
                        var dic = PlayerDataModule.Instance.LotteryCard(DataController.Instance.giftpackDataDic[1]);
                        PlayerDataModule.Instance.data.goldIngot += DataController.Instance.giftpackDataDic[1].JinYuanBao;
                        UIController.Instance.Show<RewardConfirmView>(dic, new Dictionary<CurrencyType, int> { { CurrencyType.JingYuanBao, DataController.Instance.giftpackDataDic[1].JinYuanBao } });
                        EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
                        break;
                }
            }
        }

        void Update()
        {
        }


    }
}