using System.Collections.Generic;
using System.Linq;
using Controller;
using Controller.Player;
using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using View;

public class OrderDetailPop : MonoBehaviour
{
    public UIButton closeBtn;
    public UIButton refuseBtn;
    public UIButton acceptBtn;

    public Image peopleIcon;

    public TextMeshProUGUI tongbiTxt;
    public TextMeshProUGUI jingyuanbaoTxt;
    public Transform content;
    public OrderDataProgress _data;
    AssetHandle assetHandle;

    PlayerController playerController;

    void Start()
    {
        closeBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
        });
        refuseBtn.onClick.RemoveAllListeners();
        refuseBtn.onClick.AddListener(() =>
        {
            PlayerDataModule.Instance.data.orderDataprogressList.Remove(_data);
            EventCenter.Instance.TriggerEvent(EventMessages.UpdateOrderItem);
            gameObject.SetActive(false);
        });
        acceptBtn.onClick.RemoveAllListeners();
        acceptBtn.onClick.AddListener(() =>
        {
            var list = _data.goodDic.Keys.ToList();
            foreach (var item in list)
            {
                var progress = _data.goodDic[item];
                if (playerController.goodsDic.ContainsKey(item) &&
                 progress.target - progress.current > 0)
                {
                    if (playerController.goodsDic[item] < progress.target - progress.current)
                    {
                        progress.current += playerController.goodsDic[item];
                        playerController.goodsDic[item] = 0;
                    }
                    else
                    {
                        int value = progress.target - progress.current;
                        progress.current = progress.target;
                        playerController.goodsDic[item] -= value;
                    }
                }
            }
            var list_1 = _data.dropDic.Keys.ToList();
            foreach (var item1 in list_1)
            {
                var progress = _data.dropDic[item1];
                if (playerController.dropDic.ContainsKey(item1) &&
                 progress.target - progress.current > 0)
                {
                    if (playerController.dropDic[item1] < progress.target - progress.current)
                    {
                        progress.current += playerController.dropDic[item1];
                        playerController.dropDic[item1] = 0;
                    }
                    else
                    {
                        int value = progress.target - progress.current;
                        progress.current = progress.target;
                        playerController.dropDic[item1] -= value;
                    }
                }
            }
            foreach (var item in _data.goodDic.Keys.ToList())
            {
                if (_data.goodDic[item].current == _data.goodDic[item].target)
                {
                    _data.goodDic.Remove(item);
                }
            }
            foreach (var item in _data.dropDic.Keys.ToList())
            {
                if (_data.dropDic[item].current == _data.dropDic[item].target)
                {
                    _data.dropDic.Remove(item);
                }
            }
            if (_data.goodDic.Count == 0 && _data.dropDic.Count == 0)
            {
                int id = _data.orderId;

                OrderData orderData = DataController.Instance.orderDataDic[id];
                Dictionary<CurrencyType, int> dic = new Dictionary<CurrencyType, int>();
                if (orderData.rewardCoin > 0)
                {
                    dic.Add(CurrencyType.TongBi, orderData.rewardCoin);
                    PlayerDataModule.Instance.data.tongbi += orderData.rewardCoin;
                    EventCenter.Instance.TriggerEvent(EventMessages.MakeTongBiTask, orderData.rewardCoin);
                }
                if (orderData.rewardGold > 0)
                {
                    dic.Add(CurrencyType.JingYuanBao, orderData.rewardGold);
                    PlayerDataModule.Instance.data.goldIngot += orderData.rewardGold;

                }
                UIController.Instance.Show<RewardConfirmView>(dic);

                PlayerDataModule.Instance.data.orderDataprogressList.Remove(_data);
                EventCenter.Instance.TriggerEvent(EventMessages.UpdateOrderItem);
                UIController.Instance.Show<TipView>("订单完成");
                gameObject.SetActive(false);


            }
            else
            {
                UIController.Instance.Show<TipView>("货物不足。");
            }


        });
    }

    public void Init(OrderDataProgress data)
    {
        if (playerController == null)
        {
            playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        }
        var orderData = DataController.Instance.orderDataDic[data.orderId];
        _data = data;
        if (assetHandle == null)
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
            item.Init(goods.Key, goods.Value.current
            + "/" + goods.Value.target);
        }
        foreach (var goods in data.dropDic)
        {
            GameObject obj = Instantiate(assetHandle.Get<GameObject>("OrderNeedItem"), content, false);
            var item = obj.GetComponent<OrderNeedItem>();
            item.Init(goods.Key, goods.Value.current
            + "/" + goods.Value.target);
        }
        OrderData data_ = DataController.Instance.orderDataDic[data.orderId];   
        peopleIcon.sprite = assetHandle.Get<Sprite>(data_.elderType.ToString());

    }

}
