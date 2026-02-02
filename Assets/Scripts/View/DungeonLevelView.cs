using System;
using Module;
using TMPro;
using UnityEngine;
using Utils;
using View;

public class DungeonLevelView : BaseView
{
    public UIButton closeBtn;
    public UIButton gameBtn_1;
    public UIButton gameBtn_2;
    public TextMeshProUGUI infotxt_1;
    public TextMeshProUGUI infotxt_2;

    protected override void AddEventListener()
    {
        base.AddEventListener();
        closeBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.AddListener(OnClickCloseBtn);
        gameBtn_1.onClick.RemoveAllListeners();
        gameBtn_1.onClick.AddListener(OnClickGameBtn_1);
        gameBtn_2.onClick.RemoveAllListeners();
        gameBtn_2.onClick.AddListener(OnClickGameBtn_2);
    }

    public override void UpdateViewWithArgs(params object[] args)
    {
        base.UpdateViewWithArgs(args);

    }
    void Update()
    {
        UpdateTimeInfo();
    }
    void UpdateTimeInfo()
    {
        infotxt_1.text = "今日剩余挑战次数:" + PlayerDataModule.Instance.data.playLingBaoCount + "。";
        if (PlayerDataModule.Instance.data.canPlayXuanJing)
        {
            infotxt_2.text = "当前可进行挑战。";
        }
        else 
        {

            DateTime recordTime = DateTime.Parse(PlayerDataModule.Instance.data.playXuanJingTime);
            DateTime now = DateTime.Now;
            TimeSpan delta = now - recordTime;
            double passedMinutes = delta.TotalMinutes;

            if (passedMinutes >= 30)
            {
                PlayerDataModule.Instance.data.canPlayXuanJing = true;
            }
            else
            {
                // 剩余多少分钟到 30
                double remainMinutes = 30 - passedMinutes;
                infotxt_2.text = $"{Mathf.CeilToInt((float)remainMinutes)} 分钟后可进行挑战。";
            }


        }

    }

    void OnClickCloseBtn()
    {
        Hide();
    }
    void OnClickGameBtn_1()
    {
        if (PlayerDataModule.Instance.data.playLingBaoCount <= 0)
        {
            UIController.Instance.Show<TipView>("今日挑战次数已用完!");
            return;
        }
        PlayerDataModule.Instance.data.playTrialCurrencyType = Module.Data.CurrencyType.JingYuanBao;
        UIController.Instance.Show<TrialView>(true);

    }
    void OnClickGameBtn_2()
    {
        if (!PlayerDataModule.Instance.data.canPlayXuanJing)
        {
             UIController.Instance.Show<TipView>("当前关卡冷却中!");
            return;
        }
        PlayerDataModule.Instance.data.playTrialCurrencyType = Module.Data.CurrencyType.LingJing;
        UIController.Instance.Show<TrialView>(false);
    }
    protected override void OnHideComplete()
    {
        base.OnHideComplete();
        EventCenter.Instance.TriggerEvent(EventMessages.ShowPlayerInfoViewCartoon);
    }
}
