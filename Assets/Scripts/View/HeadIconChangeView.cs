using Module;
using UnityEngine;
using Utils;
using View;

public class HeadIconChangeView : BaseView
{
    public UIButton closeBtn;
    public UIButton changeBtn;
    private int selectedHeadId = -1;

    public override void UpdateViewWithArgs(params object[] args)
    {
        base.UpdateViewWithArgs(args);
        selectedHeadId = PlayerDataModule.Instance.data.headId;
        EventCenter.Instance.TriggerEvent(EventMessages.UpdateHeadItemSelect, selectedHeadId);
    }

    protected override void AddEventListener()
    {
        base.AddEventListener();

        closeBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.AddListener(Hide);

        changeBtn.onClick.RemoveAllListeners();
        changeBtn.onClick.AddListener(ChangeHeadIcon);

        EventCenter.Instance.AddListener(EventMessages.UpdateHeadItemSelect, UpdateSelectedHeadId);
    }

    public override void RemoveEventListener()
    {
        EventCenter.Instance.RemoveListener(EventMessages.UpdateHeadItemSelect, UpdateSelectedHeadId);
        base.RemoveEventListener();
    }

    private void UpdateSelectedHeadId(params object[] args)
    {
        if (args == null || args.Length == 0) return;
        if (args[0] is int headId)
        {
            selectedHeadId = headId;
        }
    }

    private void ChangeHeadIcon()
    {
        if (selectedHeadId < 0)
        {
            UIController.Instance.Show<TipView>("请选择头像！");
            return;
        }

        if (PlayerDataModule.Instance.data.headId == selectedHeadId)
        {
            Hide();
            return;
        }

        PlayerDataModule.Instance.data.headId = selectedHeadId;
        PlayerDataModule.Instance.SavePlayerDataAsync();
        PlayerDataModule.Instance.SavePlayerDataToSever();
        EventCenter.Instance.TriggerEvent(EventMessages.UpdateHeadID, selectedHeadId);
        EventCenter.Instance.TriggerEvent(EventMessages.UpdateHeadIcon, selectedHeadId);
        EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerInfo);
        UIController.Instance.Show<TipView>("头像更换成功！");
        Hide();
    }
}
