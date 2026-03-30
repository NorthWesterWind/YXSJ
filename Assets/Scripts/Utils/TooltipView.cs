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
            infotxt.text = desc + "。";

            RectTransform itemRt = targetButton;
            Canvas canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
            RectTransform canvasRt = canvas.transform as RectTransform;
            Vector3 worldPos = itemRt.TransformPoint(itemRt.rect.center);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRt,
                RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, worldPos),
                canvas.worldCamera,
                out Vector2 canvasLocalPos
            );
            Vector2 pos = canvasLocalPos + new Vector2(0, 40);
            content.anchoredPosition = pos;
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
