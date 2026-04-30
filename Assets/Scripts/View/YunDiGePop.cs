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

public class YunDiGePop : BaseView
{
    private const string LevelSuffix = "级";
    private const string FullLevelText = "已满级";
    private const string MaxLevelText = "等级已满";
    private const string UnlockAtLevel5Text = "5级解锁";
    private const string MaxLevelTip = "等级已满。";
    private const string UnlockAtLevel5Tip = "卡牌等级达到5级解锁。";
    private const string MoneyNotEnoughTip = "铜币数量不足。";
    private const string UpgradeSuccessTip = "升级成功。";

    public RectTransform contentRect;
    public UIButton closeBtn;
    public TextMeshProUGUI speedtxt;
    public TextMeshProUGUI peopletxt;

    public GameObject lockObj;
    public TextMeshProUGUI cardLeveltxt;
    public UIButton cardBtn;
    public GameObject fillContent;
    public TextMeshProUGUI cardfilltxt;
    public Image fillImage;
    public GameObject cardMask;
    public TextMeshProUGUI cardmasktxt;

    public TextMeshProUGUI speedLeveltxt;
    public TextMeshProUGUI currentspeedtxt;
    public TextMeshProUGUI nextspeedtxt;
    public UIButton upgradeSpeedBtn;
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
    DeliverData deliverData;

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
        deliverData = playerData.deliverData;
        speedtxt.text = WorldData.speedLevelDic[deliverData.speedLevel].ToString();
        peopletxt.text = deliverData.totalNum.ToString();
        speedLeveltxt.text = deliverData.speedLevel + LevelSuffix;
        currentspeedtxt.text = WorldData.speedLevelDic[deliverData.speedLevel].ToString();
        peopleLeveltxt.text = deliverData.peopleLevel + LevelSuffix;
        currentpeopletxt.text = deliverData.totalNum.ToString();

        RefreshSpeedUpgradeState();

        var cardprogress = playerData.cardUpProgressesList.Find(x => x.developType == CardDevelopType.UpgradeYunDiGe);
        if (cardprogress == null)
        {
            fillContent.SetActive(false);
            lockObj.SetActive(true);
            cardLeveltxt.text = "0";
        }
        else
        {
            cardLeveltxt.text = cardprogress.level.ToString();
            fillContent.SetActive(true);
            lockObj.SetActive(false);
            if (cardprogress.level == WorldData.cardUpLevelArr_LingChuGe_YunDiGe.Length + 1)
            {
                fillImage.fillAmount = 1f;
                cardfilltxt.text = FullLevelText;
            }
            else
            {
                cardfilltxt.text = cardprogress.currentNum + "/" + WorldData.cardUpLevelArr_LingChuGe_YunDiGe[cardprogress.level - 1];
                fillImage.fillAmount = cardprogress.currentNum * 1f / WorldData.cardUpLevelArr_LingChuGe_YunDiGe[cardprogress.level - 1];
            }
        }

        RefreshPeopleUpgradeState(cardprogress);

        var cardLevelData = DataController.Instance.cardLevelDataList.Find(x => x.developType == CardDevelopType.UpgradeYunDiGe);
        if (cardLevelData.unlockLevel <= playerData.accountLevel)
        {
            cardMask.SetActive(false);
        }
        else
        {
            cardMask.SetActive(true);
            cardmasktxt.text = cardLevelData.unlockLevel.ToString();
        }

        if (animate)
        {
            contentRect.DOAnchorPos(new Vector2(0, 0), 0.3f);
        }
    }

    private void RefreshSpeedUpgradeState()
    {
        bool isMaxLevel = deliverData.speedLevel >= deliverData.maxSpeedLevel;
        btnMask_1.SetActive(isMaxLevel);

        if (isMaxLevel)
        {
            nextspeedtxt.text = string.Empty;
            freetxt_1.text = string.Empty;
            btnMaskTxt_1.text = MaxLevelText;
            return;
        }

        nextspeedtxt.text = WorldData.speedLevelDic[deliverData.speedLevel + 1].ToString();
        freetxt_1.text = Extensions.FormatNumber(deliverData.speedLevel * 5000).ToString();
    }

    private void RefreshPeopleUpgradeState(CardUpProgress cardprogress)
    {
        bool isMaxLevel = deliverData.peopleLevel >= deliverData.maxpeopleLevel;
        bool isUnlocked = cardprogress != null && cardprogress.level >= 5;

        if (isMaxLevel)
        {
            nextpeopletxt.text = string.Empty;
            freetxt_2.text = string.Empty;
            btnMask_2.SetActive(true);
            btnMaskTxt_2.text = MaxLevelText;
            return;
        }

        nextpeopletxt.text = (deliverData.totalNum + 1).ToString();
        freetxt_2.text = Extensions.FormatNumber(deliverData.peopleLevel * 20000).ToString();

        if (!isUnlocked)
        {
            btnMask_2.SetActive(true);
            btnMaskTxt_2.text = UnlockAtLevel5Text;
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
            if (changedCardType != CardDevelopType.UpgradeYunDiGe)
            {
                return;
            }
        }

        RefreshView(false);
    }

    protected override void AddEventListener()
    {
        base.AddEventListener();
        closeBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.AddListener(() =>
        {
            StartCoroutine(HideAnimation());
        });
        upgradeSpeedBtn.onClick.RemoveAllListeners();
        upgradeSpeedBtn.onClick.AddListener(OnClickUpgradeSpeedBtn);
        upgradePeopleBtn.onClick.RemoveAllListeners();
        upgradePeopleBtn.onClick.AddListener(OnClickUpgradePeopleBtn);
        cardBtn.onClick.RemoveAllListeners();
        cardBtn.onClick.AddListener(() =>
        {
            UIController.Instance.Show<CardDetailPop>(DataController.Instance.cardLevelDataList.Find(x => x.developType == CardDevelopType.UpgradeYunDiGe));
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
        if (deliverData.speedLevel == deliverData.maxSpeedLevel)
        {
            UIController.Instance.Show<TipView>(MaxLevelTip);
            return;
        }
        if (PlayerDataModule.Instance.data.tongbi < deliverData.speedLevel * 5000)
        {
            UIController.Instance.Show<TipView>(MoneyNotEnoughTip);
            return;
        }

        PlayerDataModule.Instance.data.tongbi -= deliverData.speedLevel * 5000;
        deliverData.speedLevel += 1;
        RefreshView(false);

        EventCenter.Instance.TriggerEvent(EventMessages.UpGradeStuctureTask, BuildingType.YunDiGe);
        UIController.Instance.Show<TipView>(UpgradeSuccessTip);
        EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
        EventCenter.Instance.TriggerEvent(EventMessages.UpdateYunDiZheSpeed);
    }

    private void OnClickUpgradePeopleBtn()
    {
        var cardProgress = PlayerDataModule.Instance.data.cardUpProgressesList.Find(x => x.developType == CardDevelopType.UpgradeYunDiGe);
        if (cardProgress == null || cardProgress.level < 5)
        {
            UIController.Instance.Show<TipView>(UnlockAtLevel5Tip);
            return;
        }
        if (deliverData.peopleLevel == deliverData.maxpeopleLevel)
        {
            UIController.Instance.Show<TipView>(MaxLevelTip);
            return;
        }
        if (PlayerDataModule.Instance.data.tongbi < deliverData.peopleLevel * 20000)
        {
            UIController.Instance.Show<TipView>(MoneyNotEnoughTip);
            return;
        }

        PlayerDataModule.Instance.data.tongbi -= deliverData.peopleLevel * 20000;
        deliverData.peopleLevel += 1;
        deliverData.totalNum += 1;
        RefreshView(false);

        UIController.Instance.Show<TipView>(UpgradeSuccessTip);
        EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
    }

    private IEnumerator HideAnimation()
    {
        contentRect.DOAnchorPos(new Vector2(0, -1100), 0.3f);
        yield return new WaitForSeconds(0.3f);
        Hide();
    }
}
