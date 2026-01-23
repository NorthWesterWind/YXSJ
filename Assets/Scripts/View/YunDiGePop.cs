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

public class YunDiGePop : BaseView
{
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
        playerData = PlayerDataModule.Instance.data;
        contentRect.DOAnchorPos(new Vector2(0, 0), 0.3f);
        deliverData = playerData.deliverData;
        speedtxt.text = deliverData.currentMoveSpeed.ToString();
        peopletxt.text = deliverData.totalNum.ToString();
        speedLeveltxt.text = deliverData.speedLevel + "级";
        currentspeedtxt.text = deliverData.currentMoveSpeed.ToString();
        if (deliverData.speedLevel < deliverData.maxSpeedLevel)
        {
            nextspeedtxt.text = (deliverData.currentMoveSpeed + 0.5f).ToString();
            btnMask_1.SetActive(false);
        }
        else
        {
            nextspeedtxt.text = "";
            btnMask_1.SetActive(true);
            btnMaskTxt_1.text = "等级已满";
        }
        freetxt_1.text = (deliverData.speedLevel * 5000).ToString();
        freetxt_2.text = (deliverData.peopleLevel * 20000).ToString();
        peopleLeveltxt.text = deliverData.peopleLevel + "级";
        currentpeopletxt.text = deliverData.totalNum.ToString();
        if (deliverData.peopleLevel < deliverData.maxpeopleLevel)
        {
            nextpeopletxt.text = (deliverData.totalNum + 1).ToString();
        }
        else
        {
            nextpeopletxt.text = "";
            btnMask_2.SetActive(true);
            btnMaskTxt_2.text = "等级已满";
        }


        var cardprogress = playerData.cardUpProgressesList.Find(x => x.developType == CardDevelopType.UpgradeYunDiGe);
        if (cardprogress == null)
        {
            fillContent.SetActive(false);
            lockObj.SetActive(true);
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
            btnMask_2.SetActive(true);
            btnMaskTxt_2.text = "需要达到5级";
        }
        else
        {
            cardLeveltxt.text = cardprogress.level.ToString();
            fillContent.SetActive(true);
            lockObj.SetActive(false);
            cardfilltxt.text = cardprogress.currentNum + "/" + WorldData.cardUpLevelArr[cardprogress.level + 1];
            fillImage.fillAmount = cardprogress.currentNum * 1f / WorldData.cardUpLevelArr[cardprogress.level + 1];

            if (cardprogress.level < 5)
            {
                btnMask_2.SetActive(true);
                btnMaskTxt_2.text = "需要达到5级";
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

        upgradeSpeedBtn.onClick.RemoveAllListeners();
        upgradeSpeedBtn.onClick.AddListener(OnClickUpgradeSpeedBtn);
        upgradePeopleBtn.onClick.RemoveAllListeners();
        upgradePeopleBtn.onClick.AddListener(OnClickUpgradePeopleBtn);
        cardBtn.onClick.RemoveAllListeners();
        cardBtn.onClick.AddListener(() => { UIController.Instance.Show<CardDetailPop>(DataController.Instance.cardLevelDataList.Find(x => x.developType == CardDevelopType.UpgradeYunDiGe)); });
    }
    private void OnClickUpgradeSpeedBtn()
    {
        if (deliverData.speedLevel == deliverData.maxSpeedLevel)
        {
            UIController.Instance.Show<TipView>("等级已满。");
        }
        if (PlayerDataModule.Instance.data.tongbi < deliverData.speedLevel * 5000)
        {
            UIController.Instance.Show<TipView>("铜币数量不足。");
        }
        else
        {
            PlayerDataModule.Instance.data.tongbi -= deliverData.speedLevel * 5000;
            deliverData.speedLevel += 1;
            deliverData.currentMoveSpeed += 0.5f;
            speedLeveltxt.text = deliverData.speedLevel + "级";
            speedtxt.text = deliverData.currentMoveSpeed.ToString();
            currentspeedtxt.text = deliverData.speedLevel.ToString();
            if (deliverData.speedLevel == deliverData.maxSpeedLevel)
            {
                btnMask_1.SetActive(true);
                btnMaskTxt_1.text = "等级已满";
                nextspeedtxt.text = "";
            }
            else
            {
                nextspeedtxt.text = (deliverData.currentMoveSpeed + 0.5).ToString();
            }
            EventCenter.Instance.TriggerEvent(EventMessages.UpGradeStuctureTask , BuildingType.YunDiGe);
            UIController.Instance.Show<TipView>("升级成功。");
        }
    }
    private void OnClickUpgradePeopleBtn()
    {
        if (deliverData.peopleLevel == deliverData.maxpeopleLevel)
        {
            UIController.Instance.Show<TipView>("等级已满。");
        }
        if (PlayerDataModule.Instance.data.tongbi < deliverData.speedLevel * 20000)
        {
            UIController.Instance.Show<TipView>("铜币数量不足。");
        }
        else
        {
            PlayerDataModule.Instance.data.tongbi -= deliverData.speedLevel * 20000;
            deliverData.peopleLevel += 1;
            deliverData.totalNum += 1;
            peopleLeveltxt.text = deliverData.peopleLevel + "级";
            peopletxt.text = deliverData.totalNum.ToString();
            currentpeopletxt.text = deliverData.totalNum.ToString();
            if (deliverData.peopleLevel == deliverData.maxpeopleLevel)
            {
                btnMask_2.SetActive(true);
                btnMaskTxt_2.text = "等级已满";
                nextpeopletxt.text = "";
            }
            else
            {
                nextpeopletxt.text = (deliverData.totalNum + 1).ToString();
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
