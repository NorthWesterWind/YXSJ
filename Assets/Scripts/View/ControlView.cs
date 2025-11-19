using UnityEngine;
using World.View.UI;

namespace View
{
    public class ControlView : BaseView
    {
        public RectTransform background; // 外圈
        public RectTransform handle;     // 摇杆点
        public float handleRange = 100f;  // 摇杆最大半径
        
        public float longPressTime = 0.2f;      // 长按触发时间
        
        [Header("角色")]
        public Transform player;
        public float moveSpeed = 5f;

        private bool isTouching = false;
        private bool isJoystickActive = false;
        private float touchStartTime = 0f;
        private Vector2 startPos;
        private Vector2 inputDir;
        
      
    }
}
