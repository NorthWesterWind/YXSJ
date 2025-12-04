using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Utils
{
    public abstract class BaseView : MonoBehaviour
    {
        [Header("Base View Settings")]
        [SerializeField] private bool _hideOnAwake = true;
        [SerializeField] private float _showAnimationTime = 0.3f;
        [SerializeField] private float _hideAnimationTime = 0.2f;

        [Tooltip("点击指定背景元素关闭面板（优先级高于点击空白处）")]
        [SerializeField] private Image _closeBackground;

        [Header("Popup Settings")]
        [SerializeField] private bool _isPopup = false;
        [SerializeField] private RectTransform _content;
        [SerializeField] private bool _enablePopupScale = true;
        [SerializeField] private AnimationCurve _popupScaleCurve = AnimationCurve.EaseInOut(0, 0.8f, 1, 1f);

        public bool IsPopup => _isPopup;

        // Events
        public UnityEvent onShow;
        public UnityEvent onHide;
        public UnityEvent onShowComplete;
        public UnityEvent onHideComplete;

        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        private bool _isInitialized = false;

        [HideInEditorMode] public AssetHandle _assetHandle;

        public bool IsVisible { get; private set; }
        public bool IsInAnimation { get; private set; }
        public bool IsHiding { get; private set; }

        protected virtual void Awake()
        {
            Initialize();

            if (GetComponent<AssetHandle>() != null)
                _assetHandle = GetComponent<AssetHandle>();
        }

        protected virtual void Start()
        {
            AddEventListener();
        }

        protected virtual void AddEventListener() { }
        public virtual void RemoveEventListener() { }

        protected virtual void Initialize()
        {
            if (_isInitialized) return;

            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            if (_hideOnAwake)
                HideImmediate();
            else
                ShowImmediate();

            _isInitialized = true;
        }

        public virtual void UpdateViewWithArgs(params object[] args) { }

        // ------------------------------
        // Show
        // ------------------------------
        public virtual void Show()
        {
            if (IsVisible || IsInAnimation) return;

            gameObject.SetActive(true);
            IsVisible = true;
            IsInAnimation = true;

            onShow?.Invoke();
            OnShow();

            if (_showAnimationTime > 0)
                StartCoroutine(ShowAnimation());
            else
                CompleteShowImmediate();
        }

        public virtual void ShowImmediate()
        {
            if (IsVisible) return;  // ****** FIXED：移除 IsHiding 限制 ******

            gameObject.SetActive(true);
            IsVisible = true;

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1;
                _canvasGroup.blocksRaycasts = true;
                _canvasGroup.interactable = true;
            }

            onShow?.Invoke();
            onShowComplete?.Invoke();
            OnShow();
        }

        // ------------------------------
        // Hide
        // ------------------------------
        public virtual void Hide()
        {
            if (!IsVisible || IsInAnimation) return;

            IsVisible = false;
            IsInAnimation = true;
            IsHiding = true;  // ****** FIXED：移到这里 ******

            onHide?.Invoke();
            OnHide();

            if (_hideAnimationTime > 0)
                StartCoroutine(HideAnimation());
            else
                CompleteHideImmediate();
        }

        public virtual void HideImmediate()
        {
            gameObject.SetActive(false);
            IsVisible = false;

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0;
                _canvasGroup.blocksRaycasts = false;
                _canvasGroup.interactable = false;
            }

            onHide?.Invoke();
            onHideComplete?.Invoke();
            OnHide();
        }

        public virtual void Toggle()
        {
            if (IsVisible) Hide();
            else Show();
        }

        // ------------------------------
        // Show Animation
        // ------------------------------
        private IEnumerator ShowAnimation()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0;
                _canvasGroup.blocksRaycasts = true;
                _canvasGroup.interactable = true;
            }

            Vector3 defaultScale = Vector3.one;
            Vector3 startScale = Vector3.one * 0.8f;

            if (_content != null && _isPopup)
                _content.localScale = startScale;

            float elapsedTime = 0;

            while (elapsedTime < _showAnimationTime)
            {
                float t = elapsedTime / _showAnimationTime;

                if (_canvasGroup != null)
                    _canvasGroup.alpha = Mathf.Lerp(0, 1, t);

                if (_content != null && _isPopup)
                    _content.localScale = Vector3.one * _popupScaleCurve.Evaluate(t);

                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }

            if (_content != null) _content.localScale = defaultScale;
            if (_canvasGroup != null) _canvasGroup.alpha = 1;

            CompleteShowImmediate();
        }

        // ------------------------------
        // Hide Animation
        // ------------------------------
        private IEnumerator HideAnimation()
        {
            Vector3 defaultScale = Vector3.one;

            float elapsedTime = 0;

            while (elapsedTime < _hideAnimationTime)
            {
                float t = elapsedTime / _hideAnimationTime;

                if (_canvasGroup != null)
                    _canvasGroup.alpha = Mathf.Lerp(1, 0, t);

                if (_content != null && _enablePopupScale && _isPopup)
                    _content.localScale = Vector3.Lerp(defaultScale, defaultScale * 0.85f, t);

                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0;
                _canvasGroup.blocksRaycasts = false;
                _canvasGroup.interactable = false;
            }

            if (_content != null && _enablePopupScale && _isPopup)
                _content.localScale = defaultScale;

            CompleteHideImmediate();
        }

        private void CompleteShowImmediate()
        {
            IsInAnimation = false;
            onShowComplete?.Invoke();
            OnShowComplete();
        }

        private void CompleteHideImmediate()
        {
            gameObject.SetActive(false);
            IsInAnimation = false;
            IsHiding = false;  // ****** FIXED ******
            onHideComplete?.Invoke();
            OnHideComplete();
        }

        public virtual void UpdateView() { }

        private void Update()
        {
            if (!IsVisible) return;

            if (_closeBackground != null && IsPointerOverGraphic(_closeBackground))
                Hide();
        }

        private bool IsPointerOverGraphic(Graphic graphic)
        {
            if (graphic == null) return false;

            var eventData = new PointerEventData(EventSystem.current)
            { position = Input.mousePosition };

            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (var result in results)
                if (result.gameObject == graphic.gameObject)
                    return true;

            return false;
        }

        private bool IsPointerOverUI()
        {
            var eventData = new PointerEventData(EventSystem.current)
            { position = Input.mousePosition };

            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (var result in results)
                if (result.gameObject != gameObject &&
                    result.gameObject != _closeBackground?.gameObject)
                    return true;

            return false;
        }

        protected virtual void OnDestroy()
        {
            onShow.RemoveAllListeners();
            onHide.RemoveAllListeners();
            onShowComplete.RemoveAllListeners();
            onHideComplete.RemoveAllListeners();
            RemoveEventListener();
        }

        protected virtual void OnShow() { }
        protected virtual void OnHide() { }  // ****** FIXED：不再设置 IsHiding ******
        protected virtual void OnShowComplete() { }
        protected virtual void OnHideComplete() { }
    }
}
