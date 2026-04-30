using Controller;
using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using View;

public class PlayerDetailView : BaseView
{
    public UIButton closeBtn;
    public UIButton headIconBtn;
    public UIButton setNameBtn;
    public TMP_InputField nameInputField;
    public TextMeshProUGUI leveltxt;
    public TextMeshProUGUI talenttxt;
    public TextMeshProUGUI tongbitxt;
    public TextMeshProUGUI maptxt;
    public TextMeshProUGUI nametxt;
    public GameObject changeMark;

    private PlayerData data;
    private bool isSelfDetail;

    public override void UpdateViewWithArgs(params object[] args)
    {
        base.UpdateViewWithArgs(args);

        data = args != null && args.Length > 0 ? args[0] as PlayerData : null;
        if (data == null)
        {
            data = PlayerDataModule.Instance.data;
        }

        PlayerData selfData = PlayerDataModule.Instance.data;
        isSelfDetail = data == selfData || data.user_id == selfData.user_id;
        changeMark.SetActive(isSelfDetail);
        RefreshInfo();
    }

    protected override void AddEventListener()
    {
        base.AddEventListener();

        closeBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.AddListener(Hide);

        headIconBtn.onClick.RemoveAllListeners();
        headIconBtn.onClick.AddListener(() =>
        {
            if (!isSelfDetail) return;
            UIController.Instance.Show<HeadIconChangeView>(data);
        });

        setNameBtn.onClick.RemoveAllListeners();
        setNameBtn.onClick.AddListener(ChangePlayerName);

        EventCenter.Instance.AddListener(EventMessages.UpdateHeadIcon, HandleUpdateHeadIcon);
    }

    public override void RemoveEventListener()
    {
        base.RemoveEventListener();
        EventCenter.Instance.RemoveListener(EventMessages.UpdateHeadIcon, HandleUpdateHeadIcon);
    }

    public void HandleUpdateHeadIcon(params object[] args)
    {
        headIconBtn.GetComponent<Image>().sprite = GetComponent<AssetHandle>().Get<Sprite>(data.headId.ToString());
    }

    private void RefreshInfo()
    {
        if (data == null)
        {
            Hide();
            return;
        }

        if (nametxt != null) nametxt.text = string.IsNullOrEmpty(data.playerName) ? data.userAccount : data.playerName;
        if (leveltxt != null) leveltxt.text = data.accountLevel.ToString();
        if (talenttxt != null) talenttxt.text = data.talentLevel.ToString();
        if (tongbitxt != null) tongbitxt.text = Extensions.FormatNumber(data.tongbi);
        if (maptxt != null) maptxt.text = GetMapName(data.currentMapID);
        if (nameInputField != null) nameInputField.text = string.Empty;
        if (nameInputField != null) nameInputField.gameObject.SetActive(isSelfDetail);
        if (setNameBtn != null) setNameBtn.gameObject.SetActive(isSelfDetail);
        headIconBtn.GetComponent<Image>().sprite = GetComponent<AssetHandle>().Get<Sprite>(data.headId.ToString());
    }

    private void ChangePlayerName()
    {
        if (!isSelfDetail) return;
        if (nameInputField == null) return;

        string playerName = nameInputField.text.Trim();
        if (string.IsNullOrEmpty(playerName))
        {
            UIController.Instance.Show<TipView>("昵称不能为空!");
            return;
        }

        if (!Extensions.IsAllChinese(playerName))
        {
            UIController.Instance.Show<TipView>("昵称只允许输入中文!");
            return;
        }

        if (playerName.Length > 6)
        {
            UIController.Instance.Show<TipView>("昵称不能超过6位中文字符。");
            return;
        }

        LoginUtil.Instance.CheckBlockedWords(playerName, (blockedData) =>
        {
            if (blockedData == null || blockedData.code != 200 || blockedData.data == null)
            {
                UIController.Instance.Show<TipView>("网络状态异常!");
                return;
            }

            if (blockedData.data.has_sensitive)
            {
                UIController.Instance.Show<TipView>($"输入内容包含敏感字符，请修改！");
                Debug.LogWarning($"昵称 '{playerName}' 包含敏感词'{blockedData.data.hit_word}'，原因类型: {blockedData.data.reason_type}，具体原因: {blockedData.data.reason}");
                return;
            }

            PlayerData selfData = PlayerDataModule.Instance.data;
            selfData.playerName = playerName;
            data.playerName = playerName;
            nameInputField.text = string.Empty;
            RefreshInfo();
            PlayerDataModule.Instance.SavePlayerDataAsync();
            PlayerDataModule.Instance.SavePlayerDataToSever();
            EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerInfo);
            UIController.Instance.Show<TipView>("修改成功!");
        });
    }

    private string GetMapName(int mapId)
    {
        if (DataController.Instance != null
            && DataController.Instance.mapDataDic != null
            && DataController.Instance.mapDataDic.TryGetValue(mapId, out var mapData)
            && mapData != null)
        {
            return mapData.name;
        }

        return "未知地图";
    }
}
