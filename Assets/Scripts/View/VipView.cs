using System;
using Module;
using Utils;
using World.View.UI;

namespace View
{
    public class VipView : BaseView
    {
        public UIButton closeBtn;
        public UIButton purchaseBtn;

        protected override void AddEventListener()
        {
            base.AddEventListener();
            closeBtn.onClick.RemoveAllListeners();
            closeBtn.onClick.AddListener(OnClickCloseBtn);
            purchaseBtn.onClick.RemoveAllListeners();
            purchaseBtn.onClick.AddListener(OnClickPurchaseBtn);
        }

        private void OnClickCloseBtn()
        {
            Hide();
        }

        protected override void OnHideComplete()
        {
            base.OnHideComplete();
            EventCenter.Instance.TriggerEvent(EventMessages.ShowPlayerInfoViewCartoon);
        }

        private void OnClickPurchaseBtn()
        {
            Action callback = () =>
            {
                ModuleMgr.Instance.GetModule<PlayerDataModule>().data.PurchaseVipTime = DateTime.Now.ToString("yyyy/MM/dd");
                UIController.Instance.Show<TipView>("兑换成功！");
            };
            UIController.Instance.Show<ExchangeView>($"是否花费30元兑换30天VIP？", callback );
        }
    }
}
