using System.Collections;
using DG.Tweening;
using Module;
using UnityEngine;
using Utils;
using View;

public class ZhuanPanView : BaseView
{
    public Transform content;

    public UIButton closeBtn;
    public UIButton beginBtn;

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
    }

    public void BeginZhuanPan()
    {
        if (ModuleMgr.Instance.GetModule<PlayerDataModule>().data.lingJing < 50)
        {
            UIController.Instance.Show<TipView>("灵晶不足!");
            return;
        }
        if (ModuleMgr.Instance.GetModule<PlayerDataModule>().data.todayUseZhuanPanNum>=10)
        {
            UIController.Instance.Show<TipView>("今日转盘次数已用完!");
            return;
        }
        ModuleMgr.Instance.GetModule<PlayerDataModule>().data.todayUseZhuanPanNum += 1;
        ModuleMgr.Instance.GetModule<PlayerDataModule>().data.lingJing -= 50;
        Spin();
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
    
        switch(index)
        {
            case 0:
                ModuleMgr.Instance.GetModule<PlayerDataModule>().data.goldIngot += 400;
                break;
            case 1:
                ModuleMgr.Instance.GetModule<PlayerDataModule>().data.lingJing += 80;
                break;
            case 2:
                ModuleMgr.Instance.GetModule<PlayerDataModule>().data.lingJing += 40;
                break;
            case 3:
                ModuleMgr.Instance.GetModule<PlayerDataModule>().data.lingJing += 100;
                break;
            case 4:
                ModuleMgr.Instance.GetModule<PlayerDataModule>().data.goldIngot += 150;
                break;
            case 5:
                ModuleMgr.Instance.GetModule<PlayerDataModule>();
                break;
            case 6:
                ModuleMgr.Instance.GetModule<PlayerDataModule>();
                break;
            case 7:
                ModuleMgr.Instance.GetModule<PlayerDataModule>();
                break;
        }
        EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
    }
    

}