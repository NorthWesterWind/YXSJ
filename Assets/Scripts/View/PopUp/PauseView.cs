using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class PauseView : BaseView
{
    public UIButton closeBtn;
    public UIButton closeGameBtn;
    public UIButton continueBtn;

    protected override void AddEventListener()
    {
        base.AddEventListener();
        closeBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.AddListener((() =>
        {
            Time.timeScale = 1;
            Hide();
        }));

        closeGameBtn.onClick.RemoveAllListeners();
        closeGameBtn.onClick.AddListener((() =>
        {
            Time.timeScale = 1;
            EventCenter.Instance.TriggerEvent(EventMessages.CloseTrialView);
            Hide();
        }));

        continueBtn.onClick.RemoveAllListeners();
        continueBtn.onClick.AddListener((() =>
        {
            Time.timeScale = 1;
            Hide();
        }));
    }
}
