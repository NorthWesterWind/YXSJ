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

public class ZhuanPanView : BaseView
{
    public Transform content;

    public UIButton closeBtn;
    public UIButton beginBtn;
    public UIButton boxBtn;
    public Image redPoint;
    public Image fillImage;
    public TextMeshProUGUI filltxt;
    public TextMeshProUGUI remaintxt;

    protected override void AddEventListener()
    {
        base.AddEventListener();
        closeBtn.onClick.AddListener((() =>
        {
            Hide();
        }));
        beginBtn.onClick.AddListener((() =>
        {
            BeginZhuanPan();
        }));
        boxBtn.onClick.AddListener((() =>
        {
            if (PlayerDataModule.Instance.data.currentUseNum < 5)
            {
                return;
            }
            PlayerDataModule.Instance.data.currentUseNum -= 5;
            filltxt.text = PlayerDataModule.Instance.data.currentUseNum + "/5";
            fillImage.fillAmount = PlayerDataModule.Instance.data.currentUseNum * 1f / 5f;
            if (PlayerDataModule.Instance.data.currentUseNum >= 5)
            {
                redPoint.gameObject.SetActive(true);
            }
            else
            {
                redPoint.gameObject.SetActive(false);
            }

            PlayerDataModule.Instance.data.goldIngot += DataController.Instance.giftpackDataDic[5].JinYuanBao;
            var dic = PlayerDataModule.Instance.LotteryCard(DataController.Instance.giftpackDataDic[5]);
            UIController.Instance.Show<RewardConfirmView>(dic, new Dictionary<CurrencyType, int> { { CurrencyType.JingYuanBao, DataController.Instance.giftpackDataDic[5].JinYuanBao } });
            EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
        }));

    }

    protected override void OnHideComplete()
    {
        base.OnHideComplete();
        EventCenter.Instance.TriggerEvent(EventMessages.ShowPlayerInfoViewCartoon);
    }
    override public void UpdateViewWithArgs(params object[] args)
    {
        base.UpdateViewWithArgs(args);

        content.rotation = Quaternion.Euler(0, 0, 0);
        filltxt.text = PlayerDataModule.Instance.data.currentUseNum + "/5";
        fillImage.fillAmount = PlayerDataModule.Instance.data.currentUseNum * 1f / 5f;
        if (PlayerDataModule.Instance.data.currentUseNum >= 5)
        {
            redPoint.gameObject.SetActive(true);
        }
        else
        {
            redPoint.gameObject.SetActive(false);
        }
        remaintxt.text = "今日剩余转动次数：" + (10 - PlayerDataModule.Instance.data.todayUseZhuanPanNum) + "。";
    }

    public void BeginZhuanPan()
    {
        if (PlayerDataModule.Instance.data.lingJing < 50)
        {
            UIController.Instance.Show<TipView>("灵晶不足!");
            return;
        }
        if (PlayerDataModule.Instance.data.todayUseZhuanPanNum >= 10)
        {
            UIController.Instance.Show<TipView>("今日转盘次数已用完!");
            return;
        }
         PlayerDataModule.Instance.data.useZhuanPanTotalValue += 1;
        PlayerDataModule.Instance.data.todayUseZhuanPanNum += 1;
        PlayerDataModule.Instance.data.currentUseNum += 1;
        PlayerDataModule.Instance.data.lingJing -= 50;
        PlayerDataModule.Instance.data.useLingJingTotalValue += 50;
        filltxt.text = PlayerDataModule.Instance.data.currentUseNum + "/5";
        fillImage.fillAmount = PlayerDataModule.Instance.data.currentUseNum * 1f / 5f;
        if (PlayerDataModule.Instance.data.currentUseNum >= 5)
        {
            redPoint.gameObject.SetActive(true);
        }
        else
        {
            redPoint.gameObject.SetActive(false);
        }
        Spin();
        remaintxt.text = "今日剩余转动次数：" + (10 - PlayerDataModule.Instance.data.todayUseZhuanPanNum)+"。";
    }

    public int sectorCount = 8;          // 奖区数量
    public float rotateDuration = 3f;    // 旋转时间
    public int extraRounds = 3;           // 额外整圈（让动画好看）

    private float sectorAngle;

    private void Awake()
    {
        sectorAngle = 360f / sectorCount;
    }


    public void Spin()
    {

        int rewardIndex = Random.Range(0, sectorCount);
        float centerAngle = rewardIndex * sectorAngle + sectorAngle / 2f;
        float targetAngle = -(extraRounds * 360f + centerAngle);
        content.DORotate(
                new Vector3(0, 0, targetAngle),
                rotateDuration,
                RotateMode.FastBeyond360
            )
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                Debug.Log($"停在奖区索引: {rewardIndex}");
                OnReward(rewardIndex);
            });
    }

    private void OnReward(int index)
    {

        Debug.LogError(" yj ==> index == > " + index);
        switch (index)
        {
            case 7:
                PlayerDataModule.Instance.data.goldIngot += 400;
                UIController.Instance.Show<RewardConfirmView>(new Dictionary<CurrencyType, int> { { CurrencyType.JingYuanBao, 400 } });
                break;
            case 6:
                PlayerDataModule.Instance.data.lingJing += 80;
                UIController.Instance.Show<RewardConfirmView>(new Dictionary<CurrencyType, int> { { CurrencyType.LingJing, 80 } });
                break;
            case 5:
                PlayerDataModule.Instance.data.lingJing += 40;
                UIController.Instance.Show<RewardConfirmView>(new Dictionary<CurrencyType, int> { { CurrencyType.LingJing, 40 } });
                break;
            case 4:
                PlayerDataModule.Instance.data.lingJing += 100;
                UIController.Instance.Show<RewardConfirmView>(new Dictionary<CurrencyType, int> { { CurrencyType.LingJing, 100 } });
                break;
            case 3:
                PlayerDataModule.Instance.data.goldIngot += 150;
                UIController.Instance.Show<RewardConfirmView>(new Dictionary<CurrencyType, int> { { CurrencyType.JingYuanBao, 150 } });
                break;
            case 2:
                PlayerDataModule.Instance.data.speedTime += 15 * 60;
                UIController.Instance.Show<RewardConfirmView>(new Dictionary<CurrencyType, int> { { CurrencyType.Speed, 15 } });
                break;
            case 0:
                PlayerDataModule.Instance.data.speedTime += 10 * 60;
                UIController.Instance.Show<RewardConfirmView>(new Dictionary<CurrencyType, int> { { CurrencyType.Speed, 10 } });
                break;
            case 1:
                PlayerDataModule.Instance.data.speedTime += 5 * 60;
                UIController.Instance.Show<RewardConfirmView>(new Dictionary<CurrencyType, int> { { CurrencyType.Speed, 5 } });

                break;
        }
        EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
    }


}