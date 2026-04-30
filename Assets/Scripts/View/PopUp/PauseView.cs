using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class PauseView : BaseView
{
    public UIButton closeBtn;
    public UIButton closeGameBtn;
    public UIButton continueBtn;

    Action action;

    public override void UpdateViewWithArgs(params object[] args)
    {
        base.UpdateViewWithArgs(args);
        action = null;
        action = args[0] as Action;
    }

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
           
            action?.Invoke();
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
