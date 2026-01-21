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
            var data = args[0] as CardUpProgress;
            switch (data.levelType)
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
            var leveldata = DataController.Instance.cardLevelDataList.Find(x => x.developType == data.developType);
            cardUpProgress = PlayerDataModule.Instance.data.cardUpProgressesList.Find(x => x.id == leveldata.id);
            cardNameTxt.text = leveldata.name;
            cardLevelTxt.text = data.level + "";
            cardNumTxt.text = "x" + (int)args[1];
            icon.sprite = assetHandle.Get<Sprite>(leveldata.name);
            fillImg.fillAmount = cardUpProgress.currentNum * 1f / WorldData.cardUpLevelArr[cardUpProgress.level - 1];
            fillTxt.text = cardUpProgress.currentNum + "/" + WorldData.cardUpLevelArr[cardUpProgress.level - 1];
            if (cardUpProgress.level == 10)
            {
                fillTxt.text = "已满级";
                fillImg.fillAmount = 1f;
            }

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

}
