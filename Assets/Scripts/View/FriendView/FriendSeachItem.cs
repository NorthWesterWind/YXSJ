using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using View;
using World.View.UI;

public class FriendSeachItem : MonoBehaviour
{
    public TextMeshProUGUI nametxt;
    public Image headIcon;
    public UIButton addBtn;
    public UIButton chatBtn;
    public RankList rankData;
    public StrangerData strangerData;
    public FriendedData friendedData;

    public PlayerData aimData;
    public UIButton headIconBtn;
    private bool isAddingFriend;

    public void InitInfo(RankList rankData)
    {
        if (rankData == null) return;
        chatBtn.gameObject.SetActive(false);
        addBtn.gameObject.SetActive(true);
        this.rankData = rankData;
        aimData = FriendPlayerDataParser.Parse(rankData.user_more, rankData.id, rankData.userName, rankData.userHead);
        nametxt.text = string.IsNullOrEmpty(aimData.playerName) ? rankData.userName : aimData.playerName;
        headIcon.sprite = GetComponent<AssetHandle>().Get<Sprite>(aimData.headId.ToString());

        // 确保数据初始化后再绑定按钮
        BindButton();
    }

    public void InitInfo_1(StrangerData strangerData)
    {
        if (strangerData == null) return;
        chatBtn.gameObject.SetActive(false);
        addBtn.gameObject.SetActive(true);
        this.strangerData = strangerData;
        aimData = FriendPlayerDataParser.Parse(strangerData.user_more, strangerData.user_id, string.IsNullOrEmpty(strangerData.user_rolename) ? strangerData.user_name : strangerData.user_rolename);
        nametxt.text = aimData.playerName;
        headIcon.sprite = GetComponent<AssetHandle>().Get<Sprite>(aimData.headId.ToString());

        // 确保数据初始化后再绑定按钮
        BindButton();
    }

    public void InitInfo_2(FriendedData friendedData)
    {
        if (friendedData == null) return;
        chatBtn.gameObject.SetActive(true);
        addBtn.gameObject.SetActive(false);
        this.friendedData = friendedData;
        aimData = FriendPlayerDataParser.Parse(friendedData.user_more, friendedData.user_id, string.IsNullOrEmpty(friendedData.user_rolename) ? friendedData.user_name : friendedData.user_rolename);
        nametxt.text = aimData.playerName;
        headIcon.sprite = GetComponent<AssetHandle>().Get<Sprite>(aimData.headId.ToString());

        // 确保数据初始化后再绑定按钮
        BindButton();
    }

    private void BindButton()
    {
        // 先移除旧的监听器，避免重复绑定
        addBtn.onClick.RemoveAllListeners();
        addBtn.onClick.AddListener(OnAddBtnClick);

        chatBtn.onClick.RemoveAllListeners();
        chatBtn.onClick.AddListener(() => { UIController.Instance.Show<ChatView>(aimData); });

        headIconBtn.onClick.RemoveAllListeners();
        headIconBtn.onClick.AddListener(() => { UIController.Instance.Show<PlayerDetailView>(aimData); });
    }

    void OnEnable()
    {

    }

    void OnDisable()
    {
        if (addBtn != null)
        {
            addBtn.onClick.RemoveAllListeners();
        }

        if (chatBtn != null)
        {
            chatBtn.onClick.RemoveAllListeners();
        }
    }

    private void OnAddBtnClick()
    {
        if (isAddingFriend || aimData == null) return;

        isAddingFriend = true;
        if (addBtn != null) addBtn.interactable = false;
        AddFriends addFriends = new AddFriends(FriendsURL.AddFriendsUrl);
        addFriends.AddFriendsCheck(PlayerDataModule.Instance.data.user_id, aimData.user_id, (AddFriendsData data) =>
        {
            if (data != null && data.success)
            {
                UIController.Instance.Show<TipView>("好友请求发送成功！");
                Destroy(gameObject);
            }
            else
            {
                isAddingFriend = false;
                if (addBtn != null) addBtn.interactable = true;
                UIController.Instance.Show<TipView>(data != null ? "已发送过好友申请。" : "好友请求发送失败！");
            }
        });
    }
}
