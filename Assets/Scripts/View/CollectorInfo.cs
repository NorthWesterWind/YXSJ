using System.Collections;
using Controller;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class CollectorInfo : MonoBehaviour
{
    public Image fillImage;
    public Image fillBg;
    public TextMeshProUGUI text;
    public CollectorController collector;



    public void HideHpInfo()
    {
        fillBg.gameObject.SetActive(false);
    }

    public void ShowHpInfo()
    {
        fillBg.gameObject.SetActive(true);
    }

    public void UpdateFill(float value)
    {
        ShowHpInfo();
        fillImage.DOFillAmount(Mathf.Min(value, 1), 0.3f);
    }


    public void UpdateTxt()
    {
        if (collector.currentCarryNum >= collector.maxCarryNum)
        {
            text.text = "已满";
        }
        else
        {
            text.text = $"{collector.currentCarryNum}/{collector.maxCarryNum}";
        }
        EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerCarryInfo);
    }
}
