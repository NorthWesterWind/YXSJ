using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using Module;
using TMPro;
using UnityEngine;
using Utils;

public class DungeonLevelView :BaseView
{
  public UIButton closeBtn;
  public UIButton gameBtn_1;
  public UIButton gameBtn_2;
  public TextMeshProUGUI infotxt_1;
  public TextMeshProUGUI infotxt_2;

  protected override void AddEventListener()
  {
    base.AddEventListener();
    closeBtn.onClick.RemoveAllListeners();
    closeBtn.onClick.AddListener(OnClickCloseBtn);
    gameBtn_1.onClick.RemoveAllListeners();
    gameBtn_1.onClick.AddListener(OnClickGameBtn_1);
    gameBtn_2.onClick.RemoveAllListeners();
    gameBtn_2.onClick.AddListener(OnClickGameBtn_2);
  }

    public override void UpdateViewWithArgs(params object[] args)
    {
        base.UpdateViewWithArgs(args);
        HandleUpdateCountInfo();
    }
    public void HandleUpdateCountInfo(params object[] args)
    {
        infotxt_1.text = "今日剩余挑战次数:" + PlayerDataModule.Instance.data.playLingBaoCount;
        infotxt_2.text = "今日剩余挑战次数:" + PlayerDataModule.Instance.data.playXuanJingCount;
    }

    void OnClickCloseBtn()
    {
        Hide();
    }
    void OnClickGameBtn_1()
    {
       
        UIController.Instance.Show<DungeonLevelView>("LingBao");
    }
    void OnClickGameBtn_2()
    {
        Hide();
        UIController.Instance.Show<DungeonLevelView>("XuanJing");
    }
}
