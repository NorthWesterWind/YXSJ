using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class MessageItem : MonoBehaviour
{
    public TextMeshProUGUI message;
    public Image headIcon;
    public TextMeshProUGUI timetxt;
    private AssetHandle assetHandle;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    private void Awake()
    {
        CacheComponents();
        SetVisible(false);
    }

    public void InitInfo(string res, string time, string message)
    {
        CacheComponents();
        SetVisible(false);

        if (headIcon != null && assetHandle != null)
        {
            headIcon.sprite = assetHandle.Get<Sprite>(res);
        }

        if (timetxt != null)
        {
            timetxt.text = time;
            timetxt.ForceMeshUpdate();
        }

        if (this.message != null)
        {
            this.message.text = string.IsNullOrEmpty(message)
                ? ""
                : (message.Length > 18 ? message.Substring(0, 18) : message);
            this.message.ForceMeshUpdate();
        }

        RebuildSelfLayout();
        StartCoroutine(ShowAfterLayoutReady());
    }

    private void CacheComponents()
    {
        if (assetHandle == null)
        {
            assetHandle = GetComponent<AssetHandle>();
        }

        if (rectTransform == null)
        {
            rectTransform = transform as RectTransform;
        }

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
    }

    private void RebuildSelfLayout()
    {
        Canvas.ForceUpdateCanvases();
        if (rectTransform != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }
    }

    private IEnumerator ShowAfterLayoutReady()
    {
        yield return null;
        RebuildSelfLayout();
        SetVisible(true);
    }

    private void SetVisible(bool visible)
    {
        if (canvasGroup == null) return;
        canvasGroup.alpha = visible ? 1f : 0f;
    }
}
