using System.Collections;
using System.Collections.Generic;
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

    public GameObject cardContent;
    public GameObject cardMask;
    public TextMeshProUGUI cardMaskTxt;
    public UIButton cardBtn;

    public TextMeshProUGUI storageTxt;

    public TextMeshProUGUI atkLevelTxt;
    public TextMeshProUGUI currentAtkTxt;
    public TextMeshProUGUI nextAtkTxt;
    public TextMeshProUGUI gradefreeTxt_1;
    public UIButton upgradeAtkBtn;

    public GameObject numgradeMask;
    public TextMeshProUGUI numgradeMaskTxt;
    public TextMeshProUGUI numLevelTxt;
    public TextMeshProUGUI currentNumTxt;
    public TextMeshProUGUI nextNumTxt;
    public TextMeshProUGUI gradefreeTxt_2;
    public UIButton upgradeNumBtn;

    public WarehouseCategoryType warehouseCategoryType;
    public WarehouseCategory warehouse;
    CardDevelopType cardType;
    protected override void Awake()
    {
        contentRect.anchoredPosition = new Vector2(0, -1100);
    }
    public override void UpdateViewWithArgs(params object[] args)
    {
        base.UpdateViewWithArgs(args);
        var type = (BuildingType)args[0];
        cardType = CardDevelopType.UpgradeLingChuGe_1;
        if (type == BuildingType.LingChuGe_1)
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
        atktxt.text = warehouse.atk.ToString();
        numtxt.text = warehouse.peopleNum.ToString();
        storageTxt.text = warehouse.capacity.ToString();
        if (warehouse.atkLevel < warehouse.maxAtkLevel)
        {
            gradefreeTxt_1.text = (warehouse.atkLevel * 3000).ToString();
        }
        else
        {
            gradefreeTxt_1.text = "等级已满";
        }

        gradefreeTxt_2.text = (warehouse.numLevel * 20000).ToString();
        atkLevelTxt.text = warehouse.atkLevel+ "级";
        numLevelTxt.text = warehouse.numLevel+ "级";
        currentAtkTxt.text = warehouse.atk.ToString();
        nextAtkTxt.text = (warehouse.atk + 1.5).ToString();

        currentNumTxt.text = warehouse.peopleNum.ToString();
        if (warehouse.peopleNum < 3)
        {
            nextNumTxt.text = (warehouse.peopleNum + 1).ToString();
        }
        else
        {
            nextNumTxt.text = "";
        }

        var cardProgress = PlayerDataModule.Instance.data.cardUpProgressesList.Find(x => x.developType == cardType);
        var cardLevelData = DataController.Instance.cardLevelDataList.Find(x => x.developType == cardType);
        cardMask.SetActive(false);

        if (cardProgress == null)
        {
            lockObj.SetActive(true);
            cardContent.SetActive(false);

            if (PlayerDataModule.Instance.data.accountLevel < cardLevelData.unlockLevel)
            {
                cardMask.SetActive(true);
                cardMaskTxt.text = cardLevelData.unlockLevel.ToString();
            }
            fillContent.SetActive(false);

            numgradeMask.SetActive(true);
            numgradeMaskTxt.text = "需要达到5级";
        }
        else
        {
            lockObj.SetActive(false);
            fillContent.SetActive(true);
            cardContent.SetActive(true);
            cardLevelTxt.text = cardProgress.level.ToString();
            cardFillTxt.text = cardProgress.currentNum + "/" + WorldData.cardUpLevelArr[cardProgress.level + 1];
            if (cardProgress.level == 10)
            {
                cardFill.fillAmount = 1f;
            }
            else
            {
                cardFill.fillAmount = cardProgress.currentNum * 1f / WorldData.cardUpLevelArr[cardProgress.level + 1];
            }
            if (cardProgress.level < 5)
            {
                numgradeMask.SetActive(true);
                numgradeMaskTxt.text = "需要达到5级";
            }
            else
            {
                numgradeMask.SetActive(false);
            }

        }

        if (warehouse.peopleNum == 3)
        {
            numgradeMask.SetActive(true);
            numgradeMaskTxt.text = "等级已满";
        }
    }


    protected override void AddEventListener()
    {
        base.AddEventListener();
        upgradeAtkBtn.onClick.RemoveAllListeners();
        upgradeAtkBtn.onClick.AddListener(OnClickUpgradeAtkBtn);
        upgradeNumBtn.onClick.RemoveAllListeners();
        upgradeNumBtn.onClick.AddListener(OnClickUpgradeNumBtn);
        cardBtn.onClick.RemoveAllListeners();
        cardBtn.onClick.AddListener(() => { UIController.Instance.Show<CardDetailPop>(DataController.Instance.cardLevelDataList.Find(x => x.developType == cardType)); });
        closeBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.AddListener((() =>
        {
            StartCoroutine(HideAnimation());
        }));
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
            UIController.Instance.Show<TipView>("等级已满。");
        }
        if (PlayerDataModule.Instance.data.tongbi < warehouse.atkLevel * 3000)
        {
            UIController.Instance.Show<TipView>("铜币数量不足。");
        }
        else
        {
            PlayerDataModule.Instance.data.tongbi -= warehouse.atkLevel * 3000;
            warehouse.atkLevel += 1;
            warehouse.atk += 1.5f;
            atkLevelTxt.text = warehouse.atkLevel+ "级";
            atktxt.text = warehouse.atk.ToString();
            currentAtkTxt.text = warehouse.atk.ToString();
            if(warehouse.atkLevel < warehouse.maxAtkLevel)
            {
                 nextAtkTxt.text = (warehouse.atk + 1.5).ToString();
            }
            else
            {
                 nextAtkTxt.text = "";
            }
           
            UIController.Instance.Show<TipView>("升级成功。");
        }
    }
    private void OnClickUpgradeNumBtn()
    {
        if (warehouse.peopleNum == 3)
        {
            UIController.Instance.Show<TipView>("等级已满。");
        }
        if (PlayerDataModule.Instance.data.tongbi < warehouse.numLevel * 20000)
        {
            UIController.Instance.Show<TipView>("铜币数量不足。");
        }
        else
        {
            PlayerDataModule.Instance.data.tongbi -= warehouse.numLevel * 20000;
            warehouse.numLevel += 1;
            warehouse.peopleNum += 1;
            numLevelTxt.text = warehouse.numLevel + "级";
            numtxt.text = warehouse.peopleNum.ToString();
            currentNumTxt.text = warehouse.peopleNum.ToString();
            if(warehouse.peopleNum < 3)
            {
                 nextNumTxt.text = (warehouse.peopleNum + 1).ToString();
            }
            else
            {
                 nextNumTxt.text = "";
            }
           
            UIController.Instance.Show<TipView>("升级成功。");
        }
    }
}
