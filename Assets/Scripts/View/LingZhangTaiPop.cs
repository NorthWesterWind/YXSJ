using System;
using System.Collections;
using Controller;
using DG.Tweening;
using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using View;
using View.CardView;

public class LingZhangTaiPop : BaseView
{
    private const string LevelSuffix = "级";
    private const string FullLevelText = "已满级";
    private const string MaxLevelText = "等级已满";
    private const string UnlockAtLevel2Text = "2级解锁";
    private const string MaxLevelTip = "等级已满。";
    private const string UnlockAtLevel2Tip = "卡牌等级达到2级解锁。";
    private const string MoneyNotEnoughTip = "铜币数量不足。";
    private const string UpgradeSuccessTip = "升级成功。";

    public RectTransform contentRect;
    public UIButton closeBtn;
    public TextMeshProUGUI workspeedtxt;
    public TextMeshProUGUI peopletxt;
    public GameObject lockObj;
    public TextMeshProUGUI cardLeveltxt;
    public UIButton cardBtn;
    public GameObject fillContent;
    public TextMeshProUGUI cardfilltxt;
    public Image fillImage;
    public GameObject cardMask;
    public TextMeshProUGUI cardmasktxt;
    public TextMeshProUGUI workspeedLeveltxt;
    public TextMeshProUGUI currentworkspeedtxt;
    public TextMeshProUGUI nextworkspeedtxt;
    public UIButton upgradeworkspeedBtn;
    public GameObject btnMask_1;
    public TextMeshProUGUI btnMaskTxt_1;
    public TextMeshProUGUI freetxt_1;
    public TextMeshProUGUI peopleLeveltxt;
    public TextMeshProUGUI currentpeopletxt;
    public TextMeshProUGUI nextpeopletxt;
    public UIButton upgradePeopleBtn;
    public GameObject btnMask_2;
    public TextMeshProUGUI btnMaskTxt_2;
    public TextMeshProUGUI freetxt_2;
    PlayerData playerData;
    CashierData cashierData;
    public TextMeshProUGUI donatetxt;

    protected override void Awake()
    {
        contentRect.anchoredPosition = new Vector2(0, -1100);
    }

    public override void UpdateViewWithArgs(params object[] args)
    {
        base.UpdateViewWithArgs(args);
        RefreshView(true);
    }

    private void RefreshView(bool animate)
    {
        playerData = PlayerDataModule.Instance.data;
        cashierData = playerData.cashierData;
        EnsureCashierDataValid();
        workspeedtxt.text = FormatWorkSpeed(cashierData.currentWorkingSpeed);
        peopletxt.text = cashierData.totalNum.ToString();
        workspeedLeveltxt.text = cashierData.workspeedLevel + LevelSuffix;
        currentworkspeedtxt.text = FormatWorkSpeed(cashierData.currentWorkingSpeed);
        peopleLeveltxt.text = cashierData.peopleLevel + LevelSuffix;
        currentpeopletxt.text = cashierData.totalNum.ToString();

        RefreshWorkSpeedUpgradeState();

        var cardLevelData = DataController.Instance.cardLevelDataList.Find(x => x.developType == CardDevelopType.UpgradeLingZhangTai);
        if (cardLevelData.unlockLevel <= playerData.accountLevel)
        {
            cardMask.SetActive(false);
        }
        else
        {
            cardMask.SetActive(true);
            cardmasktxt.text = cardLevelData.unlockLevel.ToString();
        }

        var cardprogress = playerData.cardUpProgressesList.Find(x => x.developType == CardDevelopType.UpgradeLingZhangTai);
        if (cardprogress == null)
        {
            fillContent.SetActive(false);
            lockObj.SetActive(true);
            cardLeveltxt.text = "0";
            donatetxt.text = "x0%";
        }
        else
        {
            cardLeveltxt.text = cardprogress.level.ToString();
            fillContent.SetActive(true);
            lockObj.SetActive(false);
            if (cardprogress.level < WorldData.cardUpLevelArr_LingChouLing.Length + 1)
            {
                cardfilltxt.text = cardprogress.currentNum + "/" + WorldData.cardUpLevelArr_LingChouLing[cardprogress.level - 1];
                fillImage.fillAmount = cardprogress.currentNum * 1f / WorldData.cardUpLevelArr_LingChouLing[cardprogress.level - 1];
            }
            else
            {
                cardfilltxt.text = FullLevelText;
                fillImage.fillAmount = 1f;
            }

            donatetxt.text = $"x {cardprogress.level * 0.2f * 100f}%";
        }

        RefreshPeopleUpgradeState(cardprogress);

        if (animate)
        {
            contentRect.DOAnchorPos(new Vector2(0, 0), 0.3f);
        }
    }

    private void RefreshWorkSpeedUpgradeState()
    {
        bool isMaxLevel = cashierData.workspeedLevel >= cashierData.maxworkspeedLevel;
        btnMask_1.SetActive(isMaxLevel);

        if (isMaxLevel)
        {
            nextworkspeedtxt.text = string.Empty;
            freetxt_1.text = string.Empty;
            btnMaskTxt_1.text = MaxLevelText;
            return;
        }

        nextworkspeedtxt.text = FormatWorkSpeed(cashierData.currentWorkingSpeed - 0.05f);
        freetxt_1.text = Extensions.FormatNumber(cashierData.workspeedLevel * 1000).ToString();
    }

    private void RefreshPeopleUpgradeState(CardUpProgress cardprogress)
    {
        bool isMaxLevel = cashierData.peopleLevel >= cashierData.maxpeopleLevel;
        bool isUnlocked = cardprogress != null && cardprogress.level >= 2;

        if (isMaxLevel)
        {
            nextpeopletxt.text = string.Empty;
            freetxt_2.text = string.Empty;
            btnMask_2.SetActive(true);
            btnMaskTxt_2.text = MaxLevelText;
            return;
        }

        nextpeopletxt.text = (cashierData.totalNum + 1).ToString();
        freetxt_2.text = Extensions.FormatNumber(cashierData.peopleLevel * 20000).ToString();

        if (!isUnlocked)
        {
            btnMask_2.SetActive(true);
            btnMaskTxt_2.text = UnlockAtLevel2Text;
            return;
        }

        btnMask_2.SetActive(false);
    }

    private void RefreshCardInfoEvent(params object[] args)
    {
        if (!IsVisible)
        {
            return;
        }

        if (args != null && args.Length > 0 && args[0] is CardDevelopType changedCardType)
        {
            if (changedCardType != CardDevelopType.UpgradeLingZhangTai)
            {
                return;
            }
        }

        RefreshView(false);
    }

    private void EnsureCashierDataValid()
    {
        if (playerData.cashierData == null)
        {
            playerData.cashierData = new CashierData();
        }

        cashierData = playerData.cashierData;
        cashierData.maxpeopleLevel = Mathf.Max(1, cashierData.maxpeopleLevel);
        cashierData.maxworkspeedLevel = Mathf.Max(1, cashierData.maxworkspeedLevel);
        cashierData.peopleLevel = Mathf.Clamp(Mathf.Max(1, cashierData.peopleLevel), 1, cashierData.maxpeopleLevel);
        cashierData.workspeedLevel = Mathf.Clamp(Mathf.Max(1, cashierData.workspeedLevel), 1, cashierData.maxworkspeedLevel);
        cashierData.totalNum = Mathf.Clamp(Mathf.Max(1, cashierData.totalNum, cashierData.peopleLevel), 1, cashierData.maxpeopleLevel);
        cashierData.workingNum = Mathf.Clamp(cashierData.workingNum, 0, cashierData.totalNum);
        if (cashierData.currentWorkingSpeed <= 0f)
        {
            cashierData.currentWorkingSpeed = 5f;
        }

        cashierData.currentWorkingSpeed = RoundWorkSpeed(cashierData.currentWorkingSpeed);
    }

    protected override void AddEventListener()
    {
        base.AddEventListener();
        closeBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.AddListener(() =>
        {
            StartCoroutine(HideAnimation());
        });
        upgradeworkspeedBtn.onClick.RemoveAllListeners();
        upgradeworkspeedBtn.onClick.AddListener(OnClickUpgradeSpeedBtn);
        upgradePeopleBtn.onClick.RemoveAllListeners();
        upgradePeopleBtn.onClick.AddListener(OnClickUpgradePeopleBtn);
        cardBtn.onClick.RemoveAllListeners();
        cardBtn.onClick.AddListener(() =>
        {
            UIController.Instance.Show<CardDetailPop>(DataController.Instance.cardLevelDataList.Find(x => x.developType == CardDevelopType.UpgradeLingZhangTai));
        });
        EventCenter.Instance.AddListener(EventMessages.UpdateCardInfo, RefreshCardInfoEvent);
    }

    public override void RemoveEventListener()
    {
        base.RemoveEventListener();
        EventCenter.Instance.RemoveListener(EventMessages.UpdateCardInfo, RefreshCardInfoEvent);
    }

    private void OnClickUpgradeSpeedBtn()
    {
        if (cashierData.workspeedLevel == cashierData.maxworkspeedLevel)
        {
            UIController.Instance.Show<TipView>(MaxLevelTip);
            return;
        }
        if (PlayerDataModule.Instance.data.tongbi < cashierData.workspeedLevel * 1000)
        {
            UIController.Instance.Show<TipView>(MoneyNotEnoughTip);
            return;
        }

        PlayerDataModule.Instance.data.tongbi -= cashierData.workspeedLevel * 1000;
        cashierData.workspeedLevel += 1;
        cashierData.currentWorkingSpeed = RoundWorkSpeed(cashierData.currentWorkingSpeed - 0.05f);
        RefreshView(false);

        UIController.Instance.Show<TipView>(UpgradeSuccessTip);
        EventCenter.Instance.TriggerEvent(EventMessages.UpGradeStuctureTask, BuildingType.LingZhangTai);
        EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
    }

    private void OnClickUpgradePeopleBtn()
    {
        var cardProgress = PlayerDataModule.Instance.data.cardUpProgressesList.Find(x => x.developType == CardDevelopType.UpgradeLingZhangTai);
        if (cardProgress == null || cardProgress.level < 2)
        {
            UIController.Instance.Show<TipView>(UnlockAtLevel2Tip);
            return;
        }
        if (cashierData.peopleLevel == cashierData.maxpeopleLevel)
        {
            UIController.Instance.Show<TipView>(MaxLevelTip);
            return;
        }
        if (PlayerDataModule.Instance.data.tongbi < cashierData.peopleLevel * 20000)
        {
            UIController.Instance.Show<TipView>(MoneyNotEnoughTip);
            return;
        }

        PlayerDataModule.Instance.data.tongbi -= cashierData.peopleLevel * 20000;
        cashierData.peopleLevel += 1;
        cashierData.totalNum += 1;
        RefreshView(false);

        UIController.Instance.Show<TipView>(UpgradeSuccessTip);
        EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
        EventCenter.Instance.TriggerEvent(EventMessages.UpdateLingZhangTai);
    }

    private IEnumerator HideAnimation()
    {
        contentRect.DOAnchorPos(new Vector2(0, -1100), 0.3f);
        yield return new WaitForSeconds(0.3f);
        Hide();
    }

    private static float RoundWorkSpeed(float value)
    {
        return (float)Math.Round(value, 2);
    }

    private static string FormatWorkSpeed(float value)
    {
        return RoundWorkSpeed(value).ToString("0.##");
    }
}
