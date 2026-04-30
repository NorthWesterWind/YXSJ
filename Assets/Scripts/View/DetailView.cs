using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class DetailView : BaseView
{
    public UIButton closeBtn;
    protected override void AddEventListener()
    {
        base.AddEventListener();

        closeBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.AddListener(Hide);
    }
}
