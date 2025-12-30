using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using Utils;

public class CoinInfo : MonoBehaviour
{

    public TextMeshProUGUI jingyuanbaotxt;
    public TextMeshProUGUI lingjingtxt;
    void Start()
    {
        UpodatePlayerCoinInfo();
       
    }
    void OnEnable()
    {
         EventCenter.Instance.AddListener(EventMessages.UpdatePlayerMoneyInfo, UpodatePlayerCoinInfo);
    }
    void OnDisable()
    {
         EventCenter.Instance.RemoveListener(EventMessages.UpdatePlayerMoneyInfo, UpodatePlayerCoinInfo);
    }

    public void UpodatePlayerCoinInfo(params object[] args)
    {
        PlayerData playerData = ModuleMgr.Instance.GetModule<PlayerDataModule>().data;

        jingyuanbaotxt.text = Extensions.FormatNumber(playerData.goldIngot);
        lingjingtxt.text = Extensions.FormatNumber(playerData.lingJing);
    }
}
