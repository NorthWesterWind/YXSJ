using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class WorldSpaceUIFollow : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset;
        public Image fillImage;
        public Canvas canvas;

        private Camera _cachedCamera;

        private void Awake()
        {
            _cachedCamera = Camera.main;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            if (_cachedCamera == null)
            {
                _cachedCamera = Camera.main;
                if (_cachedCamera == null)
                {
                    return;
                }
            }

            transform.position = _cachedCamera.WorldToScreenPoint(target.position + offset);
            SetLayer();
        }

        public void SetLayer()
        {
            if (canvas == null)
            {
                return;
            }

            int newOrder = 3000 - Mathf.FloorToInt(transform.localPosition.y);
            canvas.sortingOrder = newOrder;
        }

        public void UpdateFill(float value)
        {
            if (fillImage == null)
            {
                return;
            }

            fillImage.DOFillAmount(Mathf.Min(value, 1f), 0.3f);
        }
    }
}
