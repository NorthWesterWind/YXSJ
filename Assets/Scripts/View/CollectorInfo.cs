using System.Collections;
using Controller;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class CollectorInfo : MonoBehaviour
{
    public Transform target; // 角色头顶挂点
    public Vector3 offset;   // 屏幕偏移（比如往上抬一点）
    public Image fillImage;
    public Image fillBg;
    public Canvas canvas;
    public TextMeshProUGUI text;
    public CollectorController collector;
    public AssetHandle _assetHandle;

    public void Init(CollectorController c)
    {
        this.collector = c;
        target = collector.infoTransform;
        collector.collectorInfo = this;
         _assetHandle = GetComponent<AssetHandle>();
        HideHpInfo();
    }

    public void HideHpInfo()
    {
        fillBg.gameObject.SetActive(false);
    }

    public void ShowHpInfo()
    {
        fillBg.gameObject.SetActive(true);
    }



    private void LateUpdate()
    {
        StartCoroutine(UpdateUIPosition());
    }

    private IEnumerator UpdateUIPosition()
    {
        yield return new WaitForEndOfFrame(); // 等摄像机完全更新完

        Vector3 screenPos = Camera.main.WorldToScreenPoint(target.position + offset);
        transform.position = screenPos;

        SetLayer();
    }

    private void Update()
    {

    }

    public void SetLayer()
    {
        int newOrder = 3000 - Mathf.FloorToInt(collector.transform.localPosition.y);
        canvas.sortingOrder = newOrder;
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
            text.text = "口袋已满";
        }
        else
        {
            text.text = $"{collector.currentCarryNum}/{collector.maxCarryNum}";
        }
        EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerCarryInfo);
    }
}
