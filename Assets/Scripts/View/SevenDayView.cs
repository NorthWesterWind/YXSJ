using Utils;
using World.View.UI;

namespace View
{
    public class SevenDayView : BaseView
    {
        
        public UIButton closeBtn;

        protected override void AddEventListener()
        {
            base.AddEventListener();
            closeBtn.onClick.RemoveAllListeners();
            closeBtn.onClick.AddListener(OnClickCloseBtn);
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
    }
}
