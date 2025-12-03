using Module;
using Module.Data;
using TMPro;
using Utils;

namespace View
{
    public class StoreView : BaseView
    {
        public UIButton closeBtn;
        public TextMeshProUGUI LingJingtxt;
        public TextMeshProUGUI JingYuanBaotxt;
        private PlayerData  playerData;
      
        public override void UpdateViewWithArgs(params object[] args)
        {
            base.UpdateViewWithArgs(args);
            HandleUpdatePlayerInfo();
        }

        protected override void OnHideComplete()
        {
            base.OnHideComplete();
            EventCenter.Instance.TriggerEvent(EventMessages.ShowPlayerInfoViewCartoon);
        }

        protected override void AddEventListener()
        {
            base.AddEventListener();
            EventCenter.Instance.AddListener(EventMessages.UpdatePlayerMoneyInfo ,HandleUpdatePlayerInfo );
            closeBtn.onClick.RemoveAllListeners();
            closeBtn.onClick.AddListener((() =>
            {
                Hide();
            }));
        }
        
        void Update()
        {
        
        }

        private void HandleUpdatePlayerInfo(params object[] args)
        {
            playerData = ModuleMgr.Instance.GetModule<PlayerDataModule>().data;
            LingJingtxt.text = playerData.lingJing.ToString();
            JingYuanBaotxt.text = playerData.goldIngot.ToString();
        }
    }
}
