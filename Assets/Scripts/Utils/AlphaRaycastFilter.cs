using UnityEngine;
using UnityEngine.UI;

public class AlphaRaycastFilter : MonoBehaviour, ICanvasRaycastFilter
{
    public Image image;
    [Range(0,1)]
    public float alphaThreshold = 0.1f;

    private Texture2D texture;

    void Awake()
    {
        if (image == null)
            image = GetComponent<Image>();

        if (image != null && image.sprite != null)
        {
            texture = image.sprite.texture;
        }
    }

    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        if (image == null || image.sprite == null)
            return true;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            image.rectTransform, sp, eventCamera, out Vector2 localPoint);

        Rect rect = image.rectTransform.rect;

        float normalizedX = (localPoint.x - rect.x) / rect.width;
        float normalizedY = (localPoint.y - rect.y) / rect.height;

        if (normalizedX < 0 || normalizedX > 1 ||
            normalizedY < 0 || normalizedY > 1)
            return false;

        try
        {
            int x = Mathf.FloorToInt(normalizedX * texture.width);
            int y = Mathf.FloorToInt(normalizedY * texture.height);

            Color pixel = texture.GetPixel(x, y);

            return pixel.a >= alphaThreshold;
        }
        catch
        {
            return false;
        }
    }
}