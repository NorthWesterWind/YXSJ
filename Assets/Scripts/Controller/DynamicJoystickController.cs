using Controller.Player;
using SimpleInputNamespace;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;

namespace Controller
{
    public class DynamicJoystickController : MonoBehaviour
    {
        private Canvas canvas;
        private Joystick joystick;

        public PlayerController player;

        private bool isTouching;
        private int activeFingerId = -1;

        private AssetHandle assetHandle;
        public bool canInput = true;

        private void Awake()
        {
            assetHandle = GetComponent<AssetHandle>();
            EventCenter.Instance.AddListener(EventMessages.FocusView, CloseInput);
            EventCenter.Instance.AddListener(EventMessages.RestoreFocusView, OpenInput);
        }

        void Update()
        {
            if (!canInput)
                return;

#if UNITY_EDITOR
            HandleMouse();
#else
            HandleTouch();
#endif
        }

        #region Mouse (Editor)

        private void HandleMouse()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 pos = Input.mousePosition;

                if (!IsInJoystickArea(pos))
                    return;

                // if (IsPointerOverUIExceptJoystick(pos))
                //     return;

                ShowJoystick(pos, -1);
            }

            if (Input.GetMouseButton(0) && isTouching)
            {
                MovePlayer(joystick.Value);
            }

            if (Input.GetMouseButtonUp(0))
            {
                HideJoystick();
            }
        }

        #endregion

        #region Touch (Mobile)

        private void HandleTouch()
        {
            if (Input.touchCount == 0)
                return;

            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began && activeFingerId == -1)
                {
                    if (!IsInJoystickArea(touch.position))
                        continue;

                    if (IsPointerOverUI(touch.fingerId))
                        continue;

                    ShowJoystick(touch.position, touch.fingerId);
                }
                else if (touch.fingerId == activeFingerId)
                {
                    if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                    {
                        MovePlayer(joystick.Value);
                    }
                    else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        HideJoystick();
                    }
                }
            }
        }

        #endregion

        #region Core Logic

        /// <summary>
        /// 竖屏游戏：下半屏才允许摇杆
        /// </summary>
        private bool IsInJoystickArea(Vector2 screenPos)
        {
            return screenPos.y < Screen.height * 0.5f;
        }

        private bool IsPointerOverUIExceptJoystick(Vector2 screenPos)
        {
            if (EventSystem.current == null) return false;

            // 找到所有 Canvas 的 GraphicRaycaster
            GraphicRaycaster[] raycasters = FindObjectsOfType<GraphicRaycaster>();
            PointerEventData pointerData = new PointerEventData(EventSystem.current) { position = screenPos };

            foreach (var raycaster in raycasters)
            {
                var results = new System.Collections.Generic.List<RaycastResult>();
                raycaster.Raycast(pointerData, results);

                foreach (var r in results)
                {
                    // 排除摇杆 Layer
                    if (r.gameObject.layer == LayerMask.NameToLayer("Joystick"))
                        continue;

                    return true; // 其他 UI 被点击
                }
            }

            return false;
        }




        private bool IsPointerOverUI(int fingerId)
        {
            if (EventSystem.current == null)
                return false;

            return EventSystem.current.IsPointerOverGameObject(fingerId); // 手指
        }

        private void ShowJoystick(Vector2 screenPos, int fingerId)
        {
            if (canvas == null)
                canvas = GameObject.Find("Canvas").GetComponent<Canvas>();

            if (joystick == null)
            {
                joystick = Instantiate(
                    assetHandle.Get<GameObject>("ControlBg"),
                    canvas.transform
                ).GetComponent<Joystick>();
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                screenPos,
                null,
                out Vector2 anchoredPos
            );

            joystick.GetComponent<RectTransform>().anchoredPosition = anchoredPos;
            joystick.gameObject.SetActive(true);

            activeFingerId = fingerId;
            isTouching = true;
        }

        private void HideJoystick()
        {
            if (joystick)
                joystick.gameObject.SetActive(false);

            if (player)
                player.SetDir(Vector3.zero);

            isTouching = false;
            activeFingerId = -1;
        }

        private void MovePlayer(Vector2 dir)
        {
            if (!player)
                player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();

            if (!isTouching || dir.sqrMagnitude < 0.001f)
            {
                player.SetDir(Vector3.zero);
                return;
            }

            player.SetDir(new Vector3(dir.x, dir.y, 0));
        }

        #endregion

        private void CloseInput(params object[] args) => canInput = false;
        private void OpenInput(params object[] args) => canInput = true;
    }
}
