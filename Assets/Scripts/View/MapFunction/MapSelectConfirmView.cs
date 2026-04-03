using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Utils;

public class MapSelectConfirmView : BaseView
{
    public UIButton confirmBtn;
    public UIButton cancelBtn;

    public TextMeshProUGUI infoText;
    public Action actionOnConfirm;

    protected override void AddEventListener()
    {
        base.AddEventListener();
        confirmBtn.onClick.RemoveAllListeners();
        confirmBtn.onClick.AddListener(OnClickConfirmBtn);
        cancelBtn.onClick.RemoveAllListeners();
        cancelBtn.onClick.AddListener(OnClickCancelBtn);
    }

    public override void UpdateViewWithArgs(params object[] args)
    {
        base.UpdateViewWithArgs(args);
        if (args.Length > 0 && args[0] is string)
        {
            infoText.text = (string)args[0];
        }
        if (args.Length > 1 && args[1] is Action)
        {
            actionOnConfirm = (Action)args[1];
        }
    }

    private void OnClickConfirmBtn()
    {
        if(actionOnConfirm != null)
        {
            actionOnConfirm.Invoke();
        }
        else
        {
            Debug.LogWarning("MapSelectConfirmView: Confirm button clicked but no action assigned.");
        }
        Hide();
    }

    private void OnClickCancelBtn()
    {
        Hide();
    }
}
