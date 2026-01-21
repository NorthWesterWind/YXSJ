using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

namespace View
{
    public class StoreView : BaseView
    {
        public UIButton closeBtn;
        public TextMeshProUGUI LingJingtxt;
        public TextMeshProUGUI JingYuanBaotxt;
        private PlayerData  playerData;
        public  GameObject scroll_1;
        public  GameObject scroll_2;
        public UIButton btn1;
        public GameObject mask1;
        public UIButton btn2;
        public GameObject mask2;
      
        public override void UpdateViewWithArgs(params object[] args)
        {
            base.UpdateViewWithArgs(args);
            HandleUpdatePlayerInfo();
ShowScrollView(1);
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

            btn1.onClick.RemoveAllListeners();
            btn1.onClick.AddListener(() => { ShowScrollView(1); });
            btn2.onClick.RemoveAllListeners();
            btn2.onClick.AddListener(() => { ShowScrollView(2); });
        }

        private void HandleUpdatePlayerInfo(params object[] args)
        {
            playerData = PlayerDataModule.Instance.data;
            LingJingtxt.text = Extensions.FormatNumber(playerData.lingJing);
            JingYuanBaotxt.text = Extensions.FormatNumber(playerData.goldIngot) ;
        }

        public void ShowScrollView(int index)
        {
            if (index == 1)
            {
                scroll_1.gameObject.SetActive(true);
                scroll_2.gameObject.SetActive(false);
                mask1.SetActive(false);
                mask2.SetActive(true);
            }
            else
            {
                scroll_1.gameObject.SetActive(false);
                scroll_2.gameObject.SetActive(true);
                mask1.SetActive(true);
                mask2.SetActive(false);
            }
        }
    }
}
