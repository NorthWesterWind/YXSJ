using System.Collections;
using System.Collections.Generic;
using Controller;
using Controller.Player;
using Module.Data;
using TMPro;
using UnityEngine;
using Utils;

public class OrderDetailPop : MonoBehaviour
{
   public UIButton closeBtn;
   public UIButton refuseBtn;
    public UIButton acceptBtn;

    public TextMeshProUGUI tongbiTxt;
    public TextMeshProUGUI jingyuanbaoTxt;
    public Transform content;
    public OrderDataProgress _data;
    AssetHandle assetHandle;

    PlayerController playerController;

    public void Init(OrderDataProgress data)
    {
       var  orderData = DataController.Instance.orderDataDic[data.orderId];
        _data = data;
        if(assetHandle == null)
        {
            assetHandle = GetComponent<AssetHandle>();
        }
        tongbiTxt.text = orderData.rewardCoin.ToString();
        jingyuanbaoTxt.text = orderData.rewardGold.ToString();
        Extensions.ClearChildren(content);

        foreach (var goods in data.goodDic)
        {
            GameObject obj = Instantiate(assetHandle.Get<GameObject>("OrderNeedItem"), content, false);
            var item = obj.GetComponent<OrderNeedItem>();
            item.Init(goods.Key, goods.Value.ToString() + "/10");
        }
          foreach (var goods in data.dropDic)
        {
            GameObject obj = Instantiate(assetHandle.Get<GameObject>("OrderNeedItem"), content, false);
            var item = obj.GetComponent<OrderNeedItem>();
            item.Init(goods.Key, goods.Value.ToString() + "/10");
        }
        
    }


    void Start()
    {
        
    }

}
