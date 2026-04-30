using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using Utils;
using View;

public class DeletedConfirmView : BaseView
{
    public UIButton closeBtn;
    public TextMeshProUGUI infotxt;
    public UIButton confirmBtn;
    public PlayerData playerData;
    public FriendData friendData;

    public override void UpdateViewWithArgs(params object[] args)
    {
        base.UpdateViewWithArgs(args);
        friendData = args != null && args.Length > 0 ? args[0] as FriendData : null;
        if (friendData == null)
        {
            Hide();
            return;
        }

        playerData = FriendPlayerDataParser.Parse(
            friendData.user_more,
            friendData.user_id,
            string.IsNullOrEmpty(friendData.display_name) ? friendData.user_name : friendData.display_name);

        if (infotxt != null)
        {
            infotxt.text = $"是否删除：{playerData.playerName}？";
        }
    }

    protected override void AddEventListener()
    {
        base.AddEventListener();

        if (closeBtn != null)
        {
            closeBtn.onClick.RemoveAllListeners();
            closeBtn.onClick.AddListener(Hide);
        }

        if (confirmBtn != null)
        {
            confirmBtn.onClick.RemoveAllListeners();
            confirmBtn.onClick.AddListener(DeleteFriend);
        }
    }

    public override void RemoveEventListener()
    {
        if (closeBtn != null) closeBtn.onClick.RemoveAllListeners();
        if (confirmBtn != null) confirmBtn.onClick.RemoveAllListeners();
        base.RemoveEventListener();
    }

    private void DeleteFriend()
    {
        if (friendData == null)
        {
            UIController.Instance.Show<TipView>("好友数据异常，请刷新后重试。");
            return;
        }

        DeleteFriendRe deleteFriendRe = new DeleteFriendRe(FriendsURL.DeleteFrinedsUrl);
        deleteFriendRe.DeleteCheck(PlayerDataModule.Instance.data.user_id, friendData.user_id, (deleteData) =>
        {
            UIController.Instance.Show<TipView>(deleteData != null ? deleteData.message : "删除失败！");
            if (deleteData != null && deleteData.success)
            {
                UIController.Instance.Show<TipView>(deleteData.message+"。");
                EventCenter.Instance.TriggerEvent(EventMessages.RefreshFriendList);
                Hide();
            }
            else
            {
                Debug.LogWarning($"[DeletedConfirmView] Delete friend failed. friendId={friendData.user_id}");
            }
        });
    }
}
