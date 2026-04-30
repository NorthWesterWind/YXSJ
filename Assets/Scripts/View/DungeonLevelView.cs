using System;
using System.Collections;
using Module;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;
using View;

public class DungeonLevelView : BaseView
{
    public UIButton closeBtn;
    public UIButton btn_1;
    public GameObject image_1;
    public UIButton btn_2;
    public GameObject image_2;
    public UIButton gameBtn_1;
    public UIButton gameBtn_2;
    public TextMeshProUGUI infotxt_1;
    public TextMeshProUGUI infotxt_2;
    public GameObject loadView;
    public Image fillImage;
    public UIButton detailBtn;

    private bool isFirstSelected = true;

    protected override void AddEventListener()
    {
        base.AddEventListener();
        closeBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.AddListener(OnClickCloseBtn);
        gameBtn_1.onClick.RemoveAllListeners();
        gameBtn_1.onClick.AddListener(OnClickGameBtn_1);
        gameBtn_2.onClick.RemoveAllListeners();
        gameBtn_2.onClick.AddListener(OnClickGameBtn_2);

        btn_1.onClick.RemoveAllListeners();
        btn_1.onClick.AddListener(() =>
        {
            isFirstSelected = true;
            if (!image_1.activeSelf)
            {
                image_1.SetActive(true);
            }
            if (image_2.activeSelf)
            {
                image_2.SetActive(false);
            }
        });
        btn_2.onClick.RemoveAllListeners();
        btn_2.onClick.AddListener(() =>
        {
            isFirstSelected = false;
            if (!image_2.activeSelf)
            {
                image_2.SetActive(true);
            }
            if (image_1.activeSelf)
            {
                image_1.SetActive(false);
            }
        });

        detailBtn.onClick.RemoveAllListeners();
        detailBtn.onClick.AddListener(() =>
        {
            UIController.Instance.Show<DetailView>();
        });
    }

    public override void UpdateViewWithArgs(params object[] args)
    {
        base.UpdateViewWithArgs(args);

        isFirstSelected = true;
        image_1.SetActive(false);
        image_2.SetActive(false);
        loadView.SetActive(false);
    }
    void Update()
    {
        UpdateTimeInfo();
    }
    void UpdateTimeInfo()
    {
        infotxt_2.text = "今日剩余挑战次数：" + PlayerDataModule.Instance.data.playLingBaoCount + "。";

        if (PlayerDataModule.Instance.data.canPlayXuanJing)
        {
            infotxt_1.text = "当前可进行挑战。";
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
            infotxt_1.text = "当前可进行挑战。";
        }
        else
        {
            double remainSeconds = cooldownSeconds - passedSeconds;

            int minutes = Mathf.FloorToInt((float)remainSeconds / 60f);
            int seconds = Mathf.CeilToInt((float)remainSeconds % 60f);

            infotxt_1.text = $"{minutes:D2}:{seconds:D2} 后可进行挑战。";
        }



    }

    void OnClickCloseBtn()
    {
        Hide();
    }
    void OnClickGameBtn_2()
    {
        if (PlayerDataModule.Instance.data.playLingBaoCount <= 0)
        {
            UIController.Instance.Show<TipView>("今日挑战次数已用完!");
            return;
        }
        PlayerDataModule.Instance.data.playTrialCurrencyType = Module.Data.CurrencyType.JingYuanBao;
        UIController.Instance.Show<TrialView>(true);
        PlayerDataModule.Instance.data.playLingBaoCount -= 1;
    }
    void OnClickGameBtn_1()
    {
        if (!PlayerDataModule.Instance.data.canPlayXuanJing)
        {
            UIController.Instance.Show<TipView>("当前关卡冷却中!");
            return;
        }
        PlayerDataModule.Instance.data.playTrialCurrencyType = Module.Data.CurrencyType.LingJing;
        DateTime time = DateTime.Now;
        PlayerDataModule.Instance.data.playXuanJingTime = time.ToString("yyyy-MM-dd HH:mm:ss");
        PlayerDataModule.Instance.data.canPlayXuanJing = false;
        PlayerDataModule.Instance.CaptureCurrentRuntimeState();
        PlayerDataModule.Instance.SuspendRuntimeCaptureForSceneTransition();
        loadView.SetActive(true);
        StartCoroutine(LoadNextSceneCoroutine());
    }

    private IEnumerator LoadNextSceneCoroutine()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        AsyncOperation asyncLoad;
        asyncLoad = SceneManager.LoadSceneAsync($"Game_Trial");
        asyncLoad.allowSceneActivation = false;
        float displayProgress = 0f;
        while (!asyncLoad.isDone)
        {
            float targetProgress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            displayProgress = Mathf.MoveTowards(displayProgress, targetProgress, Time.deltaTime * 0.3f);
            fillImage.fillAmount = displayProgress;
            if (asyncLoad.progress >= 0.9f && displayProgress >= 0.99f)
            {
                displayProgress = Mathf.MoveTowards(displayProgress, 1f, Time.deltaTime * 0.3f);
                fillImage.fillAmount = displayProgress;
                if (displayProgress >= 1f)
                {
                    yield return new WaitForSeconds(0.5f);
                    asyncLoad.allowSceneActivation = true;
                }
            }
            yield return null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayerDataModule.Instance.BeginAutoSave();

        if (scene.name == $"Game_Trial")
        {
            UIController.Instance.Show<TrialGameView>();
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


    protected override void OnHideComplete()
    {
        base.OnHideComplete();
        EventCenter.Instance.TriggerEvent(EventMessages.ShowPlayerInfoViewCartoon);
    }
}
