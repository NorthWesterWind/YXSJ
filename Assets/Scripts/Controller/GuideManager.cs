using Module;
using UnityEngine;
using Utils;

public enum GuideStep
{
    BuildYushaPot = 1,
    BuildTeaStand = 4,
    CollectMaterial = 5,
    DeliverMaterial = 6,
    BuildAccountDesk = 8,
    TakeTea = 10,
    SellTea = 12,
    Checkout = 13,
    UpgradePot = 15,

    Finished = 16,
    Over =17,
}

public class GuideManager : MonoSingleton<GuideManager>
{

    public GuideStep CurrentStep { get; private set; }
    public bool IsRunning { get; private set; }

    public GuideFingerController finger; // 手指指引

    public override void Awake()
    {
        base.Awake();
        LoadProgress();
    }

    #region Progress

    private void LoadProgress()
    {
        CurrentStep = PlayerDataModule.Instance.data.guideStep;
    }

    private void SaveProgress()
    {
        PlayerDataModule.Instance.data.guideStep = CurrentStep;
    }

    #endregion

    #region Control

    public void StartStep(GuideStep step, Transform target)
    {
        if (step != CurrentStep) return;
        if (IsRunning) return;

        IsRunning = true;
        finger.StartGuide(target);
    }

    public void CompleteStep()
    {
        if (!IsRunning) return;

        IsRunning = false;
        finger.StopGuide();

        CurrentStep = GetNextStep(CurrentStep);
        SaveProgress();
    }

    private GuideStep GetNextStep(GuideStep step)
    {
        switch (step)
        {
            case GuideStep.BuildYushaPot: return GuideStep.BuildTeaStand;
            case GuideStep.BuildTeaStand: return GuideStep.CollectMaterial;
            case GuideStep.CollectMaterial: return GuideStep.DeliverMaterial;
            case GuideStep.DeliverMaterial: return GuideStep.BuildAccountDesk;
            case GuideStep.BuildAccountDesk: return GuideStep.TakeTea;
            case GuideStep.TakeTea: return GuideStep.SellTea;
            case GuideStep.SellTea: return GuideStep.Checkout;
            case GuideStep.Checkout: return GuideStep.UpgradePot;
            case GuideStep.UpgradePot: return GuideStep.Finished;
            default: return GuideStep.Finished;
        }
    }

    #endregion
}
