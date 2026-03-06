using Controller;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CollectorInfo : MonoBehaviour
{
    public Image fillImage;
    public Image fillBg;
    public TextMeshProUGUI text;
    public CollectorController collector;

    private void Awake()
    {
        if (collector == null)
        {
            collector = GetComponentInParent<CollectorController>();
        }

        HideHpInfo();
        UpdateTxt();
    }

    public void Bind(CollectorController controller)
    {
        collector = controller;
        if (collector == null)
        {
            return;
        }

        float maxHp = Mathf.Max(collector.maxHp, 0.001f);
        UpdateFill(collector.currentHp / maxHp);
    }

    public void HideHpInfo()
    {
        if (fillBg != null)
        {
            fillBg.gameObject.SetActive(false);
        }
    }

    public void ShowHpInfo()
    {
        if (fillBg != null)
        {
            fillBg.gameObject.SetActive(true);
        }
    }

    public void UpdateFill(float value)
    {
        if (fillImage == null)
        {
            return;
        }

        ShowHpInfo();
        fillImage.fillAmount = Mathf.Clamp01(value);
       
    }

    public void UpdateTxt()
    {
        if (text == null || collector == null)
        {
            return;
        }
        text.text = $"{collector.currentCarryNum}/{collector.maxCarryNum}";
    }
}