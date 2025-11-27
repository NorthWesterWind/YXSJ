using System;
using SimpleInputNamespace;
using UnityEngine;
using Utils;
using CharacterController = Controller.Player.CharacterController;

namespace Controller
{
    public class DynamicJoystickController : MonoBehaviour
    {
        [Header("父级 Canvas")]
        private Canvas canvas;

        private Joystick currentJoystick;

        [Header("控制角色对象")]
        public CharacterController player;
        public float moveSpeed = 5f;

        private bool isTouching = false;
        private int activeFingerId = -1;
        private AssetHandle _assetHandle;
        public bool canInput = true;
        private void Awake()
        {
            _assetHandle = gameObject.GetComponent<AssetHandle>();
            EventCenter.Instance.AddListener(EventMessages.FocusView,CloseInput);
            EventCenter.Instance.AddListener(EventMessages.RestoreFocusView,OpenInput);
        }

        void Update()
        {
            if(!canInput)
                return;
            // 处理触摸（旧输入系统下）
#if UNITY_EDITOR
            // 鼠标调试支持
            if (Input.GetMouseButtonDown(0))
                ShowJoystick(Input.mousePosition, -999);
            else if (Input.GetMouseButtonUp(0))
                HideJoystick();
            else if (Input.GetMouseButton(0) && currentJoystick)
                MovePlayer(currentJoystick.Value);
#else
        if (Input.touchCount > 0)
        {
            foreach (var touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began && activeFingerId == -1)
                {
                    ShowJoystick(touch.position, touch.fingerId);
                }
                else if (touch.fingerId == activeFingerId)
                {
                    if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                    {
                        MovePlayer(currentJoystick.Value);
                    }
                    else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        HideJoystick();
                    }
                }
            }
        }
#endif
        }
        
        private void ShowJoystick(Vector2 screenPosition, int fingerId)
        {
            if (canvas == null)
                canvas = GameObject.Find("Canvas").GetComponent<Canvas>();

            if (currentJoystick == null)
                currentJoystick = Instantiate(_assetHandle.Get<GameObject>("ControlBg"), canvas.transform).GetComponent<Joystick>();
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                screenPosition,
                null, 
                out var anchoredPos);

            var joystickRect = currentJoystick.GetComponent<RectTransform>();
            joystickRect.anchoredPosition = anchoredPos;
            currentJoystick.gameObject.SetActive(true);

            activeFingerId = fingerId;
            isTouching = true;
            
        }


        private void HideJoystick()
        {
            if (currentJoystick)
                currentJoystick.gameObject.SetActive(false);
            
            
            if (player != null)
                player.SetDir(Vector3.zero);
            isTouching = false;
            activeFingerId = -1;
            EventCenter.Instance.TriggerEvent(EventMessages.TriggerDetection);
        }

        private void MovePlayer(Vector2 direction)
        {
            if (player == null)
            {
                player = GameObject.Find("Player").GetComponent<CharacterController>();
            }
            if (!isTouching || direction.sqrMagnitude < 0.001f)
            {
                player.SetDir(Vector3.zero);
              
                return;
            }
            Vector3 moveDir = new Vector3(direction.x, direction.y,0);
            player.SetDir(moveDir);
        }


        private void CloseInput(params object[] args)
        {
           canInput = false;
        }
        private void OpenInput(params object[] args)
        {
            canInput = true;
        }
    }
}
