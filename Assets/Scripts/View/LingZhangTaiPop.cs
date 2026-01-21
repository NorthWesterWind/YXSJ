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

public class LingZhangTaiPop : BaseView
{
   
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

    protected override void Awake()
    {
        contentRect.anchoredPosition = new Vector2(0, -1100);
    }

    public override void UpdateViewWithArgs(params object[] args)
    {
        base.UpdateViewWithArgs(args);
        playerData = PlayerDataModule.Instance.data;
        contentRect.DOAnchorPos(new Vector2(0, 0), 0.3f);
        cashierData = playerData.cashierData;
        workspeedtxt.text = cashierData.currentWorkingSpeed.ToString();
        peopletxt.text = cashierData.totalNum.ToString();
        workspeedLeveltxt.text = cashierData.workspeedLevel + "级";
        currentworkspeedtxt.text = cashierData.currentWorkingSpeed.ToString();
        if (cashierData.workspeedLevel < cashierData.maxworkspeedLevel)
        {
            nextworkspeedtxt.text = (cashierData.currentWorkingSpeed - 0.05f).ToString();
            btnMask_1.SetActive(false);
        }
        else
        {
            nextworkspeedtxt.text = "";
            btnMask_1.SetActive(true);
            btnMaskTxt_1.text = "等级已满";
        }
        freetxt_1.text = (cashierData.workspeedLevel * 1000).ToString();
        freetxt_2.text = (cashierData.peopleLevel * 20000).ToString();
        peopleLeveltxt.text = cashierData.peopleLevel + "级";
        currentpeopletxt.text = cashierData.totalNum.ToString();
        if (cashierData.peopleLevel < cashierData.maxpeopleLevel)
        {
            nextpeopletxt.text = (cashierData.totalNum + 1).ToString();
        }
        else
        {
            nextpeopletxt.text = "";
            btnMask_2.SetActive(true);
            btnMaskTxt_2.text = "等级已满";
        }


        var cardprogress = playerData.cardUpProgressesList.Find(x => x.developType == CardDevelopType.UpgradeLingZhangTai);
        if (cardprogress == null)
        {
            fillContent.SetActive(false);
            lockObj.SetActive(true);
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
            btnMask_2.SetActive(true);
            btnMaskTxt_2.text = "需要达到2级";
        }
        else
        {
            cardLeveltxt.text = cardprogress.level.ToString();
            fillContent.SetActive(true);
            lockObj.SetActive(false);
            cardfilltxt.text = cardprogress.currentNum + "/" + WorldData.cardUpLevelArr[cardprogress.level + 1];
            fillImage.fillAmount = cardprogress.currentNum * 1f / WorldData.cardUpLevelArr[cardprogress.level + 1];

            if (cardprogress.level < 2)
            {
                btnMask_2.SetActive(true);
                btnMaskTxt_2.text = "需要达到2级";
            }
            else
            {
                btnMask_2.SetActive(false);
            }
        }

    }


    protected override void AddEventListener()
    {
        base.AddEventListener();
        closeBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.AddListener((() =>
        {
            StartCoroutine(HideAnimation());
        }));

        upgradeworkspeedBtn.onClick.RemoveAllListeners();
        upgradeworkspeedBtn.onClick.AddListener(OnClickUpgradeSpeedBtn);
        upgradePeopleBtn.onClick.RemoveAllListeners();
        upgradePeopleBtn.onClick.AddListener(OnClickUpgradePeopleBtn);
        cardBtn.onClick.RemoveAllListeners();
        cardBtn.onClick.AddListener(() => { UIController.Instance.Show<CardDetailPop>(DataController.Instance.cardLevelDataList.Find(x => x.developType == CardDevelopType.UpgradeYunDiGe)); });
    }
    private void OnClickUpgradeSpeedBtn()
    {
        if (cashierData.workspeedLevel == cashierData.maxworkspeedLevel)
        {
            UIController.Instance.Show<TipView>("等级已满。");
        }
        if (PlayerDataModule.Instance.data.tongbi < cashierData.workspeedLevel * 1000)
        {
            UIController.Instance.Show<TipView>("铜币数量不足。");
        }
        else
        {
            PlayerDataModule.Instance.data.tongbi -= cashierData.workspeedLevel * 1000;
            cashierData.workspeedLevel += 1;
            cashierData.currentWorkingSpeed -= 0.05f;
            workspeedLeveltxt.text =  cashierData.workspeedLevel + "级";
            workspeedtxt.text =  cashierData.currentWorkingSpeed.ToString();
            currentworkspeedtxt.text = cashierData.currentWorkingSpeed.ToString();
            if (cashierData.workspeedLevel == cashierData.maxworkspeedLevel)
            {
                btnMask_1.SetActive(true);
                btnMaskTxt_1.text = "等级已满";
                nextworkspeedtxt.text = "";
            }
            else
            {
                 nextworkspeedtxt.text = (cashierData.currentWorkingSpeed - 0.05f).ToString();
            }

            UIController.Instance.Show<TipView>("升级成功。");
        }
    }
    private void OnClickUpgradePeopleBtn()
    {
        if (cashierData.peopleLevel == cashierData.maxpeopleLevel)
        {
            UIController.Instance.Show<TipView>("等级已满。");
        }
        if (PlayerDataModule.Instance.data.tongbi < cashierData.peopleLevel * 20000)
        {
            UIController.Instance.Show<TipView>("铜币数量不足。");
        }
        else
        {
            PlayerDataModule.Instance.data.tongbi -= cashierData.peopleLevel * 20000;
            cashierData.peopleLevel += 1;
            cashierData.totalNum += 1;
            peopleLeveltxt.text = cashierData.peopleLevel + "级";
            peopletxt.text = cashierData.totalNum.ToString();
            currentpeopletxt.text = cashierData.totalNum.ToString();
            if (cashierData.peopleLevel == cashierData.maxpeopleLevel)
            {
                btnMask_2.SetActive(true);
                btnMaskTxt_2.text = "等级已满";
                nextpeopletxt.text = "";
            }
            else
            {
                nextpeopletxt.text = (cashierData.totalNum + 1).ToString();
            }

            UIController.Instance.Show<TipView>("升级成功。");
        }
    }


    private IEnumerator HideAnimation()
    {
        contentRect.DOAnchorPos(new Vector2(0, -1100), 0.3f);
        yield return new WaitForSeconds(0.3f);
        Hide();
    }

}
