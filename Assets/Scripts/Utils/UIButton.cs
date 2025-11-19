using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

[AddComponentMenu("UI/UI Button", 30)]
public class UIButton : Selectable, IPointerClickHandler, ISubmitHandler
{
    [FormerlySerializedAs("onClick")]
    [SerializeField] private Button.ButtonClickedEvent m_OnClick = new Button.ButtonClickedEvent();
    
    [Header("Enhanced Settings")]
    [Tooltip("点击间隔时间(秒)")]
    [SerializeField] private float m_ClickInterval = 0.3f;
    
    [Tooltip("点击音效")]
    [SerializeField] private AudioClip m_ClickSound;
    
    [Tooltip("音效音量")]
    [Range(0, 1)] [SerializeField] private float m_SoundVolume = 1f;
    
    private float m_LastClickTime;
    
    /// <summary>
    /// 原有Button的点击事件
    /// </summary>
    public Button.ButtonClickedEvent onClick
    {
        get { return m_OnClick; }
        set { m_OnClick = value; }
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
        
        Press();
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