using TMPro;
using UnityEngine;

namespace Utils
{
    public class TooltipView : BaseView
    {
        public TextMeshProUGUI tiptxt;
        public TextMeshProUGUI infotxt;
        private RectTransform targetButton;
        public UIButton btn;
        public RectTransform content;
        public override void UpdateViewWithArgs(params object[] args)
        {
            base.UpdateViewWithArgs(args);
            string name = args[0] as string;
            string desc = args[1] as string;
            targetButton = args[2] as RectTransform;
            
            tiptxt.text = name;
            infotxt.text = desc;
            content.anchoredPosition = new Vector2( targetButton.anchoredPosition.x, targetButton.anchoredPosition.y + 150f );
        }

        protected override void AddEventListener()
        {
            base.AddEventListener();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener((() =>
            {
                Hide();
            }));
        }
    }
    
}
