using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Controller;
using Module.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public enum OrderType
{
    LingZhi,
    LingShi,
}

public class OrderItem : MonoBehaviour
{
    public Image peopleIcon;
    public TextMeshProUGUI info;
    public UIButton checkBtn;
    public List<GoodsType> goodsTypes = new List<GoodsType>();
    public List<DropItemType> dropItemTypes = new List<DropItemType>();
    AssetHandle _assetHandle;
    public OrderDataProgress _orderDataProgress;

    public void Init(OrderDataProgress orderDataProgress)
    {
        if (_assetHandle == null)
        {
            _assetHandle = GetComponent<AssetHandle>();
        }
        _orderDataProgress = orderDataProgress;
        OrderData data = DataController.Instance.orderDataDic[orderDataProgress.orderId];
        peopleIcon.sprite = _assetHandle.Get<Sprite>(data.elderType.ToString());
        info.text = "来点上好的货物。";
    }

    void Start()
    {
        checkBtn.onClick.RemoveAllListeners();
        checkBtn.onClick.AddListener(() =>
        {
            EventCenter.Instance.TriggerEvent(EventMessages.ShowOrderDetail, _orderDataProgress);
        });
    }
}
