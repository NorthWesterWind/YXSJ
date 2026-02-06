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
            return;
        }

        DateTime recordTime = DateTime.MinValue;
        if (!string.IsNullOrEmpty(PlayerDataModule.Instance.data.playXuanJingTime))
        {
            recordTime = DateTime.Parse(PlayerDataModule.Instance.data.playXuanJingTime);
        }

        DateTime now = DateTime.Now;
        TimeSpan delta = now - recordTime;

        double passedSeconds = delta.TotalSeconds;
        double cooldownSeconds = 30 * 60;

        if (passedSeconds >= cooldownSeconds)
        {
            PlayerDataModule.Instance.data.canPlayXuanJing = true;
            infotxt_2.text = "当前可进行挑战。";
        }
        else
        {
            double remainSeconds = cooldownSeconds - passedSeconds;

            int minutes = Mathf.FloorToInt((float)remainSeconds / 60f);
            int seconds = Mathf.CeilToInt((float)remainSeconds % 60f);

            infotxt_2.text = $"{minutes:D2}:{seconds:D2} 后可进行挑战";
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
