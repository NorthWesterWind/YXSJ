using UnityEngine;

namespace Controller
{
    public class CameraController : MonoBehaviour
    {
        [Header("要跟随的目标（例如 Player）")] public Transform target;

        [Header("跟随平滑度 (越大越慢)")] [Range(0.01f, 1f)]
        public float smoothSpeed = 0.1f;

        [Header("是否限制摄像机边界")] public bool useBounds = false;

        [Header("边界设置 (世界坐标)")] public Vector2 minBounds;
        public Vector2 maxBounds;

        private Vector3 currentVelocity;
        private Vector3 lastTargetPos;

        [Header("检测移动阈值")] public float moveThreshold = 0.01f;
        [Header("动态高度")] public float idleZ = -10f; // 默认高度// 移动时拉远
        public float zSmoothSpeed = 0.1f;
        
        private float currentZVelocity = 0f; // 用于 SmoothDamp Z
        private Camera cam;
        private void Awake()
        {
            cam = GetComponent<Camera>();
            if (cam == null)
            {
                cam = Camera.main;
            }
        }

        void LateUpdate()
        {
            if (target == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null) target = player.transform;
                else return;
            }

            // 平滑 X/Y
            Vector3 targetPosXY = new Vector3(target.position.x, target.position.y, 0);
            Vector3 smoothPosXY = Vector3.SmoothDamp(
                new Vector3(transform.position.x, transform.position.y, 0),
                targetPosXY, ref currentVelocity, smoothSpeed);
            
            transform.position = new Vector3(smoothPosXY.x, smoothPosXY.y, -10);

            lastTargetPos = target.position;
         
        }

         private void OnDrawGizmosSelected()
        {
            // --- 原有地图边界 ---
            if (useBounds)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(new Vector3(minBounds.x, minBounds.y), new Vector3(maxBounds.x, minBounds.y));
                Gizmos.DrawLine(new Vector3(maxBounds.x, minBounds.y), new Vector3(maxBounds.x, maxBounds.y));
                Gizmos.DrawLine(new Vector3(maxBounds.x, maxBounds.y), new Vector3(minBounds.x, maxBounds.y));
                Gizmos.DrawLine(new Vector3(minBounds.x, maxBounds.y), new Vector3(minBounds.x, minBounds.y));
            }

            // --- 摄像机渲染范围 ---
            if (cam == null) cam = GetComponent<Camera>();
            if (cam != null)
            {
                Gizmos.color = Color.red;

                float camHeight, camWidth;
                if (cam.orthographic)
                {
                    camHeight = cam.orthographicSize * 2f;
                    camWidth = camHeight * cam.aspect;
                }
                else
                {
                    // 透视摄像机：画近平面矩形
                    float distance = Mathf.Abs(idleZ - transform.position.z);
                    camHeight = 2f * distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
                    camWidth = camHeight * cam.aspect;
                }

                Vector3 camCenter = new Vector3(transform.position.x, transform.position.y, 0);
                Vector3 topLeft = camCenter + new Vector3(-camWidth / 2, camHeight / 2, 0);
                Vector3 topRight = camCenter + new Vector3(camWidth / 2, camHeight / 2, 0);
                Vector3 bottomLeft = camCenter + new Vector3(-camWidth / 2, -camHeight / 2, 0);
                Vector3 bottomRight = camCenter + new Vector3(camWidth / 2, -camHeight / 2, 0);

                Gizmos.DrawLine(topLeft, topRight);
                Gizmos.DrawLine(topRight, bottomRight);
                Gizmos.DrawLine(bottomRight, bottomLeft);
                Gizmos.DrawLine(bottomLeft, topLeft);
            }
        }
    }
}