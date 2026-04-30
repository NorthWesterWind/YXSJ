using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using View;


public class FriendReqItem : MonoBehaviour
{
    public TextMeshProUGUI nametxt;
    public TextMeshProUGUI messagetxt;
    public Image headImage;
    public UIButton confirmBtn;
    public UIButton refuseBtn;
    public RequestData requestData;
    public AssetHandle assetHandle;
    PlayerData playerData;

    public void InitInfo(RequestData data)
    {
        requestData = data;
        playerData = FriendPlayerDataParser.Parse(data.user_more, data.from_user_id, data.from_username);
        if (headImage != null && assetHandle != null) headImage.sprite = assetHandle.Get<Sprite>(playerData.headId.ToString());
        if (nametxt != null) nametxt.text = playerData.playerName;
        if (messagetxt != null) messagetxt.text = data.message;
    }
    void OnEnable()
    {
        confirmBtn.onClick.AddListener(OnConfirmClicked);
        refuseBtn.onClick.AddListener(OnRefuseClicked);
    }
    void OnDisable()
    {
        confirmBtn.onClick.RemoveListener(OnConfirmClicked);
        refuseBtn.onClick.RemoveListener(OnRefuseClicked);
    }

    public void OnConfirmClicked()
    {
        ConSentFriend friendRequest = new ConSentFriend(FriendsURL.ConSentFriendUrl);
        friendRequest.ConSentFriendCheck(PlayerDataModule.Instance.data.user_id, requestData.id, "accept", (data) =>
        {
            if (data != null && data.code == 200)
            {
                UIController.Instance.Show<TipView>(data != null ? data.message + "。" : "处理好友申请失败！");
                EventCenter.Instance.TriggerEvent(EventMessages.RefreshFriendList, this);
                EventCenter.Instance.TriggerEvent(EventMessages.DeleteReqItem, this);
            }
            else
            {
                UIController.Instance.Show<TipView>(data != null ? data.message + "。" : "处理好友申请失败！");
            }
        });
    }
    public void OnRefuseClicked()
    {
        ConSentFriend friendRequest = new ConSentFriend(FriendsURL.ConSentFriendUrl);
        friendRequest.ConSentFriendCheck(PlayerDataModule.Instance.data.user_id, requestData.id, "reject", (data) =>
        {
            if (data != null && data.code == 200)
            {
                UIController.Instance.Show<TipView>(data.message + "。");
                EventCenter.Instance.TriggerEvent(EventMessages.RefreshFriendList, this);
                EventCenter.Instance.TriggerEvent(EventMessages.DeleteReqItem, this);
            }
            else
            {
                UIController.Instance.Show<TipView>(data.message + "。");
            }
        });
    }
}
