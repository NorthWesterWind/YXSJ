using System;
using System.Collections.Generic;
using Controller;
using Controller.Structure;
using Module;
using Module.Data;
using Utils;

public class YuanBaoKuangDongCtr : StructureBase
{

    PlayerData playerData;

    void Start()
    {
        playerData = PlayerDataModule.Instance.data;
    }

    void OnEnable()
    {
        EventCenter.Instance.AddListener(EventMessages.JingYuanBaoDead, ConsumeOne);
    }
    public bool CanProduce()
    {
        return playerData.remainCount > 0;
    }

    public void ConsumeOne(params object[] objects)
    {
        if (playerData.remainCount <= 0)
            return;

        playerData.remainCount--;
    }

    void Update()
    {
        if (IsOver12Hours())
        {
            playerData.lastRefrashTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            playerData.remainCount = 30;
            var data = PlayerDataModule.Instance.data.cardUpProgressesList.Find(x => x.developType == CardDevelopType.UpgradeGetYuanBaoLing);
            if (data != null)
                playerData.remainCount += data.level * 10;

        }
    }

    public bool IsOver12Hours()
    {
        if (string.IsNullOrEmpty(playerData.lastRefrashTime))
            return true;
        if (!DateTime.TryParse(playerData.lastRefrashTime, out DateTime lastTime))
            return true;
        TimeSpan span = DateTime.Now - lastTime;
        return span.TotalHours >= 12;
    }

}
