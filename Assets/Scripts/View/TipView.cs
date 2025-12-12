using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace View
{
    public class TipView : BaseView
    {
        public TextMeshProUGUI title;
        public Image bg;
        private Tween hideTween;  
        private Sequence showSeq; 
        public override void UpdateViewWithArgs(params object[] args)
        {
            title.text = args[0] as string ?? "";

            RectTransform rt = bg.rectTransform;

            // 初始状态：缩小 & 在屏幕下方
            rt.localScale = Vector3.one * 0.3f;
            rt.anchoredPosition = new Vector2(0, -300);  // 下方起始点

            // 动画 Sequence
            Sequence seq = DOTween.Sequence();

            seq.Append(rt.DOScale(1f, 0.6f)
                .SetEase(Ease.OutBack));   // 弹性缩放

            seq.Join(rt.DOAnchorPosY(0, 0.6f)
                .SetEase(Ease.OutCubic));  // 从下飞到中间

            seq.Join(bg.DOFade(1f, 0.3f).From(0));  // 淡入

            seq.SetUpdate(true);  // UI 动画保持可见性

            // 自动隐藏
            hideTween?.Kill();
            hideTween = DOVirtual.DelayedCall(1.2f, Hide).SetUpdate(true);
        }
    }
}