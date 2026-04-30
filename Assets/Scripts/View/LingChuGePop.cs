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

public class LingChuGePop : BaseView
{
    private const string LevelSuffix = "级";
    private const string FullLevelText = "已满级";
    private const string MaxLevelText = "等级已满";
    private const string UnlockAtLevel5Text = "5级解锁";
    private const string StoragePrefix = "容量：";
    private const string MaxLevelTip = "等级已满。";
    private const string UnlockAtLevel5Tip = "卡牌等级达到5级解锁。";
    private const string MoneyNotEnoughTip = "铜币数量不足。";
    private const string UpgradeSuccessTip = "升级成功。";

    public Image icon;
    public RectTransform contentRect;
    public UIButton closeBtn;
    public TextMeshProUGUI buildTxt;
    public GameObject lockObj;
    public TextMeshProUGUI atktxt;
    public TextMeshProUGUI numtxt;
    public TextMeshProUGUI cardNametxt;

    public GameObject fillContent;
    public Image cardFill;

    public TextMeshProUGUI cardFillTxt;
    public TextMeshProUGUI cardLevelTxt;
    public GameObject cardMask;
    public TextMeshProUGUI cardMaskTxt;
    public UIButton cardBtn;
    public TextMeshProUGUI atkLevelTxt;
    public TextMeshProUGUI currentAtkTxt;
    public TextMeshProUGUI nextAtkTxt;
    public TextMeshProUGUI gradefreeTxt_1;
    public UIButton upgradeAtkBtn;
    public GameObject btnMask_1;

    public GameObject numgradeMask;
    public TextMeshProUGUI numgradeMaskTxt;
    public TextMeshProUGUI numLevelTxt;
    public TextMeshProUGUI currentNumTxt;
    public TextMeshProUGUI nextNumTxt;
    public TextMeshProUGUI gradefreeTxt_2;
    public GameObject btnMask_2;
    public UIButton upgradeNumBtn;

    public WarehouseCategoryType warehouseCategoryType;
    public WarehouseCategory warehouse;
    private CardDevelopType cardType;
    private BuildingType buildingType;
    public TextMeshProUGUI storageTxt;

    protected override void Awake()
    {
        contentRect.anchoredPosition = new Vector2(0, -1100);
    }

    public override void UpdateViewWithArgs(params object[] args)
    {
        base.UpdateViewWithArgs(args);
        if (args == null || args.Length == 0)
        {
            return;
        }

        buildingType = (BuildingType)args[0];
        RefreshView(true);
    }

    private void RefreshView(bool animate)
    {
        btnMask_1.SetActive(false);
        btnMask_2.SetActive(false);
        cardType = CardDevelopType.UpgradeLingChuGe_1;

        if (buildingType == BuildingType.LingChuGe_1)
        {
            warehouseCategoryType = WarehouseCategoryType.LingChuGe_1;
            buildTxt.text = "一号灵储阁";
            cardNametxt.text = "一号灵储阁";
        }
        else
        {
            warehouseCategoryType = WarehouseCategoryType.LingChuGe_2;
            buildTxt.text = "二号灵储阁";
            cardNametxt.text = "二号灵储阁";
            cardType = CardDevelopType.UpgradeLingChuGe_2;
        }

        warehouse = PlayerDataModule.Instance.data.warehouselist.Find(x => x.warehouseCategoryType == warehouseCategoryType);
        if (warehouse == null)
        {
            return;
        }

        atktxt.text = warehouse.atk.ToString();
        numtxt.text = warehouse.peopleNum.ToString();
        if (warehouse.atkLevel < warehouse.maxAtkLevel)
        {
            gradefreeTxt_1.text = Extensions.FormatNumber(warehouse.atkLevel * 3000);
            btnMask_1.SetActive(false);
        }
        else
        {
            btnMask_1.SetActive(true);
            gradefreeTxt_1.text = string.Empty;
        }

        if (warehouse.numLevel < warehouse.maxNumLevel)
        {
            gradefreeTxt_2.text = Extensions.FormatNumber(warehouse.numLevel * 20000);
        }
        else
        {
            gradefreeTxt_2.text = string.Empty;
        }

        atkLevelTxt.text = warehouse.atkLevel + LevelSuffix;
        numLevelTxt.text = warehouse.numLevel + LevelSuffix;
        currentAtkTxt.text = warehouse.atk.ToString();
        nextAtkTxt.text = warehouse.atkLevel < warehouse.maxAtkLevel ? (warehouse.atk + 1.5f).ToString() : string.Empty;

        currentNumTxt.text = warehouse.peopleNum.ToString();
        nextNumTxt.text = warehouse.numLevel < warehouse.maxNumLevel ? (warehouse.peopleNum + 1).ToString() : string.Empty;

        var cardProgress = PlayerDataModule.Instance.data.cardUpProgressesList.Find(x => x.developType == cardType);
        var cardLevelData = DataController.Instance.cardLevelDataList.Find(x => x.developType == cardType);
        if (cardLevelData == null)
        {
            return;
        }

        cardMask.SetActive(false);
        if (cardProgress == null)
        {
            lockObj.SetActive(true);
            cardLevelTxt.text = "0";

            if (PlayerDataModule.Instance.data.accountLevel < cardLevelData.unlockLevel)
            {
                cardMask.SetActive(true);
                cardMaskTxt.text = cardLevelData.unlockLevel.ToString();
            }

            fillContent.SetActive(false);
        }
        else
        {
            lockObj.SetActive(false);
            fillContent.SetActive(true);
            cardLevelTxt.text = cardProgress.level.ToString();

            if (cardProgress.level == WorldData.cardUpLevelArr_LingChuGe_YunDiGe.Length + 1)
            {
                cardFill.fillAmount = 1f;
                cardFillTxt.text = FullLevelText;
            }
            else
            {
                int need = WorldData.cardUpLevelArr_LingChuGe_YunDiGe[cardProgress.level - 1];
                cardFillTxt.text = cardProgress.currentNum + "/" + need;
                cardFill.fillAmount = cardProgress.currentNum * 1f / need;
            }
        }

        RefreshNumUpgradeState(cardProgress);

        if (_assetHandle == null)
        {
            _assetHandle = GetComponent<AssetHandle>();
        }

        if (PlayerDataModule.Instance.data.currentMapID == 1 || PlayerDataModule.Instance.data.currentMapID == 2)
        {
            icon.sprite = _assetHandle.Get<Sprite>("LingChuGe1");
        }
        else if (PlayerDataModule.Instance.data.currentMapID == 3)
        {
            icon.sprite = _assetHandle.Get<Sprite>("LingChuGe3");
        }
        else if (PlayerDataModule.Instance.data.currentMapID == 4)
        {
            icon.sprite = _assetHandle.Get<Sprite>("LingChuGe4");
        }
        else if (PlayerDataModule.Instance.data.currentMapID == 5)
        {
            icon.sprite = _assetHandle.Get<Sprite>("LingChuGe5");
        }

        RefreshStorageTxt();
        if (animate)
        {
            contentRect.DOAnchorPos(new Vector2(0, 0), 0.3f);
        }
    }

    private void RefreshStorageTxt()
    {
        if (storageTxt == null)
        {
            storageTxt = FindStorageTxt();
        }

        if (storageTxt == null)
        {
            return;
        }

        int current = 0;
        int total = 0;

        var controller = GetLingChuGeController();
        if (controller != null)
        {
            current = Mathf.Max(0, controller.currentcapacity);
        }
        else
        {
            current = GetWarehouseStoredCount();
        }

        total = GetWarehouseMaxCapacity();
        storageTxt.text = StoragePrefix + current + "/" + total;
    }

    private void RefreshNumUpgradeState(CardUpProgress cardProgress)
    {
        bool isMaxLevel = warehouse != null && warehouse.numLevel >= warehouse.maxNumLevel;
        bool isUnlocked = cardProgress != null && cardProgress.level >= 5;

        if (isMaxLevel)
        {
            nextNumTxt.text = string.Empty;
            gradefreeTxt_2.text = string.Empty;
            btnMask_2.SetActive(false);
            numgradeMask.SetActive(true);
            numgradeMaskTxt.text = FullLevelText;
            return;
        }

        if (!isUnlocked)
        {
            btnMask_2.SetActive(true);
            numgradeMask.SetActive(true);
            numgradeMaskTxt.text = UnlockAtLevel5Text;
            return;
        }

        btnMask_2.SetActive(false);
        numgradeMask.SetActive(false);
    }

    private int GetWarehouseStoredCount()
    {
        if (warehouse?.ownItemList?.list == null)
        {
            return 0;
        }

        int total = 0;
        foreach (var kv in warehouse.ownItemList.list)
        {
            if (kv != null && kv.value > 0)
            {
                total += kv.value;
            }
        }

        return total;
    }

    private int GetWarehouseMaxCapacity()
    {
        if (warehouse == null)
        {
            return 0;
        }

        int total = warehouse.capacity;
        var playerData = PlayerDataModule.Instance.data;
        if (playerData != null && playerData.cardUpProgressesList != null)
        {
            var cardData = playerData.cardUpProgressesList.Find(x => x.developType == cardType);
            if (cardData != null)
            {
                total += cardData.level * 10;
            }
        }

        return total;
    }

    private TextMeshProUGUI FindStorageTxt()
    {
        var texts = GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var text in texts)
        {
            if (text != null && text.gameObject.name == "storagetxt")
            {
                return text;
            }
        }

        return null;
    }

    private LingChuGeController GetLingChuGeController()
    {
        if (GameController.Instance == null || GameController.Instance.buildings == null)
        {
            return null;
        }

        if (GameController.Instance.buildings.TryGetValue(buildingType, out var building))
        {
            return building as LingChuGeController;
        }

        return null;
    }

    private void RefreshStorageTxtEvent(params object[] args)
    {
        if (!IsVisible)
        {
            return;
        }

        RefreshStorageTxt();
    }

    private void RefreshCardInfoEvent(params object[] args)
    {
        if (!IsVisible || warehouse == null)
        {
            return;
        }

        if (args != null && args.Length > 0 && args[0] is CardDevelopType changedCardType)
        {
            if (changedCardType != CardDevelopType.UpgradeLingChuGe_1 &&
                changedCardType != CardDevelopType.UpgradeLingChuGe_2)
            {
                return;
            }

            if (changedCardType != cardType)
            {
                return;
            }
        }

        RefreshView(false);
    }

    protected override void AddEventListener()
    {
        base.AddEventListener();
        upgradeAtkBtn.onClick.RemoveAllListeners();
        upgradeAtkBtn.onClick.AddListener(OnClickUpgradeAtkBtn);
        upgradeNumBtn.onClick.RemoveAllListeners();
        upgradeNumBtn.onClick.AddListener(OnClickUpgradeNumBtn);
        cardBtn.onClick.RemoveAllListeners();
        cardBtn.onClick.AddListener(() =>
        {
            UIController.Instance.Show<CardDetailPop>(DataController.Instance.cardLevelDataList.Find(x => x.developType == cardType));
        });
        closeBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.AddListener(() =>
        {
            StartCoroutine(HideAnimation());
        });

        EventCenter.Instance.AddListener(EventMessages.UpdateLingChuGeInfo, RefreshStorageTxtEvent);
        EventCenter.Instance.AddListener(EventMessages.UpdateCardInfo, RefreshCardInfoEvent);
    }

    public override void RemoveEventListener()
    {
        base.RemoveEventListener();
        EventCenter.Instance.RemoveListener(EventMessages.UpdateLingChuGeInfo, RefreshStorageTxtEvent);
        EventCenter.Instance.RemoveListener(EventMessages.UpdateCardInfo, RefreshCardInfoEvent);
    }

    private IEnumerator HideAnimation()
    {
        contentRect.DOAnchorPos(new Vector2(0, -1100), 0.3f);
        yield return new WaitForSeconds(0.3f);
        Hide();
    }

    private void OnClickUpgradeAtkBtn()
    {
        if (warehouse.atkLevel == warehouse.maxAtkLevel)
        {
            UIController.Instance.Show<TipView>(MaxLevelTip);
            return;
        }
        if (PlayerDataModule.Instance.data.tongbi < warehouse.atkLevel * 3000)
        {
            UIController.Instance.Show<TipView>(MoneyNotEnoughTip);
            return;
        }

        PlayerDataModule.Instance.data.tongbi -= warehouse.atkLevel * 3000;
        warehouse.atkLevel += 1;
        warehouse.atk += 1.5f;
        RefreshView(false);

        if (warehouse.warehouseCategoryType == WarehouseCategoryType.LingChuGe_1)
        {
            EventCenter.Instance.TriggerEvent(EventMessages.UpGradeStuctureTask, BuildingType.LingChuGe_1);
        }
        else
        {
            EventCenter.Instance.TriggerEvent(EventMessages.UpGradeStuctureTask, BuildingType.LingChuGe_2);
        }

        UIController.Instance.Show<TipView>(UpgradeSuccessTip);
        EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
        EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerValueInfo);
    }

    private void OnClickUpgradeNumBtn()
    {
        var cardProgress = PlayerDataModule.Instance.data.cardUpProgressesList.Find(x => x.developType == cardType);
        if (cardProgress == null || cardProgress.level < 5)
        {
            UIController.Instance.Show<TipView>(UnlockAtLevel5Tip);
            return;
        }
        if (warehouse.numLevel >= warehouse.maxNumLevel)
        {
            UIController.Instance.Show<TipView>(MaxLevelTip);
            return;
        }
        if (PlayerDataModule.Instance.data.tongbi < warehouse.numLevel * 20000)
        {
            UIController.Instance.Show<TipView>(MoneyNotEnoughTip);
            return;
        }

        PlayerDataModule.Instance.data.tongbi -= warehouse.numLevel * 20000;
        warehouse.numLevel += 1;
        warehouse.peopleNum += 1;
        AddIdleCollectorForUpgrade();
        RefreshView(false);

        UIController.Instance.Show<TipView>(UpgradeSuccessTip);
        EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
        EventCenter.Instance.TriggerEvent(EventMessages.UpdateLingChuGeWorkingInfo);
    }

    private void AddIdleCollectorForUpgrade()
    {
        if (warehouse == null)
        {
            return;
        }

        warehouse.workingCollectorList ??= new System.Collections.Generic.List<Collector>();
        warehouse.unworkingCollectorList ??= new System.Collections.Generic.List<Collector>();

        int nextCollectorId = 1;
        foreach (var collector in warehouse.workingCollectorList)
        {
            if (collector != null)
            {
                nextCollectorId = Mathf.Max(nextCollectorId, collector.id + 1);
            }
        }

        foreach (var collector in warehouse.unworkingCollectorList)
        {
            if (collector != null)
            {
                nextCollectorId = Mathf.Max(nextCollectorId, collector.id + 1);
            }
        }

        warehouse.unworkingCollectorList.Add(new Collector(nextCollectorId, MonsterFamily.None));
    }
}
