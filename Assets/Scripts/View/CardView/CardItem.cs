using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace View.CardView
{
    public class CardItem : MonoBehaviour
    {
        public Image iconBg;
        public Image icon;
        public TextMeshProUGUI leveltxt;
        public TextMeshProUGUI nametxt;
        public TextMeshProUGUI progresstxt;
        public Image fillImg;
        public GameObject fillContent;
        public UIButton btn;
        public CardLevelData data;
        public AssetHandle _assetHandle;
        public Image topLeftLockImage;
        public Image MaskImg;
        public TextMeshProUGUI masktxt;

        void Start()
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener((() =>
            {
                UIController.Instance.Show<CardDetailPop>(data);
            }));
        }

        void OnEnable()
        {
            EventCenter.Instance.AddListener(EventMessages.UpdateCardInfo, HandleUpdateCardInfo);
        }
        void OnDisable()
        {
            EventCenter.Instance.RemoveListener(EventMessages.UpdateCardInfo, HandleUpdateCardInfo);
        }
        void Update()
        {

        }

        public void HandleUpdateCardInfo(params object[] args)
        {
            PlayerData playerData = PlayerDataModule.Instance.data;

            bool own = false;
            CardUpProgress cardUpProgress = null;
            foreach (var value in playerData.cardUpProgressesList)
            {
                if (value.id == data.id)
                {
                    own = true;
                    cardUpProgress = value;
                    break;
                }
            }

            nametxt.text = data.name;
            if (own)
            {
                leveltxt.text = cardUpProgress.level.ToString();
                leveltxt.gameObject.SetActive(true);
                MaskImg.gameObject.SetActive(false);
                topLeftLockImage.gameObject.SetActive(false);
                if (cardUpProgress.level == 10)
                {
                    fillContent.SetActive(true);
                    progresstxt.text = "已满级";
                    fillImg.fillAmount = 1f;
                }
                else
                {
                    fillContent.SetActive(true);
                    fillImg.fillAmount = cardUpProgress.currentNum * 1f / WorldData.cardUpLevelArr[cardUpProgress.level - 1];
                    progresstxt.text = cardUpProgress.currentNum + "/" + WorldData.cardUpLevelArr[cardUpProgress.level - 1];
                }

            }
            else
            {
                topLeftLockImage.gameObject.SetActive(true);
                leveltxt.gameObject.SetActive(false);
                if (data.unlockLevel > playerData.accountLevel)
                {
                    //未到达等级解锁条件
                    fillContent.SetActive(false);
                    progresstxt.gameObject.SetActive(false);
                    MaskImg.gameObject.SetActive(true);
                    masktxt.text = data.unlockLevel.ToString();
                }
                else
                {
                    //到达等级解锁条件，未拥有
                    fillContent.SetActive(false);
                    progresstxt.gameObject.SetActive(false);
                    MaskImg.gameObject.SetActive(false);
                }

            }
        }

        public void Init(CardLevelData _data)
        {

            switch (_data.levelType)
            {
                case CardLevelType.FanPing:

                    iconBg.sprite = _assetHandle.Get<Sprite>("白卡");
                    break;
                case CardLevelType.XianYun:
                    iconBg.sprite = _assetHandle.Get<Sprite>("红卡");
                    break;
                case CardLevelType.LingYun:
                    iconBg.sprite = _assetHandle.Get<Sprite>("紫卡");
                    break;
            }
            data = _data;
            icon.sprite = _assetHandle.Get<Sprite>(data.name);
            PlayerData playerData = PlayerDataModule.Instance.data;

            bool own = false;
            CardUpProgress cardUpProgress = null;
            foreach (var value in playerData.cardUpProgressesList)
            {
                if (value.id == data.id)
                {
                    own = true;
                    cardUpProgress = value;
                    break;
                }
            }

            nametxt.text = data.name;
            if (own)
            {
                leveltxt.text = cardUpProgress.level.ToString();
                leveltxt.gameObject.SetActive(true);
                MaskImg.gameObject.SetActive(false);
                topLeftLockImage.gameObject.SetActive(false);

                switch (cardUpProgress.developType)
                {
                    case CardDevelopType.UpgradeYuShaHu_1:
                    case CardDevelopType.UpgradeYuShaHu_2:
                    case CardDevelopType.UpgradeYuShaHu_3:
                    case CardDevelopType.UpgradeYuShaHu_4:
                    case CardDevelopType.UpgradeLianQiLu_1:
                    case CardDevelopType.UpgradeLianQiLu_2:
                    case CardDevelopType.UpgradeLianQiLu_3:
                        if (cardUpProgress.level == WorldData.cardUpLevelArr.Length + 1)
                        {
                            fillContent.SetActive(true);
                            progresstxt.text = "已满级";
                            fillImg.fillAmount = 1f;
                        }
                        else
                        {
                            fillContent.SetActive(true);
                            fillImg.fillAmount = cardUpProgress.currentNum * 1f / WorldData.cardUpLevelArr[cardUpProgress.level - 1];
                            progresstxt.text = cardUpProgress.currentNum + "/" + WorldData.cardUpLevelArr[cardUpProgress.level - 1];
                        }
                        break;
                    case CardDevelopType.UpgradeLingZhangTai:
                        if (cardUpProgress.level - 1 < WorldData.cardUpLevelArr_LingChouLing.Length)
                        {
                            fillContent.SetActive(true);
                            fillImg.fillAmount = cardUpProgress.currentNum * 1f / WorldData.cardUpLevelArr_LingChouLing[cardUpProgress.level - 1];
                            progresstxt.text = $"{cardUpProgress.currentNum}/{WorldData.cardUpLevelArr_LingChouLing[cardUpProgress.level - 1]}";
                        }
                        else
                        {
                            fillContent.SetActive(false);
                            progresstxt.text = "已满级";
                            fillImg.fillAmount = 1f;
                        }
                        break;
                    case CardDevelopType.UpgradeLingChuGe_1:
                    case CardDevelopType.UpgradeLingChuGe_2:
                    case CardDevelopType.UpgradeYunDiGe:
                        if (cardUpProgress.level - 1 < WorldData.cardUpLevelArr_LingChuGe_YunDiGe.Length)
                        {
                            fillContent.SetActive(true);
                            fillImg.fillAmount = cardUpProgress.currentNum * 1f / WorldData.cardUpLevelArr_LingChuGe_YunDiGe[cardUpProgress.level - 1];
                            progresstxt.text = $"{cardUpProgress.currentNum}/{WorldData.cardUpLevelArr_LingChuGe_YunDiGe[cardUpProgress.level - 1]}";
                        }
                        else
                        {
                            fillContent.SetActive(false);
                            progresstxt.text = "已满级";
                            fillImg.fillAmount = 1f;
                        }
                        break;
                    case CardDevelopType.UpgradeCharacterWithXuanCaiTuAtk:
                    case CardDevelopType.UpgradeCharacterWithXuanCaiTuHp:
                    case CardDevelopType.UpgradeGetYuanBaoLing:
                        if (cardUpProgress.level - 1 < WorldData.cardUpLevelArr_WuQiLing_LingLiLingr_YuanBaoLing.Length)
                        {
                            fillContent.SetActive(true);
                            fillImg.fillAmount = cardUpProgress.currentNum * 1f / WorldData.cardUpLevelArr_WuQiLing_LingLiLingr_YuanBaoLing[cardUpProgress.level - 1];
                            progresstxt.text = $"{cardUpProgress.currentNum}/{WorldData.cardUpLevelArr_WuQiLing_LingLiLingr_YuanBaoLing[cardUpProgress.level - 1]}";
                        }
                        else
                        {
                            fillContent.SetActive(false);
                            progresstxt.text = "已满级";
                            fillImg.fillAmount = 1f;
                        }
                        break;
                }


            }
            else
            {
                topLeftLockImage.gameObject.SetActive(true);
                leveltxt.gameObject.SetActive(false);
                if (data.unlockLevel > playerData.accountLevel)
                {
                    //未到达等级解锁条件
                    fillContent.SetActive(false);
                    progresstxt.gameObject.SetActive(false);
                    MaskImg.gameObject.SetActive(true);
                    masktxt.text = data.unlockLevel.ToString();
                }
                else
                {
                    //到达等级解锁条件，未拥有
                    fillContent.SetActive(false);
                    progresstxt.gameObject.SetActive(false);
                    MaskImg.gameObject.SetActive(false);
                }

            }
        }
    }
}
