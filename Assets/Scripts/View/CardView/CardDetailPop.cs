using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using System.Collections;

namespace View.CardView
{
    public class CardDetailPop : BaseView
    {
        private const string FullLevelText = "已满级";
        private const string CurrentCardMaxLevelTip = "当前卡片已满级！";
        private const string CurrentCardNotEnoughTip = "当前卡片数量不足。";
        private const string GoldIngotNotEnoughTip = "金元宝数量不足！";
        private const string UpgradeSuccessTip = "升级成功。";
        private const string CardLockedTip = "当前卡片仍处于锁定状态。";

        public UIButton closeBtn;
        public TextMeshProUGUI title1txt;
        public TextMeshProUGUI contenttxt;
        public TextMeshProUGUI currenttxt;
        public TextMeshProUGUI nexttxt;
        public TextMeshProUGUI infotxt;
        public Image cardImg;
        public TextMeshProUGUI levelTxt;
        public Image fillImg;
        public GameObject fillContent;
        public TextMeshProUGUI filltxt;
        public CardLevelData cardLevelData;
        public UIButton upgradeBtn;
        public TextMeshProUGUI cardprogresstxt;
        public TextMeshProUGUI goldneedtxt;
        public Image mask;
        public Image topleftLock;
        public TextMeshProUGUI masktxt;
        public AssetHandle assetHandle;

        public int currentNeedCard;
        public int currentNeedGold;
        CardUpProgress cardUpProgress = null;
        private Coroutine _layoutRefreshCoroutine;

        public Image icon;

        public override void UpdateViewWithArgs(params object[] args)
        {
            cardUpProgress = null;
            base.UpdateViewWithArgs(args);
            cardLevelData = args[0] as CardLevelData;
            if (cardLevelData == null)
            {
                return;
            }

            icon.sprite = assetHandle.Get<Sprite>(cardLevelData.name);
            title1txt.text = cardLevelData.name;
            contenttxt.text = GetContentLabel(cardLevelData.developType);
            infotxt.text = "　　" + cardLevelData.description;

            PlayerData playerData = PlayerDataModule.Instance.data;
            cardUpProgress = playerData.cardUpProgressesList.Find(x => x.id == cardLevelData.id);

            RefreshOwnedState(playerData);
            RefreshUpgradeCost();
            RefreshProgressInfo();
            RefreshLayout();
        }

        private void RefreshOwnedState(PlayerData playerData)
        {
            if (IsLockedByAccountLevel(playerData))
            {
                RefreshLockedState();
                return;
            }

            if (cardUpProgress != null)
            {
                bool isMaxLevel = IsCardMaxLevel();
                nexttxt.gameObject.SetActive(!isMaxLevel);
                currenttxt.text = GetCurrentValueText(cardLevelData.developType, cardUpProgress.level);
                nexttxt.text = isMaxLevel ? string.Empty : GetNextValueText(cardLevelData.developType, cardUpProgress.level);
                levelTxt.text = cardUpProgress.level.ToString();
                levelTxt.gameObject.SetActive(true);
                mask.gameObject.SetActive(false);
                topleftLock.gameObject.SetActive(false);
                filltxt.gameObject.SetActive(true);
                upgradeBtn.gameObject.SetActive(!isMaxLevel);
                return;
            }

            upgradeBtn.gameObject.SetActive(false);
            nexttxt.gameObject.SetActive(false);
            levelTxt.gameObject.SetActive(false);
            currenttxt.text = GetDefaultValueText(cardLevelData.developType);
            cardprogresstxt.text = "0/" + GetProgressArray(cardLevelData.developType)[0];

            if (cardLevelData.unlockLevel > playerData.accountLevel)
            {
                fillContent.SetActive(false);
                filltxt.gameObject.SetActive(false);
                mask.gameObject.SetActive(true);
                masktxt.text = cardLevelData.unlockLevel.ToString();
            }
            else
            {
                fillContent.SetActive(false);
                filltxt.gameObject.SetActive(true);
                mask.gameObject.SetActive(false);
            }

            topleftLock.gameObject.SetActive(true);
        }

        private void RefreshLockedState()
        {
            upgradeBtn.gameObject.SetActive(false);
            topleftLock.gameObject.SetActive(true);
            mask.gameObject.SetActive(true);
            masktxt.text = cardLevelData.unlockLevel.ToString();
            levelTxt.gameObject.SetActive(false);
            nexttxt.gameObject.SetActive(false);
            fillContent.SetActive(false);
            filltxt.gameObject.SetActive(false);

            if (cardUpProgress != null)
            {
                currenttxt.text = GetCurrentValueText(cardLevelData.developType, cardUpProgress.level);
                cardprogresstxt.text = $"{cardUpProgress.currentNum}/{GetLockedProgressTarget()}";
            }
            else
            {
                currenttxt.text = GetDefaultValueText(cardLevelData.developType);
                cardprogresstxt.text = "0/" + GetLockedProgressTarget();
            }
        }

        private void RefreshUpgradeCost()
        {
            int tempvalue = 0;
            if (cardUpProgress != null)
            {
                tempvalue = (cardUpProgress.level + 1) > 10 ? 10 : cardUpProgress.level + 1;
            }

            switch (cardLevelData.levelType)
            {
                case CardLevelType.FanPing:
                    currentNeedGold = WorldData.cardUpgradeCostArr_FanPin[tempvalue];
                    cardImg.sprite = assetHandle.Get<Sprite>("白卡");
                    mask.sprite = assetHandle.Get<Sprite>("白卡");
                    break;
                case CardLevelType.LingYun:
                    currentNeedGold = WorldData.cardUpgradeCostArr_LingYun[tempvalue];
                    cardImg.sprite = assetHandle.Get<Sprite>("紫卡");
                    mask.sprite = assetHandle.Get<Sprite>("紫卡");
                    break;
                case CardLevelType.XianYun:
                    if(tempvalue < WorldData.cardUpgradeCostArr_XianYun.Length)
                    {
                        currentNeedGold = WorldData.cardUpgradeCostArr_XianYun[tempvalue];
                    cardImg.sprite = assetHandle.Get<Sprite>("红卡");
                    mask.sprite = assetHandle.Get<Sprite>("红卡"); 
                    }
                   
                    break;
            }

            goldneedtxt.text = currentNeedGold.ToString();
        }

        private void RefreshProgressInfo()
        {
            int[] progressArray = GetProgressArray(cardLevelData.developType);
            if (cardUpProgress == null)
            {
                currentNeedCard = progressArray[0];
                fillImg.fillAmount = 0f;
                return;
            }
            int progressIndex = cardUpProgress.level - 1;
            upgradeBtn.gameObject.SetActive(progressIndex < progressArray.Length);

            if (progressIndex >= progressArray.Length)
            {
                fillContent.SetActive(true);
                filltxt.gameObject.SetActive(true);
                upgradeBtn.gameObject.SetActive(false);
                filltxt.text = FullLevelText;
                fillImg.fillAmount = 1f;
                cardprogresstxt.text = FullLevelText;
                goldneedtxt.text = FullLevelText;
                currentNeedCard = 0;

                return;
            }

            currentNeedCard = progressArray[progressIndex];
            fillContent.SetActive(true);
            fillImg.fillAmount = cardUpProgress.currentNum * 1f / currentNeedCard;
            filltxt.text = $"{cardUpProgress.currentNum}/{currentNeedCard}";
            cardprogresstxt.text = filltxt.text;
        }

        private string GetContentLabel(CardDevelopType developType)
        {
            switch (developType)
            {
                case CardDevelopType.UpgradeCharacterWithXuanCaiTuAtk:
                    return "攻击力";
                case CardDevelopType.UpgradeCharacterWithXuanCaiTuHp:
                    return "健康值";
                case CardDevelopType.UpgradeGetYuanBaoLing:
                    return "金元宝产出数";
                case CardDevelopType.UpgradeLingZhangTai:
                    return "打赏";
                case CardDevelopType.UpgradeLingChuGe_1:
                case CardDevelopType.UpgradeLingChuGe_2:
                    return "储物容量";
                case CardDevelopType.UpgradeYunDiGe:
                    return "云递者容纳量";
                default:
                    return "收益";
            }
        }

        private string GetCurrentValueText(CardDevelopType developType, int level)
        {
            switch (developType)
            {
                case CardDevelopType.UpgradeCharacterWithXuanCaiTuAtk:
                    return $"+ {level * 0.3f * 100f}%";
                case CardDevelopType.UpgradeCharacterWithXuanCaiTuHp:
                    return $"+ {level * 30}";
                case CardDevelopType.UpgradeGetYuanBaoLing:
                    return $"+ {level * 10}";
                case CardDevelopType.UpgradeLingZhangTai:
                    return $"+ {level * 0.2f * 100f}%";
                case CardDevelopType.UpgradeLingChuGe_1:
                case CardDevelopType.UpgradeLingChuGe_2:
                    return $"+ {level * 10f}";
                case CardDevelopType.UpgradeYunDiGe:
                    return $"+ {level}";

                default:
                    return $"x {level}";
            }
        }

        private string GetNextValueText(CardDevelopType developType, int level)
        {
            switch (developType)
            {
                case CardDevelopType.UpgradeCharacterWithXuanCaiTuAtk:
                    return $"+{(level + 1) * 0.3f * 100f}%";
                case CardDevelopType.UpgradeCharacterWithXuanCaiTuHp:
                    return $"+{(level + 1) * 30}";
                case CardDevelopType.UpgradeGetYuanBaoLing:
                    return $"+{(level + 1) * 10}";
                case CardDevelopType.UpgradeLingZhangTai:
                    return $"+{(level + 1) * 0.2f * 100f}%";
                case CardDevelopType.UpgradeLingChuGe_1:
                case CardDevelopType.UpgradeLingChuGe_2:
                    return $"+{(level + 1) * 10}";
                case CardDevelopType.UpgradeYunDiGe:
                    return $"+{level + 1}";
                default:
                    return $"x{level + 1}";
            }
        }

        private string GetDefaultValueText(CardDevelopType developType)
        {
            switch (developType)
            {
                case CardDevelopType.UpgradeCharacterWithXuanCaiTuAtk:
                    return $"+ {0.5f * 100f}%";
                case CardDevelopType.UpgradeCharacterWithXuanCaiTuHp:
                    return "+30";
                case CardDevelopType.UpgradeGetYuanBaoLing:
                    return "+10";
                case CardDevelopType.UpgradeLingZhangTai:
                    return $"+ {0.2f * 100f}%";
                case CardDevelopType.UpgradeLingChuGe_1:
                case CardDevelopType.UpgradeLingChuGe_2:
                    return "+10";
                case CardDevelopType.UpgradeYunDiGe:
                    return "x1";
                default:
                    return "x1";
            }
        }

        protected override void AddEventListener()
        {
            base.AddEventListener();
            closeBtn.onClick.RemoveAllListeners();
            closeBtn.onClick.AddListener(() =>
            {
                Hide();
            });

            upgradeBtn.onClick.RemoveAllListeners();
            upgradeBtn.onClick.AddListener(() =>
            {
                if (IsLockedByAccountLevel(PlayerDataModule.Instance.data))
                {
                    UIController.Instance.Show<TipView>(CardLockedTip);
                    return;
                }

                if (cardUpProgress == null || IsCardMaxLevel())
                {
                    UIController.Instance.Show<TipView>(CurrentCardMaxLevelTip);
                    return;
                }

                if (cardUpProgress.currentNum < currentNeedCard)
                {
                    UIController.Instance.Show<TipView>(CurrentCardNotEnoughTip);
                    return;
                }

                if (PlayerDataModule.Instance.data.goldIngot < currentNeedGold)
                {
                    UIController.Instance.Show<TipView>(GoldIngotNotEnoughTip);
                    return;
                }

                cardUpProgress.currentNum -= currentNeedCard;
                PlayerDataModule.Instance.data.goldIngot -= currentNeedGold;
                cardUpProgress.level += 1;

                UIController.Instance.Show<TipView>(UpgradeSuccessTip);
                UpdateViewWithArgs(cardLevelData);
                EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
                EventCenter.Instance.TriggerEvent(EventMessages.UpdateCardInfo, cardLevelData.developType);
            });
        }

        private void RefreshLayout()
        {
            if (_layoutRefreshCoroutine != null)
            {
                StopCoroutine(_layoutRefreshCoroutine);
            }

            _layoutRefreshCoroutine = StartCoroutine(RefreshLayoutCoroutine());
        }

        private IEnumerator RefreshLayoutCoroutine()
        {
            yield return null;
            RebuildLayoutTree();
            yield return null;
            RebuildLayoutTree();
            _layoutRefreshCoroutine = null;
        }

        private void RebuildLayoutTree()
        {
            var root = transform as RectTransform;
            if (root == null)
            {
                return;
            }

            Canvas.ForceUpdateCanvases();

            var fitters = GetComponentsInChildren<ContentSizeFitter>(true);
            for (int i = 0; i < fitters.Length; i++)
            {
                if (fitters[i] == null) continue;
                var rect = fitters[i].transform as RectTransform;
                if (rect != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
                }
            }

            var layoutGroups = GetComponentsInChildren<HorizontalOrVerticalLayoutGroup>(true);
            for (int i = 0; i < layoutGroups.Length; i++)
            {
                if (layoutGroups[i] == null) continue;
                var rect = layoutGroups[i].transform as RectTransform;
                if (rect != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
                }
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(root);
            Canvas.ForceUpdateCanvases();
        }

        private bool IsLockedByAccountLevel(PlayerData playerData)
        {
            return playerData != null && cardLevelData != null && playerData.accountLevel < cardLevelData.unlockLevel;
        }

        private int GetLockedProgressTarget()
        {
            int[] progressArray = GetProgressArray(cardLevelData.developType);
            if (cardUpProgress == null)
            {
                return progressArray[0];
            }

            int progressIndex = Mathf.Clamp(cardUpProgress.level - 1, 0, progressArray.Length - 1);
            return progressArray[progressIndex];
        }

        private bool IsCardMaxLevel()
        {
            if (cardUpProgress == null || cardLevelData == null)
            {
                return false;
            }

            return cardUpProgress.level - 1 >= GetProgressArray(cardLevelData.developType).Length;
        }

        private int[] GetProgressArray(CardDevelopType developType)
        {
            switch (developType)
            {
                case CardDevelopType.UpgradeLingZhangTai:
                    return WorldData.cardUpLevelArr_LingChouLing;
                case CardDevelopType.UpgradeLingChuGe_1:
                case CardDevelopType.UpgradeLingChuGe_2:
                case CardDevelopType.UpgradeYunDiGe:
                    return WorldData.cardUpLevelArr_LingChuGe_YunDiGe;
                case CardDevelopType.UpgradeCharacterWithXuanCaiTuAtk:
                case CardDevelopType.UpgradeCharacterWithXuanCaiTuHp:
                case CardDevelopType.UpgradeGetYuanBaoLing:
                    return WorldData.cardUpLevelArr_WuQiLing_LingLiLingr_YuanBaoLing;
                default:
                    return WorldData.cardUpLevelArr;
            }
        }
    }
}
