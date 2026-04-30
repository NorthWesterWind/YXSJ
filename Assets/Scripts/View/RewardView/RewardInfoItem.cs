using System.Collections;
using System.Collections.Generic;
using Controller;
using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class RewardInfoItem : MonoBehaviour
{
    private AssetHandle assetHandle;
    public GameObject contentCard;
    public GameObject contentOther;
    public TextMeshProUGUI otherNameTxt;
    public TextMeshProUGUI otherValueTxt;
    public Image otherIcon;

    public Image iconBg;
    public Image icon;
    public TextMeshProUGUI cardNameTxt;
    public TextMeshProUGUI cardLevelTxt;
    public TextMeshProUGUI cardNumTxt;
    public TextMeshProUGUI fillTxt;
    public Image fillImg;

    public CardUpProgress cardUpProgress;
    public void Init(int id, params object[] args)
    {
        if (assetHandle == null)
        {
            assetHandle = GetComponent<AssetHandle>();
        }
        if (id == 1)
        {
            contentCard.SetActive(true);
            contentOther.SetActive(false);
            int cardId = ResolveCardId(args);
            int rewardCount = ResolveRewardCount(args);
            var levelData = DataController.Instance.cardLevelDataList.Find(x => x.id == cardId);
            if (levelData == null)
            {
                return;
            }

            cardUpProgress = PlayerDataModule.Instance.data.cardUpProgressesList.Find(x => x.id == cardId);
            switch (levelData.levelType)
            {
                case CardLevelType.FanPing:
                    iconBg.sprite = assetHandle.Get<Sprite>("白卡");
                    break;
                case CardLevelType.XianYun:
                    iconBg.sprite = assetHandle.Get<Sprite>("红卡");
                    break;
                case CardLevelType.LingYun:
                    iconBg.sprite = assetHandle.Get<Sprite>("紫卡");
                    break;
            }
            cardNameTxt.text = levelData.name;
            cardLevelTxt.text = cardUpProgress != null ? cardUpProgress.level.ToString() : "1";
            cardNumTxt.text = "x" + rewardCount;
            icon.sprite = assetHandle.Get<Sprite>(levelData.name);
            RefreshCardProgress(levelData);

        }
        else
        {
            contentCard.SetActive(false);
            contentOther.SetActive(true);

            CurrencyType currencyType = (CurrencyType)args[0];
            switch (currencyType)
            {
                case CurrencyType.JingYuanBao:
                    otherNameTxt.text = "金元宝";
                    otherIcon.sprite = assetHandle.Get<Sprite>("JingYuanBao");
                    otherValueTxt.text = Extensions.FormatNumber((long)args[1]);
                    break;
                case CurrencyType.LingJing:
                    otherNameTxt.text = "灵晶";
                    otherIcon.sprite = assetHandle.Get<Sprite>("LingJing");
                    otherValueTxt.text = Extensions.FormatNumber((long)args[1]);
                    break;
                case CurrencyType.TongBi:
                    otherNameTxt.text = "铜币";
                    otherIcon.sprite = assetHandle.Get<Sprite>("TongBi");
                    otherValueTxt.text = Extensions.FormatNumber((long)args[1]);
                    break;
                case CurrencyType.Speed:
                    otherNameTxt.text = "生产速度增加";
                    otherIcon.sprite = assetHandle.Get<Sprite>("灵瞬令");
                    otherValueTxt.text = Extensions.FormatNumber((long)args[1]) + "分钟";
                    break;
            }


        }


    }

    private int ResolveCardId(object[] args)
    {
        if (args == null || args.Length == 0)
        {
            return 0;
        }

        if (args[0] is CardUpProgress progress)
        {
            return progress.id;
        }

        if (args[0] is int id)
        {
            return id;
        }

        return 0;
    }

    private int ResolveRewardCount(object[] args)
    {
        if (args == null || args.Length < 2)
        {
            return 0;
        }

        if (args[1] is int intValue)
        {
            return intValue;
        }

        if (args[1] is long longValue)
        {
            return (int)longValue;
        }

        return 0;
    }

    private void RefreshCardProgress(CardLevelData levelData)
    {
        if (levelData == null)
        {
            fillImg.fillAmount = 0f;
            fillTxt.text = string.Empty;
            return;
        }

        int[] progressArr = GetProgressArray(levelData.developType);
        if (cardUpProgress == null)
        {
            fillImg.fillAmount = 0f;
            fillTxt.text = progressArr.Length > 0 ? "0/" + progressArr[0] : string.Empty;
            return;
        }

        int progressIndex = Mathf.Max(0, cardUpProgress.level - 1);
        if (progressIndex >= progressArr.Length)
        {
            fillTxt.text = "已满级";
            fillImg.fillAmount = 1f;
            return;
        }

        int need = Mathf.Max(1, progressArr[progressIndex]);
        fillImg.fillAmount = Mathf.Clamp01(cardUpProgress.currentNum * 1f / need);
        fillTxt.text = cardUpProgress.currentNum + "/" + need;
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
