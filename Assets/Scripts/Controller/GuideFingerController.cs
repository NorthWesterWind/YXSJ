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

            float dist = Vector2.Distance(player.position, target.position);
            if (dist <= hideDistance)
            {
                GuideManager.Instance.CompleteStep();
                yield break;
            }

            Vector2 dir = (Vector2)(target.position - player.position);
            Vector2 n = dir.normalized;

            float halfW = canvasRect.rect.width * 0.5f - edgePadding;
            float halfH = canvasRect.rect.height * 0.5f - edgePadding;

            float tX = halfW / Mathf.Abs(n.x);
            float tY = halfH / Mathf.Abs(n.y);

            float t = Mathf.Min(tX, tY);
            fingerUI.anchoredPosition = n * t;

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            fingerUI.rotation = Quaternion.Euler(0, 0, angle);

            yield return null;
        }
    }
}
