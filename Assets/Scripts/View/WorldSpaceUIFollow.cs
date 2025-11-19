using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class WorldSpaceUIFollow : MonoBehaviour
    {
        public Transform target; // 角色头顶挂点
        public Vector3 offset;   // 屏幕偏移（比如往上抬一点）
        public Image fillImage;
        public Canvas  canvas;
        private void Start()
        {
            
        }

        private void LateUpdate()
        {
            if (target == null) return;

            // 直接使用世界坐标
            transform.position = target.position + offset;
            SetLayer();
        }
        public void SetLayer()
        {
            int newOrder = 3000 - Mathf.FloorToInt(transform.localPosition.y);
            canvas.sortingOrder = newOrder;
        }

        public void UpdateFill(float value)
        {
            fillImage.DOFillAmount(Mathf.Min(value, 1), 0.3f);
        }
    }
}