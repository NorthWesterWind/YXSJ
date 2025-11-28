using DG.Tweening;
using Module.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using View.PlayerInfo;
using World.View.UI;
using CharacterController = Controller.Player.CharacterController;

namespace View
{
    public class PlayerInfoView : BaseView
    {
        public CharacterController character;
        public Transform infoItemContent;

        public TextMeshProUGUI accountLevelTxt;
        public TextMeshProUGUI accountLevelProgressTxt;
        public Image fillImage;
        public Transform leftSideContent;

        public UIButton settingBtn;
        public UIButton storeBtn;
        public UIButton sevendayBtn;
        public UIButton vipBtn;
        
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
        protected override void Start()
        {
            base.Start();
            InitInfoItem();
            HandleUpdateLevelProgress();
            HandleShowPlayerInfoViewCartoon();
            HandleUpdateFunctionState();
        }


        protected override void AddEventListener()
        {
            base.AddEventListener();
            EventCenter.Instance.AddListener(EventMessages.UpdatePlayerMoneyInfo, HandleUpdateMoneyInfo);
            EventCenter.Instance.AddListener(EventMessages.ShowPlayerInfoViewCartoon, HandleShowPlayerInfoViewCartoon);
            EventCenter.Instance.AddListener(EventMessages.UpdateLevelProgress, HandleUpdateLevelProgress);
            EventCenter.Instance.AddListener(EventMessages.UpdateFunctionState, HandleUpdateFunctionState);
        }

        public override void RemoveEventListener()
        {
            base.RemoveEventListener();
            EventCenter.Instance.RemoveListener(EventMessages.UpdatePlayerMoneyInfo, HandleUpdateMoneyInfo);
            EventCenter.Instance.RemoveListener(EventMessages.ShowPlayerInfoViewCartoon,
                HandleShowPlayerInfoViewCartoon);
            EventCenter.Instance.RemoveListener(EventMessages.UpdateLevelProgress, HandleUpdateLevelProgress);
            EventCenter.Instance.RemoveListener(EventMessages.UpdateFunctionState, HandleUpdateFunctionState);
        }

        private void InitInfoItem()
        {
            Extensions.ClearChildren(infoItemContent);
            GameObject go1 = Instantiate(_assetHandle.Get<GameObject>("InfoItem"), transform, false);
            go1.GetComponent<InfoItem>().SetType(InfoType.JinYuanBao);
            go1.GetComponent<InfoItem>().Init(character);
            GameObject go2 = Instantiate(_assetHandle.Get<GameObject>("InfoItem"), transform, false);
            go2.GetComponent<InfoItem>().SetType(InfoType.LingJing);
            go2.GetComponent<InfoItem>().Init(character);
            GameObject go3 = Instantiate(_assetHandle.Get<GameObject>("InfoItem"), transform, false);
            go3.GetComponent<InfoItem>().SetType(InfoType.YinQian);
            go3.GetComponent<InfoItem>().Init(character);
            foreach (var value in character.dropDic)
            {
                GameObject go = Instantiate(_assetHandle.Get<GameObject>("InfoItem"), transform, false);
                go.GetComponent<InfoItem>().SetType(ExchangeType(value.Key));
                go.GetComponent<InfoItem>().Init(character);
            }
            foreach (var value in character.goodsDic)
            {
                GameObject go = Instantiate(_assetHandle.Get<GameObject>("InfoItem"), transform, false);
                go.GetComponent<InfoItem>().SetType(ExchangeType(value.Key));
                go.GetComponent<InfoItem>().Init(character);
            }
        }

        #region 事件监听函数

        public void HandleUpdateMoneyInfo(params object[] args)
        {
            
        }

        public void HandleShowPlayerInfoViewCartoon(params object[] args)
        {
            leftSideContent.transform.DOLocalMoveX(40,1.5f);
        }

        public void HandleUpdateLevelProgress(params object[] args)
        {
            accountLevelTxt.text = character.dataModule.data.accountLevel.ToString();
            accountLevelProgressTxt.text = character.dataModule.data.jingMangZhu + "/" +  WorldData.LevelRequirementDic[ character.dataModule.data.currentMapID];
            fillImage.fillAmount = character.dataModule.data.jingMangZhu * 1f / WorldData.LevelRequirementDic[character.dataModule.data.currentMapID];
        }

        #endregion



        public void HandleUpdateFunctionState(params object[] args)
        {
            mask1.gameObject.SetActive(character.dataModule.data.characterFunction != 1);
            mask2.gameObject.SetActive(character.dataModule.data.cardFunction != 1);
            mask3.gameObject.SetActive(character.dataModule.data.mapFunction != 1);
            mask4.gameObject.SetActive(character.dataModule.data.employeeFunction != 1);
            mask5.gameObject.SetActive(character.dataModule.data.ordenFunction != 1);
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
                case GoodsType.YuHeCha:
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