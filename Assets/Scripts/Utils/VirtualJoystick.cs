using Controller.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

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

    public float Horizontal => horizontal ? InputVector.x : 0f;
    public float Vertical => vertical ? InputVector.y : 0f;
    PlayerController playerController;
    PlayerController2D playerController2D;

    private bool isTrial;

    public void Awake()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName.Contains("Trial"))
        {
            isTrial = true;
        }
        if (isTrial)
        {
            playerController2D = GameObject.FindWithTag("Player").GetComponent<PlayerController2D>();
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
    }

    public void OnPointerDown(PointerEventData eventData)
    {
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
        HandleDragInternal(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
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
            playerController2D.SetDir(InputVector);
        }
        else
        {
            playerController.SetDir(InputVector);
        }
        baseRect.gameObject.SetActive(false);
    }
}
