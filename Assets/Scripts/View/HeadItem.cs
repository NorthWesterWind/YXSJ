using Module;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class HeadItem : MonoBehaviour
{
    public int headId;
    public UIButton btn;
    public Image icon;
    public GameObject selectObj;
    private AssetHandle assetHandle;

    private void Start()
    {
        assetHandle = GetComponent<AssetHandle>();
        if (assetHandle != null && icon != null)
        {
            icon.sprite = assetHandle.Get<Sprite>(headId.ToString());
        }

        if (selectObj != null)
        {
            selectObj.SetActive(PlayerDataModule.Instance.data.headId == headId);
        }

        if (btn == null) return;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() =>
        {
            EventCenter.Instance.TriggerEvent(EventMessages.UpdateHeadItemSelect, headId);
        });
    }

    public void OnUpdateHeadItemSelect(params object[] args)
    {
        if (selectObj == null || args == null || args.Length == 0 || args[0] is not int selectedHeadId) return;
        selectObj.SetActive(selectedHeadId == headId);
    }
    public void UpdateHeadID(params object[] args)
    {
        OnUpdateHeadItemSelect(args);
    }

    void OnEnable()
    {
        EventCenter.Instance.AddListener(EventMessages.UpdateHeadItemSelect, OnUpdateHeadItemSelect);
        EventCenter.Instance.AddListener(EventMessages.UpdateHeadID, OnUpdateHeadItemSelect);
    }

    void OnDisable()
    {
        EventCenter.Instance.RemoveListener(EventMessages.UpdateHeadItemSelect, OnUpdateHeadItemSelect);
        EventCenter.Instance.RemoveListener(EventMessages.UpdateHeadID, OnUpdateHeadItemSelect);
    }
}
