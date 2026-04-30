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

        private void Start()
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                UIController.Instance.Show<CardDetailPop>(data);
            });
        }

        private void OnEnable()
        {
            EventCenter.Instance.AddListener(EventMessages.UpdateCardInfo, HandleUpdateCardInfo);
        }

        private void OnDisable()
        {
            EventCenter.Instance.RemoveListener(EventMessages.UpdateCardInfo, HandleUpdateCardInfo);
        }

        public void HandleUpdateCardInfo(params object[] args)
        {
            if (data == null)
            {
                return;
            }

            if (args.Length > 0 && args[0] is CardDevelopType changedType && changedType != data.developType)
            {
                return;
            }

            RefreshView();
        }

        public void Init(CardLevelData cardData)
        {
            data = cardData;
            switch (data.levelType)
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

            icon.sprite = _assetHandle.Get<Sprite>(data.name);
            RefreshView();
        }

        private void RefreshView()
        {
            if (data == null)
            {
                return;
            }

            var playerData = PlayerDataModule.Instance.data;
            nametxt.text = data.name;

            if (playerData.accountLevel < data.unlockLevel)
            {
                RenderLocked();
                return;
            }

            var cardUpProgress = GetCardProgress(playerData);

            if (cardUpProgress == null)
            {
                RenderLockedOrUnowned(playerData);
                return;
            }

            RenderOwned(cardUpProgress);
        }

        private CardUpProgress GetCardProgress(PlayerData playerData)
        {
            foreach (var value in playerData.cardUpProgressesList)
            {
                if (value.id == data.id)
                {
                    return value;
                }
            }

            return null;
        }

        private void RenderOwned(CardUpProgress cardUpProgress)
        {
            leveltxt.text = cardUpProgress.level.ToString();
            leveltxt.gameObject.SetActive(true);
            MaskImg.gameObject.SetActive(false);
            topLeftLockImage.gameObject.SetActive(false);
            progresstxt.gameObject.SetActive(true);

            switch (data.developType)
            {
                case CardDevelopType.UpgradeYuShaHu_1:
                case CardDevelopType.UpgradeYuShaHu_2:
                case CardDevelopType.UpgradeYuShaHu_3:
                case CardDevelopType.UpgradeYuShaHu_4:
                case CardDevelopType.UpgradeLianQiLu_1:
                case CardDevelopType.UpgradeLianQiLu_2:
                case CardDevelopType.UpgradeLianQiLu_3:
                    RenderProgress(cardUpProgress, WorldData.cardUpLevelArr, hideFillOnMax: false);
                    break;
                case CardDevelopType.UpgradeLingZhangTai:
                    RenderProgress(cardUpProgress, WorldData.cardUpLevelArr_LingChouLing, hideFillOnMax: true);
                    break;
                case CardDevelopType.UpgradeLingChuGe_1:
                case CardDevelopType.UpgradeLingChuGe_2:
                case CardDevelopType.UpgradeYunDiGe:
                    RenderProgress(cardUpProgress, WorldData.cardUpLevelArr_LingChuGe_YunDiGe, hideFillOnMax: true);
                    break;
                case CardDevelopType.UpgradeCharacterWithXuanCaiTuAtk:
                case CardDevelopType.UpgradeCharacterWithXuanCaiTuHp:
                case CardDevelopType.UpgradeGetYuanBaoLing:
                    RenderProgress(cardUpProgress, WorldData.cardUpLevelArr_WuQiLing_LingLiLingr_YuanBaoLing, hideFillOnMax: true);
                    break;
            }
        }

        private void RenderProgress(CardUpProgress cardUpProgress, int[] progressArr, bool hideFillOnMax)
        {
            int progressIndex = cardUpProgress.level - 1;
            bool isMaxLevel = progressIndex >= progressArr.Length;

            if (isMaxLevel)
            {
                fillContent.SetActive(true);
                progresstxt.text = "已满级";
                fillImg.fillAmount = 1f;
                return;
            }

            fillContent.SetActive(true);
            fillImg.fillAmount = cardUpProgress.currentNum * 1f / progressArr[progressIndex];
            progresstxt.text = $"{cardUpProgress.currentNum}/{progressArr[progressIndex]}";
        }

        private void RenderLockedOrUnowned(PlayerData playerData)
        {
            topLeftLockImage.gameObject.SetActive(true);
            leveltxt.gameObject.SetActive(false);

            if (data.unlockLevel > playerData.accountLevel)
            {
                fillContent.SetActive(false);
                progresstxt.gameObject.SetActive(false);
                MaskImg.gameObject.SetActive(true);
                masktxt.text = data.unlockLevel.ToString();
            }
            else
            {
                fillContent.SetActive(false);
                progresstxt.gameObject.SetActive(false);
                MaskImg.gameObject.SetActive(false);
            }
        }

        private void RenderLocked()
        {
            topLeftLockImage.gameObject.SetActive(true);
            leveltxt.gameObject.SetActive(false);
            progresstxt.gameObject.SetActive(false);
            fillContent.SetActive(false);
            MaskImg.gameObject.SetActive(true);
            masktxt.text = data.unlockLevel.ToString();
        }
    }
}
