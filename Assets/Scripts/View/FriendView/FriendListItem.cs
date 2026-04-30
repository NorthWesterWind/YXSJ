using System.Collections;
using System.Linq;
using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using View;

public class FriendListItem : MonoBehaviour
{
    private AssetHandle assetHandle;
    public Image headIcon;
    public TextMeshProUGUI nametxt;
    public TextMeshProUGUI leveltxt;
    public UIButton chatBtn;
    public UIButton deleteBtn;
    public FriendData friendData;
    public PlayerData playerData;
    public GameObject redPoint;
    public UIButton infoBtn;
    private bool suppressUnreadRedPoint;

    public void InitInfo(FriendData friendData)
    {
        this.friendData = friendData;
        assetHandle = GetComponent<AssetHandle>();
        playerData = FriendPlayerDataParser.Parse(
            friendData.user_more,
            friendData.user_id,
            string.IsNullOrEmpty(friendData.display_name) ? friendData.user_name : friendData.display_name);

        if (nametxt != null)
        {
            nametxt.text = playerData.playerName;
            if (!string.IsNullOrEmpty(friendData.remark_name as string))
            {
                nametxt.text += "(" + friendData.remark_name + ")";
            }
        }

        if (leveltxt != null) leveltxt.text = "账号等级：" + playerData.accountLevel;
        if (headIcon != null && assetHandle != null) headIcon.sprite = assetHandle.Get<Sprite>(playerData.headId.ToString());

        BindButtons();
        StartUnreadCheck();
    }

    private void OnEnable()
    {
        BindButtons();
        if (friendData != null) StartUnreadCheck();
    }

    private void OnDisable()
    {
        if (deleteBtn != null) deleteBtn.onClick.RemoveAllListeners();
        if (chatBtn != null) chatBtn.onClick.RemoveAllListeners();
        if (infoBtn != null) infoBtn.onClick.RemoveAllListeners();
        StopUnreadCheck();
    }

    private void BindButtons()
    {
        if (chatBtn != null)
        {
            chatBtn.onClick.RemoveAllListeners();
            chatBtn.onClick.AddListener(() =>
            {
                if (playerData == null) return;
                ClearUnreadRedPointImmediately();
                UIController.Instance.Show<ChatView>(playerData);
            });
        }

        if (deleteBtn != null)
        {
            deleteBtn.onClick.RemoveAllListeners();
            deleteBtn.onClick.AddListener(() =>
            {
                if (friendData == null)
                {
                    UIController.Instance.Show<TipView>("好友数据异常，请刷新后重试。");
                    return;
                }

                UIController.Instance.Show<DeletedConfirmView>(friendData);
            });
        }

        if (infoBtn != null)
        {
            infoBtn.onClick.RemoveAllListeners();
            infoBtn.onClick.AddListener(() =>
            {
                if (playerData == null) return;
                UIController.Instance.Show<PlayerDetailView>(playerData);
            });
        }
    }

    private Coroutine unreadCoroutine;

    public void StartUnreadCheck()
    {
        if (!gameObject.activeInHierarchy) return;
        if (unreadCoroutine != null) return;
        unreadCoroutine = StartCoroutine(UnreadLoop());
    }

    private IEnumerator UnreadLoop()
    {
        while (true)
        {
            RequestUnread();
            yield return new WaitForSeconds(5f);
        }
    }

    private bool isRequesting;

    private void RequestUnread()
    {
        if (isRequesting || playerData == null) return;

        isRequesting = true;
        GetUnreadCount getUnreadCount = new GetUnreadCount(FriendsURL.GetUnreadCountUrl);
        getUnreadCount.LoadMsg(PlayerDataModule.Instance.data.user_id, (data) =>
        {
            UnreadSender unreadSender = data?.data?.unread_by_sender?.FirstOrDefault(x => x.sender_id == playerData.user_id);
            bool hasUnread = data != null
                             && data.data != null
                             && data.data.total_unread > 0
                             && unreadSender != null
                             && unreadSender.count > 0;

            if (!hasUnread)
            {
                suppressUnreadRedPoint = false;
            }

            SetUnreadRedPoint(hasUnread && !suppressUnreadRedPoint);
            isRequesting = false;
        });
    }

    private void ClearUnreadRedPointImmediately()
    {
        suppressUnreadRedPoint = true;
        SetUnreadRedPoint(false);
        EventCenter.Instance.TriggerEvent(EventMessages.RefreshExpeditionFriendRedPoint);
    }

    private void SetUnreadRedPoint(bool isShow)
    {
        if (redPoint != null) redPoint.SetActive(isShow);
    }

    private void StopUnreadCheck()
    {
        if (unreadCoroutine != null)
        {
            StopCoroutine(unreadCoroutine);
            unreadCoroutine = null;
        }

        isRequesting = false;
    }
}
