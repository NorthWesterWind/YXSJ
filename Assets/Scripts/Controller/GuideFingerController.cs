using System.Collections;
using UnityEngine;

public class GuideFingerController : MonoBehaviour
{
    public RectTransform fingerUI;
    public RectTransform canvasRect;
    public Transform player;
    public float hideDistance = 2.5f;
    public float edgePadding = 40f;

    private Transform target;
    private Coroutine routine;

    public void StartGuide(Transform t)
    {
        target = t;

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(GuideRoutine());
    }

    public void StopGuide()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }

        fingerUI.gameObject.SetActive(false);
    }

    private IEnumerator GuideRoutine()
    {
        fingerUI.gameObject.SetActive(true);

        while (true)
        {
            if (player == null || target == null)
                yield break;

            float dist = Vector3.Distance(player.position, target.position);
            if (dist <= hideDistance)
            {
                GuideManager.Instance.CompleteStep();
                yield break;
            }

            // ① 世界坐标 → 屏幕坐标
            Vector3 targetScreenPos = Camera.main.WorldToScreenPoint(target.position);

            // 如果目标在摄像机背后，反向
            if (targetScreenPos.z < 0)
                targetScreenPos *= -1;

            // ② 屏幕坐标 → Canvas 本地坐标
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                targetScreenPos,
                Camera.main,
                out Vector2 targetCanvasPos
            );

            // ③ 从屏幕中心指向目标的方向
            Vector2 dir = targetCanvasPos.normalized;

            // ④ 限制在屏幕边缘
            float halfW = canvasRect.rect.width * 0.5f - edgePadding;
            float halfH = canvasRect.rect.height * 0.5f - edgePadding;

            float scale = Mathf.Min(
                halfW / Mathf.Abs(dir.x == 0 ? 0.0001f : dir.x),
                halfH / Mathf.Abs(dir.y == 0 ? 0.0001f : dir.y)
            );

            fingerUI.anchoredPosition = dir * scale;

            // ⑤ UI 朝向目标
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            fingerUI.localRotation = Quaternion.Euler(0, 0, angle);

            yield return null;
        }
    }
}
