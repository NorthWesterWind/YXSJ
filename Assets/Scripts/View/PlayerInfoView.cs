using System;
using System.Collections.Generic;
using Controller.Player;
using DG.Tweening;
using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using View.CardView;
using View.CharacterInfoView;
using View.EmployeeFunction;
using View.MapFunction;
using View.OrderFunction;
using View.PlayerInfo;

namespace View
{
    public class PlayerInfoView : BaseView
    {
        public PlayerController player;
        public Transform infoItemContent;

        public TextMeshProUGUI accountLevelTxt;
        public TextMeshProUGUI accountLevelProgressTxt;
        public Image fillImage;
        public Transform leftSideContent;

        public UIButton settingBtn;
        public UIButton storeBtn;
        public UIButton sevendayBtn;
        public UIButton zhuanpanBtn;
        public UIButton lingjingBtn;
        public UIButton friendBtn;
        public UIButton mijingBtn;


        public UIButton characterBtn;
        public Image mask1;
        public UIButton cardFunctionBtn;
        public Image mask2;
        public UIButton mapFunctionBtn;
        public Image mask3;
        public UIButton employeeFunctionBtn;
        public Image mask4;
        public UIButton ordenFunctionBtn;
        public Image mask5;

        public TextMeshProUGUI tongbitxt;
        protected override void Start()
        {
            base.Start();
            if (player == null)
            {
                player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
            }
            InitInfoItem();
            HandleUpdateLevelProgress();
            HandleShowPlayerInfoViewCartoon();
            HandleUpdateFunctionState();
            HandleUpdateMoneyInfo();
        }


        protected override void AddEventListener()
        {
            base.AddEventListener();
            EventCenter.Instance.AddListener(EventMessages.UpdatePlayerMoneyInfo, HandleUpdateMoneyInfo);
            EventCenter.Instance.AddListener(EventMessages.ShowPlayerInfoViewCartoon, HandleShowPlayerInfoViewCartoon);
            EventCenter.Instance.AddListener(EventMessages.HidePlayerInfoViewCartoon, HandleHidePlayerInfoViewCartoon);
            EventCenter.Instance.AddListener(EventMessages.UpdateLevelProgress, HandleUpdateLevelProgress);
            EventCenter.Instance.AddListener(EventMessages.UpdateFunctionState, HandleUpdateFunctionState);
            EventCenter.Instance.AddListener(EventMessages.UpdatePlayerCarryInfo, HandleUpdatePlayerCarryInfo);

            settingBtn.onClick.RemoveAllListeners();
            settingBtn.onClick.AddListener(OnClickSettingBtn);
            storeBtn.onClick.RemoveAllListeners();
            storeBtn.onClick.AddListener(OnClickStoreBtn);
            sevendayBtn.onClick.RemoveAllListeners();
            sevendayBtn.onClick.AddListener(OnClickSevendayBtn);
            characterBtn.onClick.RemoveAllListeners();
            characterBtn.onClick.AddListener((() =>
            {
                if (PlayerDataModule.Instance.data.characterFunction != 1)
                {
                    return;
                }
                UIController.Instance.Show<CharacterView>();
                HandleHidePlayerInfoViewCartoon();
            }));
            cardFunctionBtn.onClick.RemoveAllListeners();
            cardFunctionBtn.onClick.AddListener((() =>
            {
                if (PlayerDataModule.Instance.data.cardFunction != 1)
                {
                    return;
                }
                UIController.Instance.Show<CardInfoView>();
                HandleHidePlayerInfoViewCartoon();
            }));
            mapFunctionBtn.onClick.RemoveAllListeners();
            mapFunctionBtn.onClick.AddListener((() =>
            {
                if (PlayerDataModule.Instance.data.mapFunction != 1)
                {
                    return;
                }
                UIController.Instance.Show<MapSelectView>();
                HandleHidePlayerInfoViewCartoon();
            }));
            employeeFunctionBtn.onClick.RemoveAllListeners();
            employeeFunctionBtn.onClick.AddListener((() =>
            {
                if (PlayerDataModule.Instance.data.employeeFunction != 1)
                {
                    return;
                }
                UIController.Instance.Show<EmployeeFunctionView>();
                HandleHidePlayerInfoViewCartoon();
            }));

            zhuanpanBtn.onClick.RemoveAllListeners();
            zhuanpanBtn.onClick.AddListener((() =>
            {

                UIController.Instance.Show<ZhuanPanView>();
                HandleHidePlayerInfoViewCartoon();
            }));
            lingjingBtn.onClick.RemoveAllListeners();
            lingjingBtn.onClick.AddListener((() =>
            {
                if(PlayerDataModule.Instance.data.GetLingJingTime != DateTime.Now.ToString("yyyy/MM/dd"))
                {
                    PlayerDataModule.Instance.data.lingJing += PlayerDataModule.Instance.data.GetLingJingCount;
                    UIController.Instance.Show<RewardConfirmView>(new Dictionary<CurrencyType, int> { { CurrencyType.LingJing, PlayerDataModule.Instance.data.GetLingJingCount } });
                    PlayerDataModule.Instance.data.GetLingJingTime = DateTime.Now.ToString("yyyy/MM/dd");
                    EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
                }
                else
                {
                    UIController.Instance.Show<TipView>("今天已经领取过灵晶！");
                }
                
            }));

            ordenFunctionBtn.onClick.RemoveAllListeners();
            ordenFunctionBtn.onClick.AddListener((() =>
            {
                if (PlayerDataModule.Instance.data.ordenFunction != 1)
                {
                    return;
                }
                UIController.Instance.Show<OrderFunctionView>();
                HandleHidePlayerInfoViewCartoon();
            }));

            friendBtn.onClick.RemoveAllListeners();
            friendBtn.onClick.AddListener((() =>
            {
                UIController.Instance.Show<FriendView>();
                HandleHidePlayerInfoViewCartoon();
            }));    
        }

        public override void RemoveEventListener()
        {
            base.RemoveEventListener();
            EventCenter.Instance.RemoveListener(EventMessages.UpdatePlayerMoneyInfo, HandleUpdateMoneyInfo);
            EventCenter.Instance.RemoveListener(EventMessages.ShowPlayerInfoViewCartoon, HandleShowPlayerInfoViewCartoon);
            EventCenter.Instance.RemoveListener(EventMessages.UpdateLevelProgress, HandleUpdateLevelProgress);
            EventCenter.Instance.RemoveListener(EventMessages.UpdateFunctionState, HandleUpdateFunctionState);
            EventCenter.Instance.RemoveListener(EventMessages.UpdatePlayerCarryInfo, HandleUpdatePlayerCarryInfo);
        }


        public void HandleUpdatePlayerCarryInfo(params object[] args)
        {
            InitInfoItem();
        }


        private void InitInfoItem()
        {
            Extensions.ClearChildren(infoItemContent);
            if (player.dropDic != null && player.dropDic.Count > 0)
            {
                foreach (var value in player.dropDic)
                {
                    if (value.Value == 0)
                        continue;
                    GameObject go = Instantiate(_assetHandle.Get<GameObject>("InfoItem"), infoItemContent, false);
                    go.GetComponent<InfoItem>().SetType(ExchangeType(value.Key));
                    go.GetComponent<InfoItem>().Init(player);
                }
            }

            if (player.goodsDic != null && player.goodsDic.Count > 0)
            {
                foreach (var value in player.goodsDic)
                {
                    if (value.Value == 0)
                        continue;
                    GameObject go = Instantiate(_assetHandle.Get<GameObject>("InfoItem"), infoItemContent, false);
                    go.GetComponent<InfoItem>().SetType(ExchangeType(value.Key));
                    go.GetComponent<InfoItem>().Init(player);
                }
            }

        }

        #region 事件监听函数

        private void OnClickSettingBtn()
        {
            UIController.Instance.Show<SettingView>();
            HandleHidePlayerInfoViewCartoon();
        }

        private void OnClickStoreBtn()
        {
            UIController.Instance.Show<StoreView>();
            HandleHidePlayerInfoViewCartoon();
        }

        private void OnClickSevendayBtn()
        {
            UIController.Instance.Show<SevenDayView>();
            HandleHidePlayerInfoViewCartoon();
        }




        public void HandleUpdateMoneyInfo(params object[] args)
        {
            tongbitxt.text = Extensions.FormatNumber(player.dataModule.data.tongbi);

            
        }

        public void HandleShowPlayerInfoViewCartoon(params object[] args)
        {
            this.leftSideContent.transform.position = new Vector3(-200, this.leftSideContent.transform.position.y, 0);
            this.leftSideContent.transform.DOMove(new Vector3(40, this.leftSideContent.transform.position.y, 0), 0.3f);
        }

        public void HandleHidePlayerInfoViewCartoon(params object[] args)
        {
            this.leftSideContent.transform.position = new Vector3(40, this.leftSideContent.transform.position.y, 0);
            this.leftSideContent.transform.DOMove(new Vector3(-200, this.leftSideContent.transform.position.y, 0), 0.3f);
        }

        public void HandleUpdateLevelProgress(params object[] args)
        {
            accountLevelTxt.text = player.dataModule.data.accountLevel.ToString();
            accountLevelProgressTxt.text = player.dataModule.data.jingMangZhu + "/" + WorldData.LevelRequirementDic[player.dataModule.data.currentMapID];
            fillImage.fillAmount = player.dataModule.data.jingMangZhu * 1f / WorldData.LevelRequirementDic[player.dataModule.data.currentMapID];
        }

        #endregion



        public void HandleUpdateFunctionState(params object[] args)
        {
            mask1.gameObject.SetActive(player.dataModule.data.characterFunction != 1);
            mask2.gameObject.SetActive(player.dataModule.data.cardFunction != 1);
            mask3.gameObject.SetActive(player.dataModule.data.mapFunction != 1);
            mask4.gameObject.SetActive(player.dataModule.data.employeeFunction != 1);
            mask5.gameObject.SetActive(player.dataModule.data.ordenFunction != 1);
        }


        private InfoType ExchangeType(DropItemType type)
        {
            InfoType result = InfoType.None;
            switch (type)
            {
                case DropItemType.ShuangYunZhiFragment:
                    result = InfoType.ShuangYunZhiFragment;
                    break;
                case DropItemType.YueLuCaoFragment:
                    result = InfoType.YueLuCaoFragment;
                    break;
                case DropItemType.ZiXinHuaFragment:
                    result = InfoType.ZiXinHuaFragment;
                    break;
                case DropItemType.YuHuiHeFragment:
                    result = InfoType.YuHuiHeFragment;
                    break;
                case DropItemType.XingWenGuoFragment:
                    result = InfoType.XingWenGuoFragment;
                    break;
                case DropItemType.WuRongJunFragment:
                    result = InfoType.WuRongJunFragment;
                    break;
                case DropItemType.LingXuShengFragment:
                    result = InfoType.LingXuShengFragment;
                    break;
                case DropItemType.XueBanHuaFragment:
                    result = InfoType.XueBanHuaFragment;
                    break;
                case DropItemType.MuLingYaFragment:
                    result = InfoType.MuLingYaFragment;
                    break;
                case DropItemType.JingRuiCaoFragment:
                    result = InfoType.JingRuiCaoFragment;
                    break;
                case DropItemType.TieKuangShiFragment:
                    result = InfoType.TieKuangShiFragment;
                    break;
                case DropItemType.YinKuangShiFragment:
                    result = InfoType.YinKuangShiFragment;
                    break;
                case DropItemType.TongKuangShiFragment:
                    result = InfoType.TongKuangShiFragment;
                    break;
                case DropItemType.ZiJingShiFragment:
                    result = InfoType.ZiJingShiFragment;
                    break;
                case DropItemType.YueJingShiFragment:
                    result = InfoType.YueJingShiFragment;
                    break;
            }
            return result;
        }

        private InfoType ExchangeType(GoodsType type)
        {
            InfoType result = InfoType.None;
            switch (type)
            {
                case GoodsType.YunZhiCha:
                    result = InfoType.YunZhiCha;
                    break;
                case GoodsType.YueLuCha:
                    result = InfoType.YueLuCha;
                    break;
                case GoodsType.ZiXinCha:
                    result = InfoType.ZiXinCha;
                    break;
                case GoodsType.YuHuiCha:
                    result = InfoType.YuHeCha;
                    break;
                case GoodsType.XingWenCha:
                    result = InfoType.XingWenCha;
                    break;
                case GoodsType.WuRongCha:
                    result = InfoType.WuRongCha;
                    break;
                case GoodsType.LingXuCha:
                    result = InfoType.LingXuCha;
                    break;
                case GoodsType.XueBanCha:
                    result = InfoType.XueBanCha;
                    break;
                case GoodsType.MuLingCha:
                    result = InfoType.MuLingCha;
                    break;
                case GoodsType.JingRuiCha:
                    result = InfoType.JingRuiCaoFragment;
                    break;
                case GoodsType.QingYanJian:
                    result = InfoType.QingYanJian;
                    break;
                case GoodsType.YinSiDao:
                    result = InfoType.YinSiDao;
                    break;
                case GoodsType.TongWenDao:
                    result = InfoType.TongWenDao;
                    break;
                case GoodsType.ZiWuJian:
                    result = InfoType.ZiWuJian;
                    break;
                case GoodsType.YueXinJing:
                    result = InfoType.YueXinJing;
                    break;
            }
            return result;
        }
    }
}