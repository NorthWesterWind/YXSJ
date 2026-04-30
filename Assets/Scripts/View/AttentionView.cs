using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class AttentionView : BaseView
{
    public TextMeshProUGUI tiptxt;
    public TextMeshProUGUI infotxt;
    public UIButton confirmBtn;
    private Action callback;
    public VerticalLayoutGroup verticalLayoutGroup;
    public override void UpdateViewWithArgs(params object[] args)
    {
        base.UpdateViewWithArgs(args);
        infotxt.text = args[0] as string;
        tiptxt.text = args[1] as string;
        callback = null;
        if (args.Length > 2 && args[2] is Action)
        {
            callback = (Action)args[2];
        }
        verticalLayoutGroup.enabled = false;
        verticalLayoutGroup.enabled = true;
        infotxt.gameObject.SetActive(false);
        infotxt.gameObject.SetActive(true);
        Canvas.ForceUpdateCanvases();
        infotxt.ForceMeshUpdate(); 
        LayoutRebuilder.ForceRebuildLayoutImmediate(
            verticalLayoutGroup.GetComponent<RectTransform>()
        );

    }

    protected override void AddEventListener()
    {
        base.AddEventListener();
        confirmBtn.onClick.AddListener((() =>
        {
            Hide();
            callback?.Invoke();
        }));
    }
}
