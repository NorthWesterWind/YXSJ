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
        public TextMeshProUGUI bottomleveltxt1;
        public TextMeshProUGUI bottomleveltxt2;
        public TextMeshProUGUI bottompreviewtxt1;
        public TextMeshProUGUI bottompreviewtxt2;
        public Image bottomprogressfill;

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
                case GoodsType.YuHeCha:
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
            bootomBtn1.onClick.AddListener((() => { }));

            bootomBtn2.onClick.RemoveAllListeners();
            bootomBtn2.onClick.AddListener((() => { }));

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
        }

        private IEnumerator ShowAnimation()
        {
            content.DOAnchorPos(new Vector2(0, -1100), 0.3f)
                .SetEase(Ease.InBack);
            yield return new WaitForSeconds(0.4f);
            Hide();
        }

        public void UpdateInfo()
        {
            PlayerData player = PlayerDataModule.Instance.data;
            ProductStationData productStationdata = player.ProductStationDataList.Find(x => x.buildingType == type);

            CardLevelData cardLevelData = DataController.Instance.cardLevelDataList.Find(x => x.developType == CardDevelopType.UpgradeYuShaHu_1); ;
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
                    cardData = player.cardUpProgressesList.Find(x =>
                        x.developType == CardDevelopType.UpgradeLianQiLu_1);
                    cardnametxt.text = "一号炼器炉";
                    nametxt.text = "一号炼器炉";
                    iconBg.sprite = _assetHandle.Get<Sprite>("紫卡");
                    mask.GetComponent<Image>().sprite = _assetHandle.Get<Sprite>("紫卡");
                    cardLevelData = DataController.Instance.cardLevelDataList.Find(x => x.developType == CardDevelopType.UpgradeLianQiLu_1);
                    break;
                case BuildingType.LianQiLu_2:
                    cardData = player.cardUpProgressesList.Find(x =>
                        x.developType == CardDevelopType.UpgradeLianQiLu_2);
                    cardnametxt.text = "二号炼器炉";
                    nametxt.text = "二号炼器炉";
                    iconBg.sprite = _assetHandle.Get<Sprite>("紫卡");
                    mask.GetComponent<Image>().sprite = _assetHandle.Get<Sprite>("紫卡");
                    cardLevelData = DataController.Instance.cardLevelDataList.Find(x => x.developType == CardDevelopType.UpgradeLianQiLu_2);
                    break;
                case BuildingType.LianQiLu_3:
                    cardData = player.cardUpProgressesList.Find(x =>
                        x.developType == CardDevelopType.UpgradeLianQiLu_3);
                    cardnametxt.text = "三号炼器炉";
                    nametxt.text = "三号炼器炉";
                    iconBg.sprite = _assetHandle.Get<Sprite>("紫卡");
                    mask.GetComponent<Image>().sprite = _assetHandle.Get<Sprite>("紫卡");
                    cardLevelData = DataController.Instance.cardLevelDataList.Find(x => x.developType == CardDevelopType.UpgradeLianQiLu_3);
                    break;
            }

            if (productStationdata == null)
            {
                if (cardData == null)
                {
                    pricetxt.text = WorldData.goodsPriceDic[goodsType] * DataController.Instance
                        .mapDataDic[PlayerDataModule.Instance.data.currentMapID].price * 3 + "";
                    lockObj.SetActive(true);
                }
                else
                {
                    pricetxt.text = WorldData.goodsPriceDic[goodsType] * DataController.Instance
                            .mapDataDic[PlayerDataModule.Instance.data.currentMapID].price *
                        Mathf.Pow(3, cardData.level) + "";
                    lockObj.SetActive(false);
                }

                workingtimetxt.text = "5";
                bottomleveltxt1.text = "1级";
                bottomleveltxt2.text = "1级";
                bottompreviewtxt1.text = "x3   <color=green>X9</color>";
                bottompreviewtxt2.text = "5秒   <color=green>4秒</color>";
                bottomprogressfill.fillAmount = 0f;

                bootomBtntxt1.text = "1000";
                bootomBtntxt2.text = "1000";
            }
            else
            {
                if (cardData == null)
                {
                    pricetxt.text =
                        (WorldData.goodsPriceDic[goodsType] * DataController.Instance
                             .mapDataDic[PlayerDataModule.Instance.data.currentMapID].price +
                         (productStationdata.priceLevel - 1) * 25).ToString();
                }
                else
                {
                    pricetxt.text =
                        (WorldData.goodsPriceDic[goodsType] * DataController.Instance
                             .mapDataDic[PlayerDataModule.Instance.data.currentMapID].price *
                         Mathf.Pow(3, cardData.level) +
                         (productStationdata.priceLevel - 1) * 25).ToString();
                }

                bootomBtntxt1.text = (1000 * productStationdata.priceLevel).ToString();
                bootomBtntxt2.text = (1000 * productStationdata.timelevel).ToString();

                workingtimetxt.text = WorldData.productStationWorkingTimeDic[productStationdata.priceLevel] + "秒";
                bottomleveltxt1.text = productStationdata.priceLevel + "级";
                bottomleveltxt2.text = productStationdata.timelevel + "级";
                bottompreviewtxt1.text = "x" + (WorldData.goodsPriceDic[goodsType] * DataController.Instance
                                                    .mapDataDic[
                                                        PlayerDataModule.Instance.data
                                                            .currentMapID].price +
                                                (productStationdata.priceLevel - 1) * 25) + "  " +
                                         $" <color=green>{(WorldData.goodsPriceDic[goodsType] * DataController.Instance.mapDataDic[PlayerDataModule.Instance.data.currentMapID].price + (productStationdata.priceLevel) * 25)}</color>";
                if (productStationdata.timelevel < 10)
                {
                    bottompreviewtxt2.text = WorldData.productStationWorkingTimeDic[productStationdata.timelevel] +
                                             "秒  " +
                                             $"  <color=green>{WorldData.productStationWorkingTimeDic[productStationdata.timelevel + 1]}</color>";
                }
                else
                {
                    bottompreviewtxt2.text = WorldData.productStationWorkingTimeDic[productStationdata.timelevel] + "秒";
                }

                float tempvalue = productStationdata.priceLevel % 10 / 10;
                bottomprogressfill.fillAmount = tempvalue;

            }


            if (cardData == null)
            {
                cardleveltxt.text = "0";
                cardprogressfill.fillAmount = 0;
                cardprogresstxt.text = "0";
                lockObj.SetActive(true);
                if (PlayerDataModule.Instance.data.accountLevel < cardLevelData.unlockLevel)
                {
                    mask.SetActive(true);
                    masktxt.text = cardLevelData.unlockLevel + "";
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
                if (cardData.level == 10)
                {
                    cardprogressfill.fillAmount = 1;
                    cardprogresstxt.text = "已满级";
                }
                else
                {
                    cardprogressfill.fillAmount =
                        cardData.currentNum * 1f / WorldData.cardUpLevelArr[cardData.level + 1];
                    cardprogresstxt.text = cardData.currentNum + "/" + WorldData.cardUpLevelArr[cardData.level + 1];

                }
            }
        }
    }
}