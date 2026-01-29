using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class FriendView : BaseView
{
    public GameObject content_1;
    public Transform content_1Transform;

    public GameObject content_2;
    public Transform content_2Transform;
    public UIButton addAllBtn;
    public UIButton reduseAllBtn;


    public GameObject content_3;
    public Transform content_3Transform;
    public TMP_InputField inputField;
    public UIButton selectBtn;

    public UIButton myfriendBtn;
    public GameObject myfriendBtnMask;
    public UIButton listBtn;
    public GameObject listBtnMask;
    public UIButton addfriendBtn;
    public GameObject addfriendBtnMask;

    protected override void AddEventListener()
    {
        base.AddEventListener();
        myfriendBtn.onClick.RemoveAllListeners();
        myfriendBtn.onClick.AddListener(ShowContent_1);
        listBtn.onClick.RemoveAllListeners();
        listBtn.onClick.AddListener(ShowContent_2);
        addfriendBtn.onClick.RemoveAllListeners();
        addfriendBtn.onClick.AddListener(ShowContent_3);

        selectBtn.onClick.RemoveAllListeners();
        selectBtn.onClick.AddListener(() => { });
        addAllBtn.onClick.RemoveAllListeners();
        addAllBtn.onClick.AddListener(() => { });
        reduseAllBtn.onClick.RemoveAllListeners();
        reduseAllBtn.onClick.AddListener(() => { });
    }

    public override void UpdateViewWithArgs(params object[] args)
    {
        base.UpdateViewWithArgs(args);
        ShowContent_1();
    }

    public override void RemoveEventListener()
    {
        base.RemoveEventListener();
    }

    public void ShowContent_1()
    {
        content_1.SetActive(true);
        content_2.SetActive(false);
        content_3.SetActive(false);
        Extensions.ClearChildren(content_1Transform);
    }

    public void ShowContent_2()
    {
        content_1.SetActive(false);
        content_2.SetActive(true);
        content_3.SetActive(false);
        Extensions.ClearChildren(content_2Transform);
    }

    public void ShowContent_3()
    {
        content_1.SetActive(false);
        content_2.SetActive(false);
        content_3.SetActive(true);
        Extensions.ClearChildren(content_3Transform);
    }
}
