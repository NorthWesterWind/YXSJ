using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;

namespace World.View.UI
{
    /// <summary>
    /// UI面板基类，所有UI面板都应继承此类
    /// </summary>
    public abstract class BaseView : MonoBehaviour
    {
        [Header("Base View Settings")]
        [SerializeField] private bool _hideOnAwake = true;
        [SerializeField] private float _showAnimationTime = 0.3f;
        [SerializeField] private float _hideAnimationTime = 0.2f;
        
        [Tooltip("点击指定背景元素关闭面板（优先级高于点击空白处）")]
        [SerializeField] private Image _closeBackground;
        
        [Header("Popup Settings")]
        [Tooltip("是否作为弹窗加入 UI 栈")]
        [SerializeField] private bool _isPopup = false;

        public bool IsPopup => _isPopup;
        
        // 面板状态事件
        public UnityEvent onShow;
        public UnityEvent onHide;
        public UnityEvent onShowComplete;
        public UnityEvent onHideComplete;
    
        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        private bool _isInitialized = false;
        
        private AssetHandle _assetHandle;
        /// <summary>
        /// 面板是否可见
        /// </summary>
        public bool IsVisible { get; private set; }
    
        /// <summary>
        /// 面板是否正在播放动画
        /// </summary>
        public bool IsInAnimation { get; private set; }
    
        protected virtual void Awake()
        {
            Initialize();
            if (GetComponent<AssetHandle>() != null)
            {
                _assetHandle = GetComponent<AssetHandle>();
            }
        }

        protected virtual void Start()
        {
            AddEventListener();
        }
        

        protected virtual void AddEventListener()
        {
        
        }
        public virtual void RemoveEventListener()
        {
        
        }
        protected virtual void Initialize()
        {
            if (_isInitialized) return;
        
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        
            // // 初始化背景点击事件
            // if (_closeBackground != null )
            // {
            //     var trigger = _closeBackground.gameObject.GetComponent<UIButton>();
            //     if (trigger == null)
            //     {
            //         trigger = _closeBackground.gameObject.AddComponent<UIButton>();
            //     }
            //     trigger.onClick.RemoveAllListeners();
            //     trigger.onClick.AddListener((() =>
            //     {
            //         Hide();
            //     }));
            //   
            // }
        
            if (_hideOnAwake)
            {
                HideImmediate();
            }
            else
            {
                ShowImmediate();
            }
        
            _isInitialized = true;
        }
    
        public virtual void UpdateViewWithArgs(params object[] args)
        {
            // 子类重写，解析参数
        }
    
        /// <summary>
        /// 显示面板（带动画）
        /// </summary>
        public virtual void Show()
        {
            if (IsVisible || IsInAnimation) return;

            gameObject.SetActive(true);
            IsVisible = true;
            IsInAnimation = true;

            onShow?.Invoke();
            OnShow();

            if (_showAnimationTime > 0)
            {
                StartCoroutine(ShowAnimation());
            }
            else
            {
                CompleteShowImmediate();
            }
        }
    
        /// <summary>
        /// 立即显示面板（无动画）
        /// </summary>
        public virtual void ShowImmediate()
        {
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
    
        /// <summary>
        /// 隐藏面板（带动画）
        /// </summary>
        public virtual void Hide()
        {
            if (!IsVisible || IsInAnimation) return;

            IsVisible = false;
            IsInAnimation = true;

            onHide?.Invoke();
            OnHide();

            if (_hideAnimationTime > 0)
            {
                StartCoroutine(HideAnimation());
            }
            else
            {
                CompleteHideImmediate();
            }
        }

    
        /// <summary>
        /// 立即隐藏面板（无动画）
        /// </summary>
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
    
        /// <summary>
        /// 切换面板显示状态
        /// </summary>
        public virtual void Toggle()
        {
            if (IsVisible)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }
    
        /// <summary>
        /// 显示动画协程
        /// </summary>
        private IEnumerator ShowAnimation()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0;
                _canvasGroup.blocksRaycasts = true;
                _canvasGroup.interactable = true;

                float elapsedTime = 0;
                while (elapsedTime < _showAnimationTime)
                {
                    _canvasGroup.alpha = Mathf.Lerp(0, 1, elapsedTime / _showAnimationTime);
                    elapsedTime += Time.unscaledDeltaTime;
                    yield return null;
                }

                _canvasGroup.alpha = 1;
            }

            CompleteShowImmediate();
        }

    
        /// <summary>
        /// 隐藏动画协程
        /// </summary>
        private IEnumerator HideAnimation()
        {
            if (_canvasGroup != null)
            {
                float elapsedTime = 0;
                while (elapsedTime < _hideAnimationTime)
                {
                    _canvasGroup.alpha = Mathf.Lerp(1, 0, elapsedTime / _hideAnimationTime);
                    elapsedTime += Time.unscaledDeltaTime;
                    yield return null;
                }

                _canvasGroup.alpha = 0;
                _canvasGroup.blocksRaycasts = false;
                _canvasGroup.interactable = false;
            }

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
            onHideComplete?.Invoke();
            OnHideComplete();
        }

        /// <summary>
        /// 更新面板数据（子类实现具体逻辑）
        /// </summary>
        public virtual void UpdateView()
        {
            // 子类实现具体的数据更新逻辑
        }
        
        /// <summary>
        /// 检测点击空白处
        /// </summary>
        private void Update()
        {
            // 优先检查是否点击了指定背景
            if (!IsVisible) return;

            if (_closeBackground != null && IsPointerOverGraphic(_closeBackground))
            {
                Hide();
            }
        }
    
        /// <summary>
        /// 检测是否点击在指定UI元素上
        /// </summary>
        private bool IsPointerOverGraphic(Graphic graphic)
        {
            if (graphic == null) return false;
        
            var eventData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };
        
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
        
            foreach (var result in results)
            {
                if (result.gameObject == graphic.gameObject)
                {
                    return true;
                }
            }
        
            return false;
        }
    
        /// <summary>
        /// 检测是否点击在UI上
        /// </summary>
        private bool IsPointerOverUI()
        {
            var eventData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };
        
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
        
            // 排除自己（如果是点击背景关闭的情况）
            foreach (var result in results)
            {
                if (result.gameObject != gameObject && 
                    result.gameObject != _closeBackground?.gameObject)
                {
                    return true;
                }
            }
        
            return false;
        }
        protected virtual void OnDestroy()
        {
            // 清理事件监听
            onShow.RemoveAllListeners();
            onHide.RemoveAllListeners();
            onShowComplete.RemoveAllListeners();
            onHideComplete.RemoveAllListeners();
            RemoveEventListener();
        }
        
        /// <summary>
        /// 当面板开始显示时调用（Show / ShowImmediate）
        /// </summary>
        protected virtual void OnShow() { }

        /// <summary>
        /// 当面板开始隐藏时调用（Hide / HideImmediate）
        /// </summary>
        protected virtual void OnHide() { }

        /// <summary>
        /// 当显示动画播放完毕时调用
        /// </summary>
        protected virtual void OnShowComplete() { }

        /// <summary>
        /// 当隐藏动画播放完毕时调用
        /// </summary>
        protected virtual void OnHideComplete() { }
    }
}