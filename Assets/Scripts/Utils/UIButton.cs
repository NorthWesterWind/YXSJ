using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.Events;

[AddComponentMenu("UI/UI Button", 30)]
public class UIButton : Selectable, IPointerClickHandler, ISubmitHandler, IPointerDownHandler, IPointerUpHandler, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [System.Serializable]
    public class PointerDataEvent : UnityEvent<PointerEventData> { }

    [FormerlySerializedAs("onClick")]
    [SerializeField] private Button.ButtonClickedEvent m_OnClick = new Button.ButtonClickedEvent();

    [Header("Pointer Events")]
    [SerializeField] private PointerDataEvent m_OnPointerDown = new PointerDataEvent();
    [SerializeField] private PointerDataEvent m_OnPointerUp = new PointerDataEvent();
    [SerializeField] private PointerDataEvent m_OnDrag = new PointerDataEvent();
    
    [Header("Enhanced Settings")]
    [Tooltip("点击间隔时间(秒)")]
    [SerializeField] private float m_ClickInterval = 0.3f;
    
    [Tooltip("点击音效")]
    [SerializeField] private AudioClip m_ClickSound;
    
    [Tooltip("音效音量")]
    [Range(0, 1)] [SerializeField] private float m_SoundVolume = 1f;
    [SerializeField] private float m_ClickCancelDragThreshold = 15f;

    private float m_LastClickTime;
    private ScrollRect m_ParentScrollRect;
    private Vector2 m_PointerDownPosition;
    private bool m_SuppressClick;
    
    /// <summary>
    /// 原有Button的点击事件
    /// </summary>
    public Button.ButtonClickedEvent onClick
    {
        get { return m_OnClick; }
        set { m_OnClick = value; }
    }

    public PointerDataEvent onPointerDownEvent
    {
        get { return m_OnPointerDown; }
        set { m_OnPointerDown = value; }
    }

    public PointerDataEvent onPointerUpEvent
    {
        get { return m_OnPointerUp; }
        set { m_OnPointerUp = value; }
    }

    public PointerDataEvent onDragEvent
    {
        get { return m_OnDrag; }
        set { m_OnDrag = value; }
    }
    
    /// <summary>
    /// 点击间隔时间(秒)
    /// </summary>
    public float clickInterval
    {
        get { return m_ClickInterval; }
        set { m_ClickInterval = Mathf.Max(0, value); }
    }
    
    protected UIButton()
    {
        // 保持与Button相同的初始化
    }
    
    protected override void Awake()
    {
        base.Awake();
        CacheParentScrollRect();
    }

    private ScrollRect GetParentScrollRect()
    {
        if (m_ParentScrollRect == null)
        {
            CacheParentScrollRect();
        }

        return m_ParentScrollRect;
    }

    private void CacheParentScrollRect()
    {
        m_ParentScrollRect = GetComponentInParent<ScrollRect>();
    }
    
    private void PlayClickSound()
    {
        if(m_ClickSound == null)
            return;
       // AudioSourceController.Instance.PlayUISound(m_ClickSound, m_SoundVolume);
    }
    
    private bool AllowClick()
    {
        return Time.unscaledTime - m_LastClickTime >= m_ClickInterval;
    }
    
    private void Press()
    {
        if (!IsActive() || !IsInteractable())
            return;
        
        if (!AllowClick())
            return;
        
        m_LastClickTime = Time.unscaledTime;
        
        // 播放音效
        PlayClickSound();
        
        // 触发点击事件
        m_OnClick.Invoke();
    }
    
    // 处理鼠标/触摸点击
    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (ShouldSuppressClick(eventData))
            return;

        Press();
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        if (!IsActive() || !IsInteractable())
            return;

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        m_PointerDownPosition = eventData.position;
        m_SuppressClick = false;
        m_OnPointerDown.Invoke(eventData);
    }

    public virtual void OnInitializePotentialDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        GetParentScrollRect()?.OnInitializePotentialDrag(eventData);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);

        if (!IsActive() || !IsInteractable())
            return;

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        m_OnPointerUp.Invoke(eventData);
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        if (!IsActive() || !IsInteractable())
            return;

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        m_SuppressClick = true;
        GetParentScrollRect()?.OnBeginDrag(eventData);
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        if (!IsActive() || !IsInteractable())
            return;

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (!m_SuppressClick && HasExceededDragThreshold(eventData.position))
        {
            m_SuppressClick = true;
        }

        m_OnDrag.Invoke(eventData);
        GetParentScrollRect()?.OnDrag(eventData);
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        if (!IsActive() || !IsInteractable())
            return;

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        GetParentScrollRect()?.OnEndDrag(eventData);
    }

    private bool ShouldSuppressClick(PointerEventData eventData)
    {
        if (m_SuppressClick || eventData.dragging)
            return true;

        return HasExceededDragThreshold(eventData.position);
    }

    private bool HasExceededDragThreshold(Vector2 currentPosition)
    {
        float threshold = Mathf.Max(0f, m_ClickCancelDragThreshold);
        if (EventSystem.current != null)
        {
            threshold = Mathf.Max(threshold, EventSystem.current.pixelDragThreshold);
        }

        return (currentPosition - m_PointerDownPosition).sqrMagnitude > threshold * threshold;
    }
    
    // 处理键盘/手柄提交
    public virtual void OnSubmit(BaseEventData eventData)
    {
        Press();
        
        // 保持选中状态
        if (IsActive() && IsInteractable())
        {
            DoStateTransition(SelectionState.Pressed, false);
            StartCoroutine(OnFinishSubmit());
        }
    }
    
    private IEnumerator OnFinishSubmit()
    {
        var fadeTime = colors.fadeDuration;
        var elapsedTime = 0f;
        
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        
        DoStateTransition(currentSelectionState, false);
    }
    
    // 模拟按钮点击
    public virtual void SimulateClick()
    {
        Press();
    }
    
    // 重置点击计时器
    public void ResetClickTimer()
    {
        m_LastClickTime = -m_ClickInterval;
    }
}
