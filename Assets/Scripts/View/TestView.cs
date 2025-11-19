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
            closeButton.onClick.AddListener((() => { Hide(); }));
        }

        public override void UpdateViewWithArgs(params object[] args)
        {
            base.UpdateViewWithArgs(args);
            EventCenter.Instance.TriggerEvent(EventMessages.FocusView);
        }

        protected override void OnHide()
        {
            EventCenter.Instance.TriggerEvent(EventMessages.RestoreFocusView);
           // base.OnHide();
        }
    }
}