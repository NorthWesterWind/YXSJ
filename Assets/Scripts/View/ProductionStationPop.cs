using System.Collections;
using Controller;
using DG.Tweening;
using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using View.CardView;

namespace View
{
    public class ProductionStationPop : BaseView
    {
        public GameObject lockObj;
        public GameObject mask;
        public TextMeshProUGUI masktxt;
        public TextMeshProUGUI cardleveltxt;
        public TextMeshProUGUI cardprogresstxt;
        public TextMeshProUGUI pricetxt;
        public TextMeshProUGUI workingtimetxt;
        public TextMeshProUGUI cardnametxt;
        public TextMeshProUGUI nametxt;
        public TextMeshProUGUI productiontxt;
        public Image cardIcon;
        public Image productionIcon;
        public Image cardprogressfill;
        public GameObject fillContent;
        public TextMeshProUGUI bottomleveltxt1;
        public TextMeshProUGUI bottomleveltxt2;
        public TextMeshProUGUI bottompreviewtxt1;
        public TextMeshProUGUI bottompreviewtxt2;
        public TextMeshProUGUI bottompreviewtxt1_1;
        public TextMeshProUGUI bottompreviewtxt2_2;
        public UIButton bootomBtn1;
        public TextMeshProUGUI bootomBtntxt1;
        public UIButton bootomBtn2;
        public TextMeshProUGUI bootomBtntxt2;
        private BuildingType type;
        private GoodsType goodsType;
        public UIButton cardBtn;
        public RectTransform content;

        public UIButton closeBtn;
        CardUpProgress cardData = null;
        public Image iconBg;
        void OnEnable()
        {
            content.anchoredPosition = new Vector2(0, -1100);
        }

        public override void UpdateViewWithArgs(params object[] args)
        {
            base.UpdateViewWithArgs(args);
            type = (BuildingType)args[0];
            goodsType = (GoodsType)args[1];
            productionIcon.sprite = _assetHandle.Get<Sprite>(Extensions.GetGoodsResNameByType(goodsType));
            cardIcon.sprite = _assetHandle.Get<Sprite>(Extensions.GetStructureResNameByType(type));



            switch (goodsType)
            {
                case GoodsType.YunZhiCha:
                    productiontxt.text = "云芝茶";
                    break;
                case GoodsType.YueLuCha:
                    productiontxt.text = "月露茶";
                    break;
                case GoodsType.ZiXinCha:
                    productiontxt.text = "栀心茶";
                    break;
                case GoodsType.YuHuiCha:
                    productiontxt.text = "玉禾茶";
                    break;
                case GoodsType.XingWenCha:
                    productiontxt.text = "星纹茶";
                    break;
                case GoodsType.WuRongCha:
                    productiontxt.text = "雾茸茶";
                    break;
                case GoodsType.LingXuCha:
                    productiontxt.text = "灵须茶";
                    break;
                case GoodsType.XueBanCha:
                    productiontxt.text = "雪瓣茶";
                    break;
                case GoodsType.MuLingCha:
                    productiontxt.text = "木灵茶";
                    break;
                case GoodsType.JingRuiCha:
                    productiontxt.text = "晶蕊茶";
                    break;
                case GoodsType.QingYanJian:
                    productiontxt.text = "青岩剑";
                    break;
                case GoodsType.YinSiDao:
                    productiontxt.text = "银丝刀";
                    break;
                case GoodsType.TongWenDao:
                    productiontxt.text = "铜纹刀";
                    break;
                case GoodsType.ZiWuJian:
                    productiontxt.text = "紫雾剑";
                    break;
                case GoodsType.YueXinJing:
                    productiontxt.text = "月心镜";
                    break;
                case GoodsType.JingYunBao:
                    productiontxt.text = "金元宝";
                    break;
                case GoodsType.TongBi:
                    productiontxt.text = "铜币";
                    break;
            }

            UpdateInfo();
            content.DOAnchorPos(new Vector2(0, 0), 0.5f).SetEase(Ease.OutBack);
        }

        protected override void OnShowComplete()
        {
            base.OnShowComplete();

        }

        protected override void AddEventListener()
        {
            base.AddEventListener();
            bootomBtn1.onClick.RemoveAllListeners();
            bootomBtn1.onClick.AddListener((() =>
            {
                if (productStationdata.priceLevel == productStationdata.maxPriceLevel)
                {
                    UIController.Instance.Show<TipView>("等级已满。");
                    return;
                }
                if (PlayerDataModule.Instance.data.tongbi < productStationdata.priceLevel * 1000)
                {
                    UIController.Instance.Show<TipView>("铜币不足。");
                    return;
                }
                PlayerDataModule.Instance.data.tongbi -= productStationdata.priceLevel * 1000;
                productStationdata.priceLevel++;
                UpdateStationInfo(productStationdata);
                if (PlayerDataModule.Instance.data.guideStep == GuideStep.UpgradePot)
                {
                    PlayerDataModule.Instance.data.guideStep = GuideStep.Finished;
                    UIController.Instance.Show<PlayerGuide>();
                }
                EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
                EventCenter.Instance.TriggerEvent(EventMessages.UpGradeStuctureTask, productStationdata.buildingType);
            }));

            bootomBtn2.onClick.RemoveAllListeners();
            bootomBtn2.onClick.AddListener((() =>
            {
                if (productStationdata.timelevel == productStationdata.maxTimeLevel)
                {
                    UIController.Instance.Show<TipView>("等级已满。");
                    return;
                }
                if (PlayerDataModule.Instance.data.tongbi < productStationdata.timelevel * 1000)
                {
                    UIController.Instance.Show<TipView>("铜币不足。");
                    return;
                }
                PlayerDataModule.Instance.data.tongbi -= productStationdata.timelevel * 1000;
                productStationdata.timelevel++;
                UpdateStationInfo(productStationdata);
                EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);

            }));

            cardBtn.onClick.RemoveAllListeners();
            cardBtn.onClick.AddListener((() =>
            {
                switch (type)
                {
                    case BuildingType.YuShaHu_1:
                        UIController.Instance.Show<CardDetailPop>(DataController.Instance.cardLevelDataList.Find(x => x.developType == CardDevelopType.UpgradeYuShaHu_1));
                        break;
                    case BuildingType.YuShaHu_2:
                        UIController.Instance.Show<CardDetailPop>(DataController.Instance.cardLevelDataList.Find(x => x.developType == CardDevelopType.UpgradeYuShaHu_2));
                        break;
                    case BuildingType.YuShaHu_3:
                        UIController.Instance.Show<CardDetailPop>(DataController.Instance.cardLevelDataList.Find(x => x.developType == CardDevelopType.UpgradeYuShaHu_3));
                        break;
                    case BuildingType.YuShaHu_4:
                        UIController.Instance.Show<CardDetailPop>(DataController.Instance.cardLevelDataList.Find(x => x.developType == CardDevelopType.UpgradeYuShaHu_4));
                        break;
                    case BuildingType.LianQiLu_1:
                        UIController.Instance.Show<CardDetailPop>(DataController.Instance.cardLevelDataList.Find(x => x.developType == CardDevelopType.UpgradeLianQiLu_1));
                        break;
                    case BuildingType.LianQiLu_2:
                        UIController.Instance.Show<CardDetailPop>(DataController.Instance.cardLevelDataList.Find(x => x.developType == CardDevelopType.UpgradeLianQiLu_2));
                        break;
                    case BuildingType.LianQiLu_3:
                        UIController.Instance.Show<CardDetailPop>(DataController.Instance.cardLevelDataList.Find(x => x.developType == CardDevelopType.UpgradeLianQiLu_3));
                        break;
                }


            }));

            closeBtn.onClick.RemoveAllListeners();
            closeBtn.onClick.AddListener(() => { StartCoroutine(ShowAnimation()); });

            EventCenter.Instance.AddListener(EventMessages.UpdateCardInfo, UpdateViewWithArgs);
        }
        public override void RemoveEventListener()
        {
            base.RemoveEventListener();
               EventCenter.Instance.RemoveListener(EventMessages.UpdateCardInfo, UpdateViewWithArgs);
        }

        private IEnumerator ShowAnimation()
        {
            content.DOAnchorPos(new Vector2(0, -1100), 0.3f)
                .SetEase(Ease.InBack);
            yield return new WaitForSeconds(0.4f);
            Hide();
        }
        ProductStationData productStationdata;

        public void UpdateInfo()
        {
            PlayerData player = PlayerDataModule.Instance.data;
            productStationdata = player.ProductStationDataList.Find(x => x.buildingType == type);
            CardLevelData cardLevelData;
            InitBaseInfo(player, out cardLevelData);
            UpdateStationInfo(productStationdata);
            UpdateCardInfo(cardLevelData);
        }
        private void InitBaseInfo(PlayerData player, out CardLevelData cardLevelData)
        {
            cardLevelData = null;

            switch (type)
            {
                case BuildingType.YuShaHu_1:
                    cardData = player.cardUpProgressesList.Find(x => x.developType == CardDevelopType.UpgradeYuShaHu_1);
                    cardnametxt.text = "一号玉砂壶";
                    nametxt.text = "一号玉砂壶";
                    iconBg.sprite = _assetHandle.Get<Sprite>("白卡");
                    mask.GetComponent<Image>().sprite = _assetHandle.Get<Sprite>("白卡");
                    cardLevelData = DataController.Instance.cardLevelDataList.Find(x => x.developType == CardDevelopType.UpgradeYuShaHu_1);
                    break;

                case BuildingType.YuShaHu_2:
                    cardData = player.cardUpProgressesList.Find(x => x.developType == CardDevelopType.UpgradeYuShaHu_2);
                    cardnametxt.text = "二号玉砂壶";
                    nametxt.text = "二号玉砂壶";
                    iconBg.sprite = _assetHandle.Get<Sprite>("白卡");
                    mask.GetComponent<Image>().sprite = _assetHandle.Get<Sprite>("白卡");
                    cardLevelData = DataController.Instance.cardLevelDataList.Find(x => x.developType == CardDevelopType.UpgradeYuShaHu_2);
                    break;

                case BuildingType.YuShaHu_3:
                    cardData = player.cardUpProgressesList.Find(x => x.developType == CardDevelopType.UpgradeYuShaHu_3);
                    cardnametxt.text = "三号玉砂壶";
                    nametxt.text = "三号玉砂壶";
                    iconBg.sprite = _assetHandle.Get<Sprite>("白卡");
                    mask.GetComponent<Image>().sprite = _assetHandle.Get<Sprite>("白卡");
                    cardLevelData = DataController.Instance.cardLevelDataList.Find(x => x.developType == CardDevelopType.UpgradeYuShaHu_3);
                    break;

                case BuildingType.YuShaHu_4:
                    cardData = player.cardUpProgressesList.Find(x => x.developType == CardDevelopType.UpgradeYuShaHu_4);
                    cardnametxt.text = "四号玉砂壶";
                    nametxt.text = "四号玉砂壶";
                    iconBg.sprite = _assetHandle.Get<Sprite>("白卡");
                    mask.GetComponent<Image>().sprite = _assetHandle.Get<Sprite>("白卡");
                    cardLevelData = DataController.Instance.cardLevelDataList.Find(x => x.developType == CardDevelopType.UpgradeYuShaHu_4);
                    break;

                case BuildingType.LianQiLu_1:
                    cardData = player.cardUpProgressesList.Find(x => x.developType == CardDevelopType.UpgradeLianQiLu_1);
                    cardnametxt.text = "一号炼器炉";
                    nametxt.text = "一号炼器炉";
                    iconBg.sprite = _assetHandle.Get<Sprite>("紫卡");
                    mask.GetComponent<Image>().sprite = _assetHandle.Get<Sprite>("紫卡");
                    cardLevelData = DataController.Instance.cardLevelDataList.Find(x => x.developType == CardDevelopType.UpgradeLianQiLu_1);
                    break;

                case BuildingType.LianQiLu_2:
                    cardData = player.cardUpProgressesList.Find(x => x.developType == CardDevelopType.UpgradeLianQiLu_2);
                    cardnametxt.text = "二号炼器炉";
                    nametxt.text = "二号炼器炉";
                    iconBg.sprite = _assetHandle.Get<Sprite>("紫卡");
                    mask.GetComponent<Image>().sprite = _assetHandle.Get<Sprite>("紫卡");
                    cardLevelData = DataController.Instance.cardLevelDataList.Find(x => x.developType == CardDevelopType.UpgradeLianQiLu_2);
                    break;

                case BuildingType.LianQiLu_3:
                    cardData = player.cardUpProgressesList.Find(x => x.developType == CardDevelopType.UpgradeLianQiLu_3);
                    cardnametxt.text = "三号炼器炉";
                    nametxt.text = "三号炼器炉";
                    iconBg.sprite = _assetHandle.Get<Sprite>("紫卡");
                    mask.GetComponent<Image>().sprite = _assetHandle.Get<Sprite>("紫卡");
                    cardLevelData = DataController.Instance.cardLevelDataList.Find(x => x.developType == CardDevelopType.UpgradeLianQiLu_3);
                    break;
            }
        }


        private void UpdateStationInfo(ProductStationData productStationdata)
        {
            if (cardData == null)
            {
                pricetxt.text =
                    Extensions.FormatNumber((WorldData.goodsPriceDic[goodsType] *
                     DataController.Instance.mapDataDic[PlayerDataModule.Instance.data.currentMapID].price +
                     (productStationdata.priceLevel - 1) * 25));
            }
            else
            {
                pricetxt.text = Extensions.FormatNumber((WorldData.goodsPriceDic[goodsType] *
                     DataController.Instance.mapDataDic[PlayerDataModule.Instance.data.currentMapID].price * cardData.level +
                     (productStationdata.priceLevel - 1) * 25));
            }



            workingtimetxt.text = WorldData.productStationWorkingTimeDic[productStationdata.timelevel] + "秒";
            bottomleveltxt1.text = productStationdata.priceLevel + "级";
            bottomleveltxt2.text = productStationdata.timelevel + "级";

            if (productStationdata.priceLevel < productStationdata.maxPriceLevel)
            {
                if (cardData == null)
                {
                    bottompreviewtxt1.text =
                                   "x" + Extensions.FormatNumber(WorldData.goodsPriceDic[goodsType] *
                                          DataController.Instance.mapDataDic[PlayerDataModule.Instance.data.currentMapID].price +
                                          (productStationdata.priceLevel - 1) * 25);

                    bottompreviewtxt2.text = "x" + Extensions.FormatNumber(WorldData.goodsPriceDic[goodsType] * DataController.Instance.mapDataDic[PlayerDataModule.Instance.data.currentMapID].price + (productStationdata.priceLevel) * 25);

                }
                else
                {
                    bottompreviewtxt1.text =
                              "x" + Extensions.FormatNumber(WorldData.goodsPriceDic[goodsType] *
                                     DataController.Instance.mapDataDic[PlayerDataModule.Instance.data.currentMapID].price* cardData.level +
                                     (productStationdata.priceLevel - 1) * 25);

                    bottompreviewtxt2.text = "x" + Extensions.FormatNumber(WorldData.goodsPriceDic[goodsType] * DataController.Instance.mapDataDic[PlayerDataModule.Instance.data.currentMapID].price* cardData.level + (productStationdata.priceLevel) * 25);

                }

                bootomBtntxt1.text = (1000 * productStationdata.priceLevel).ToString();
            }
            else
            {

                 if (cardData == null)
                {
                           bottompreviewtxt1.text =
              "x" + Extensions.FormatNumber(WorldData.goodsPriceDic[goodsType] *
                     DataController.Instance.mapDataDic[PlayerDataModule.Instance.data.currentMapID].price +
                     productStationdata.priceLevel * 25);
                }
                else
                {
                      bottompreviewtxt1.text =
              "x" + Extensions.FormatNumber(WorldData.goodsPriceDic[goodsType] *
                     DataController.Instance.mapDataDic[PlayerDataModule.Instance.data.currentMapID].price*cardData.level +
                     productStationdata.priceLevel * 25);
                }
             
                bottompreviewtxt2.text = "";
                bootomBtntxt1.text = "已满级";
            }

            if (productStationdata.timelevel < productStationdata.maxTimeLevel)
            {
                bottompreviewtxt1_1.text =
                    WorldData.productStationWorkingTimeDic[productStationdata.timelevel] + "秒  ";
                bottompreviewtxt2_2.text = $"{WorldData.productStationWorkingTimeDic[productStationdata.timelevel + 1]}秒 ";
                bootomBtntxt2.text = (2000 * productStationdata.timelevel).ToString();
            }
            else
            {
                bottompreviewtxt2.text =
                    WorldData.productStationWorkingTimeDic[productStationdata.timelevel] + "秒";
                bottompreviewtxt2_2.text = "";
                bootomBtntxt2.text = "已满级";
            }
        }

        private void UpdateCardInfo(CardLevelData cardLevelData)
        {
            if (cardData == null)
            {
                fillContent.SetActive(false);
                cardleveltxt.text = "0";
                lockObj.SetActive(true);
                if (PlayerDataModule.Instance.data.accountLevel < cardLevelData.unlockLevel)
                {
                    mask.SetActive(true);
                    masktxt.text = cardLevelData.unlockLevel.ToString();
                }
                else
                {
                    mask.SetActive(false);
                }
            }
            else
            {
                lockObj.SetActive(false);
                mask.SetActive(false);
                cardleveltxt.text = cardData.level.ToString();

                if (cardData.level == WorldData.cardUpLevelArr.Length +1)
                {
                    cardprogressfill.fillAmount = 1;
                    cardprogresstxt.text = "已满级";
                }
                else
                {
                    cardprogressfill.fillAmount =
                        cardData.currentNum * 1f / WorldData.cardUpLevelArr[cardData.level - 1];
                    cardprogresstxt.text =
                        cardData.currentNum + "/" + WorldData.cardUpLevelArr[cardData.level - 1];
                }
            }
        }
    }
}