using System;
using TMPro;
using Utils;

namespace View
{
    public class ExchangeView : BaseView
    {
        public TextMeshProUGUI infotxt;
        public UIButton confirmBtn;
        public UIButton closeBtn;
        private Action callback;
        
        protected override void AddEventListener()
        {
            base.AddEventListener();
            closeBtn.onClick.AddListener((() =>
            {
                Hide();
            }));
            confirmBtn.onClick.AddListener((() =>
            {
                callback?.Invoke();
                Hide();
            }));
        }

        public override void UpdateViewWithArgs(params object[] args)
        {
            base.UpdateViewWithArgs(args);
            string info = args[0] as string;
            infotxt.text = info;
            callback =  args[1] as Action;
        }
    }
}