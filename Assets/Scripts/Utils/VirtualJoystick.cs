using Controller.Player;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;

public class VirtualJoystick : MonoBehaviour,
    IPointerDownHandler,
    IDragHandler,
    IPointerUpHandler
{
    [Header("Core Settings")]
    [SerializeField] private float radius = 100f;
    [SerializeField, Range(0f, 1f)] private float deadZone = 0.2f;
    [SerializeField] private bool horizontal = true;
    [SerializeField] private bool vertical = true;

    [Header("Components")]
    [SerializeField] private RectTransform handle;

    public Vector2 InputVector { get; private set; }

    [SerializeField] private RectTransform baseRect;
    private Vector2 initialPosition;
    private Canvas canvas;
    private Camera uiCamera;
    private bool ignoreCurrentPointer;
    private bool canInput = true;

    public float Horizontal => horizontal ? InputVector.x : 0f;
    public float Vertical => vertical ? InputVector.y : 0f;
    PlayerController playerController;
    public PlayerController2D playerController2D;

    public bool isTrial;

    public void Awake()
    {
        if (isTrial)
        {

        }
        else
        {
            playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        }
        initialPosition = baseRect.anchoredPosition;

        canvas = GetComponentInParent<Canvas>();
        uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas.worldCamera;
        baseRect.gameObject.SetActive(false);

        EventCenter.Instance.AddListener(EventMessages.LockJoystickInput, CloseInput);
        EventCenter.Instance.AddListener(EventMessages.UnlockJoystickInput, OpenInput);
    }

    void OnEnable()
    {
        ResetJoystick();
        ignoreCurrentPointer = false;
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

        if (!isTrial && TryPassThroughToUnderlyingClickable(eventData))
        {
            ignoreCurrentPointer = true;
            return;
        }

        ignoreCurrentPointer = false;
        baseRect.gameObject.SetActive(true);
        MoveBaseToTouch(eventData);
        HandleDragInternal(eventData);
    }
    private void MoveBaseToTouch(PointerEventData eventData)
    {
        RectTransform parentRect = baseRect.parent as RectTransform;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            eventData.position,
            uiCamera,
            out Vector2 localPos
        );

        baseRect.anchoredPosition = localPos;
    }


    public void OnDrag(PointerEventData eventData)
    {
        if (!canInput || ignoreCurrentPointer) return;
        HandleDragInternal(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (ignoreCurrentPointer)
        {
            ignoreCurrentPointer = false;
            return;
        }
        ResetJoystick();
    }


    private void HandleDragInternal(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            baseRect,
            eventData.position,
            uiCamera,
            out Vector2 localPoint
        );

        ProcessInput(localPoint);
        UpdateHandlePosition();
    }

    private void ProcessInput(Vector2 rawInput)
    {
        if (!horizontal) rawInput.x = 0;
        if (!vertical) rawInput.y = 0;

        Vector2 clamped = Vector2.ClampMagnitude(rawInput, radius);
        InputVector = clamped / radius;

        if (InputVector.sqrMagnitude < deadZone * deadZone)
            InputVector = Vector2.zero;
        if (isTrial)
        {
            playerController2D.SetDir(InputVector);
        }
        else
        {
            playerController.SetDir(InputVector);
        }

    }

    private void UpdateHandlePosition()
    {
        handle.anchoredPosition = InputVector * radius;
    }

    public void ResetJoystick()
    {
        InputVector = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
        baseRect.anchoredPosition = initialPosition;
        if (isTrial)
        {
            playerController2D.SetDir(Vector2.zero);
        }
        else
        {
            playerController.SetDir(InputVector);
        }
        baseRect.gameObject.SetActive(false);
    }

    private bool TryPassThroughToUnderlyingClickable(PointerEventData eventData)
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        var raycastData = new PointerEventData(EventSystem.current)
        {
            position = eventData.position,
            button = eventData.button
        };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(raycastData, results);

        foreach (var result in results)
        {
            if (result.gameObject == null)
            {
                continue;
            }

            // Skip joystick root and its child graphics.
            var target = result.gameObject.transform;
            if (target == transform || target.IsChildOf(transform))
            {
                continue;
            }

            var uiButton = result.gameObject.GetComponentInParent<UIButton>();
            if (uiButton != null && uiButton.IsActive() && uiButton.IsInteractable())
            {
                uiButton.SimulateClick();
                return true;
            }

            var button = result.gameObject.GetComponentInParent<Button>();
            if (button != null && button.IsActive() && button.IsInteractable())
            {
                button.onClick.Invoke();
                return true;
            }
        }

        return false;
    }

    private void CloseInput(params object[] args)
    {
        canInput = false;
        ignoreCurrentPointer = false;
        ResetJoystick();
    }

    private void OpenInput(params object[] args)
    {
        canInput = true;
    }
}
