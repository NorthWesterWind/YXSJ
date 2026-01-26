using Controller.Player;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class DynamicVirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Core Settings")]
    [SerializeField] private float radius = 100f;      // 摇杆移动半径
    [SerializeField, Range(0f, 1f)] private float deadZone = 0.2f; // 死区阈值
    [SerializeField] private bool horizontal = true;  // 启用水平轴
    [SerializeField] private bool vertical = true;    // 启用垂直轴

    [Header("Components")]
    [SerializeField] private RectTransform baseRect;    // 摇杆底座
    [SerializeField] private RectTransform handleRect;  // 摇杆手柄

    public Vector2 InputVector { get; private set; }

    private Canvas canvas;
    private Camera uiCamera;
    private RectTransform canvasRect;
    private bool isDragging;

    // 便捷访问
    public float Horizontal => horizontal ? InputVector.x : 0f;
    public float Vertical => vertical ? InputVector.y : 0f;

    public PlayerController player;
    private void Awake()
    {

        if (!player)
                player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("DynamicVirtualJoystick: 找不到父 Canvas");
            return;
        }

        canvasRect = canvas.transform as RectTransform;
        uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        HideJoystick();
    }

    // ================= Pointer =================
    public void OnPointerDown(PointerEventData eventData)
    {
        // 如果点击在 UI（除自己底座手柄外）则忽略
        if (IsPointerOverUIExcludingJoystick(eventData))
            return;

        ShowJoystickAt(eventData.position);
        isDragging = true;
        HandleDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        HandleDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        HideJoystick();
    }

    // ================= Core =================
    private bool IsPointerOverUIExcludingJoystick(PointerEventData eventData)
    {
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var r in results)
        {
            if (r.gameObject == baseRect.gameObject || r.gameObject == handleRect.gameObject)
                continue;

            if (r.gameObject.GetComponent<UnityEngine.UI.Graphic>() != null)
                return true;
        }
        return false;
    }

    private void ShowJoystickAt(Vector2 screenPos)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, uiCamera, out Vector2 localPos);

        baseRect.anchoredPosition = localPos;
        handleRect.anchoredPosition = Vector2.zero;

        baseRect.gameObject.SetActive(true);
        handleRect.gameObject.SetActive(true);
    }

    private void HandleDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(baseRect, eventData.position, uiCamera, out Vector2 localPoint);

        ProcessInput(localPoint);
        UpdateHandle();
    }

    private void ProcessInput(Vector2 rawInput)
    {
        if (!horizontal) rawInput.x = 0f;
        if (!vertical) rawInput.y = 0f;

        Vector2 clamped = Vector2.ClampMagnitude(rawInput, radius);
        Vector2 normalized = clamped / radius;

        if (normalized.sqrMagnitude < deadZone * deadZone)
            normalized = Vector2.zero;

        InputVector = normalized;
        player.SetDir(InputVector);
    }

    private void UpdateHandle()
    {
        handleRect.anchoredPosition = InputVector * radius;
    }

    private void HideJoystick()
    {
        InputVector = Vector2.zero;
        handleRect.anchoredPosition = Vector2.zero;

        baseRect.gameObject.SetActive(false);
        handleRect.gameObject.SetActive(false);
    }

    // 可以外部手动拖动
    public void HandleExternalDrag(PointerEventData eventData)
    {
        HandleDrag(eventData);
    }
}
