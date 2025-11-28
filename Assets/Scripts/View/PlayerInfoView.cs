using Module;
using TMPro;
using Utils;
using World.View.UI;

namespace View
{
    public class PlayerMoneyView : BaseView
    {
        public TextMeshProUGUI jinYuanBaoText;
        public TextMeshProUGUI yinQianText;

        protected override void Start()
        {
            base.Start();
            HandleUpdateMoneyInfo();
        }

        protected override void AddEventListener()
        {
            base.AddEventListener();
            EventCenter.Instance.AddListener(EventMessages.UpdatePlayerMoneyInfo,HandleUpdateMoneyInfo);
        }

        public override void RemoveEventListener()
        {
            base.RemoveEventListener();
            EventCenter.Instance.RemoveListener(EventMessages.UpdatePlayerMoneyInfo,HandleUpdateMoneyInfo);
        }

        public void HandleUpdateMoneyInfo(params object[] args)
        {
            jinYuanBaoText.text =  "金元宝:" + ModuleMgr.Instance.GetModule<PlayerDataModule>().data.goldIngot; 
            yinQianText.text = "银钱：" + ModuleMgr.Instance.GetModule<PlayerDataModule>().data.silverCoin;
        }
    }
}
