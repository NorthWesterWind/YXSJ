using Controller.Structure;
using Module;
using Module.Data;
using TMPro;
using UnityEngine;

public class YuanBaoKuangDongCtr : StructureBase
{
    private PlayerData playerData;
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
        PlayerDataModule.Instance?.RefreshYuanBaoKuangDongDailyCountIfNeeded();
        RefreshStatusText();
    }

    void Update()
    {
        PlayerDataModule.Instance?.RefreshYuanBaoKuangDongDailyCountIfNeeded();
        if (Time.unscaledTime >= nextRefreshTextUpdateTime)
        {
            nextRefreshTextUpdateTime = Time.unscaledTime + 1f;
            RefreshStatusText();
        }
    }

    public bool CanProduce()
    {
        return PlayerDataModule.Instance != null &&
               PlayerDataModule.Instance.GetYuanBaoKuangDongRemainingCount(PlayerData.currentMapID) > 0;
    }

    public void ConsumeOne(params object[] objects)
    {
        if (PlayerDataModule.Instance == null)
        {
            return;
        }

        PlayerDataModule.Instance.TryConsumeYuanBaoKuangDongSpawnQuota(PlayerData.currentMapID);
        RefreshStatusText();
    }

    private void RefreshStatusText()
    {
        if (nextRefreshTimeText == null || PlayerDataModule.Instance == null)
        {
            return;
        }

        int remainCount = PlayerDataModule.Instance.GetYuanBaoKuangDongRemainingCount(PlayerData.currentMapID);
        nextRefreshTimeText.text =
            $"{PlayerDataModule.Instance.GetYuanBaoKuangDongNextRefreshText()}";
    }
}
