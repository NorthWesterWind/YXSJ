using System;
using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class TrialResultView : BaseView
{
    public Image bg;
    public GameObject JybObj;
    public TextMeshProUGUI Jybtxt;
    public GameObject LjObj;
    public TextMeshProUGUI Ljtxt;
    public UIButton btn;
    protected override void AddEventListener()
    {
        base.AddEventListener();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() =>
        {
            EventCenter.Instance.TriggerEvent(EventMessages.CloseTrialView);
            Hide();
        });
    }
    public override void UpdateViewWithArgs(params object[] args)
    {
        base.UpdateViewWithArgs(args);
        bool isSucces = (bool)args[0];
        int value = (int)args[1];
        if (isSucces)
        {
            bg.sprite = _assetHandle.Get<Sprite>("胜利界面");
        }
        else
        {
            bg.sprite = _assetHandle.Get<Sprite>("失败界面");
        }
        CurrencyType type = PlayerDataModule.Instance.data.playTrialCurrencyType;
        if (type == CurrencyType.JingYuanBao)
        {
            JybObj.SetActive(true);
            LjObj.SetActive(false);
            Jybtxt.text = value.ToString();
            PlayerDataModule.Instance.data.goldIngot += value;
            EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
            PlayerDataModule.Instance.data.playLingBaoCount -= 1;
        }
        else
        {
            JybObj.SetActive(false);
            LjObj.SetActive(true);
            Ljtxt.text = value.ToString();
            PlayerDataModule.Instance.data.lingJing += value;
            EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
            DateTime time = DateTime.Now;
            PlayerDataModule.Instance.data.playXuanJingTime = time.ToString("yyyy/MM/dd/HH:mm:ss");
            PlayerDataModule.Instance.data.canPlayXuanJing = false;
        }

    }
}
