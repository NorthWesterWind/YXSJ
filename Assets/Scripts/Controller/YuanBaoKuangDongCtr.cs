using Controller.Structure;
using Module;
using Module.Data;
using TMPro;
using UnityEngine;

public class YuanBaoKuangDongCtr : StructureBase
{
    PlayerData playerData;
    public TextMeshProUGUI nextRefreshTimeText;
    private float nextRefreshTextUpdateTime;

    private PlayerData PlayerData => playerData ??= PlayerDataModule.Instance.data;

    void Awake()
    {
        playerData = PlayerDataModule.Instance.data;
    }

    void OnEnable()
    {
        playerData ??= PlayerDataModule.Instance.data;
        RefreshNextRefreshTimeText();
    }

    void Update()
    {
        PlayerDataModule.Instance?.RefreshYuanBaoKuangDongDailyCountIfNeeded();
        if (Time.unscaledTime >= nextRefreshTextUpdateTime)
        {
            nextRefreshTextUpdateTime = Time.unscaledTime + 1f;
            RefreshNextRefreshTimeText();
        }
    }

    public bool CanProduce()
    {
        return PlayerData.remainCount > 0;
    }

    public void ConsumeOne(params object[] objects)
    {
        if (PlayerData.remainCount <= 0)
            return;

        PlayerData.remainCount--;
    }

    private void RefreshNextRefreshTimeText()
    {
        if (nextRefreshTimeText == null || PlayerDataModule.Instance == null)
        {
            return;
        }

        nextRefreshTimeText.text = PlayerDataModule.Instance.GetYuanBaoKuangDongNextRefreshText();
    }

}
