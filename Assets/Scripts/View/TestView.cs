using Utils;
using World.View.UI;

namespace View
{
    public class TestView : BaseView
    {
        public UIButton closeButton;

        protected override void AddEventListener()
        {
            base.AddEventListener();
            closeButton.onClick.AddListener((() => { Hide();  }));
        }

        public override void UpdateViewWithArgs(params object[] args)
        {
            base.UpdateViewWithArgs(args);
            EventCenter.Instance.TriggerEvent(EventMessages.FocusView);
        }

        protected override void OnShow()
        {
            base.OnShow();
           
        }

        protected override void OnHide()
        {
            base.OnHide();
           
         
        }

        protected override void OnHideComplete()
        {
            base.OnHideComplete();
            EventCenter.Instance.TriggerEvent(EventMessages.RestoreFocusView);
        }
    }
}