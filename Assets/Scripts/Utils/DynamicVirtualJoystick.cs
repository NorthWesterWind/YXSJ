using Controller.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils;

[RequireComponent(typeof(RectTransform))]
public class DynamicVirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Core Settings")]
    [SerializeField] private float radius = 100f;
    [SerializeField, Range(0f, 1f)] private float deadZone = 0.2f;
    [SerializeField] private bool horizontal = true;
    [SerializeField] private bool vertical = true;

    [Header("Components")]
    [SerializeField] private RectTransform baseRect;
    [SerializeField] private RectTransform handleRect;

    public Vector2 InputVector { get; private set; }

    private Canvas canvas;
    private Camera uiCamera;
    private RectTransform canvasRect;
    private bool isDragging;
    private bool canInput = true;

    public float Horizontal => horizontal ? InputVector.x : 0f;
    public float Vertical => vertical ? InputVector.y : 0f;

    public PlayerController player;

    private void Awake()
    {
        if (!player)
        {
            player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        }

        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("DynamicVirtualJoystick: cannot find parent Canvas.");
            return;
        }

        canvasRect = canvas.transform as RectTransform;
        uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        EventCenter.Instance.AddListener(EventMessages.LockJoystickInput, CloseInput);
        EventCenter.Instance.AddListener(EventMessages.UnlockJoystickInput, OpenInput);
        HideJoystick();
    }

    private void OnDestroy()
    {
        EventCenter.Instance.RemoveListener(EventMessages.LockJoystickInput, CloseInput);
        EventCenter.Instance.RemoveListener(EventMessages.UnlockJoystickInput, OpenInput);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!canInput)
        {
            return;
        }

        if (IsPointerOverUIExcludingJoystick(eventData))
        {
            return;
        }

        ShowJoystickAt(eventData.position);
        isDragging = true;
        HandleDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!canInput || !isDragging)
        {
            return;
        }

        HandleDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        HideJoystick();
    }

    private bool IsPointerOverUIExcludingJoystick(PointerEventData eventData)
    {
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            if (result.gameObject == baseRect.gameObject || result.gameObject == handleRect.gameObject)
            {
                continue;
            }

            if (result.gameObject.GetComponent<UnityEngine.UI.Graphic>() != null)
            {
                return true;
            }
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
        if (!horizontal)
        {
            rawInput.x = 0f;
        }

        if (!vertical)
        {
            rawInput.y = 0f;
        }

        Vector2 clamped = Vector2.ClampMagnitude(rawInput, radius);
        Vector2 normalized = clamped / radius;

        if (normalized.sqrMagnitude < deadZone * deadZone)
        {
            normalized = Vector2.zero;
        }

        InputVector = normalized;
        if (player != null)
        {
            player.SetDir(InputVector);
        }
    }

    private void UpdateHandle()
    {
        handleRect.anchoredPosition = InputVector * radius;
    }

    private void HideJoystick()
    {
        InputVector = Vector2.zero;
        isDragging = false;

        if (handleRect != null)
        {
            handleRect.anchoredPosition = Vector2.zero;
            handleRect.gameObject.SetActive(false);
        }

        if (baseRect != null)
        {
            baseRect.gameObject.SetActive(false);
        }

        if (player != null)
        {
            player.SetDir(Vector2.zero);
        }
    }

    public void HandleExternalDrag(PointerEventData eventData)
    {
        HandleDrag(eventData);
    }

    private void CloseInput(params object[] args)
    {
        canInput = false;
        HideJoystick();
    }

    private void OpenInput(params object[] args)
    {
        canInput = true;
    }
}
