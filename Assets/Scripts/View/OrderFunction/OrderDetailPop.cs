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

    private AssetHandle assetHandle;
    private PlayerController playerController;

    void Start()
    {
        closeBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.AddListener(() => { gameObject.SetActive(false); });

        refuseBtn.onClick.RemoveAllListeners();
        refuseBtn.onClick.AddListener(() =>
        {
            PlayerDataModule.Instance.data.orderDataprogressList.Remove(_data);
            EventCenter.Instance.TriggerEvent(EventMessages.UpdateOrderItem);
            gameObject.SetActive(false);
        });

        acceptBtn.onClick.RemoveAllListeners();
        acceptBtn.onClick.AddListener(HandleAcceptClicked);
    }

    private void HandleAcceptClicked()
    {
        bool submittedAnyItem = false;

        submittedAnyItem |= SubmitGoodsRequirements();
        submittedAnyItem |= SubmitDropRequirements();
        RemoveCompletedRequirements();

        if (submittedAnyItem)
        {
            EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerInfo);
            EventCenter.Instance.TriggerEvent(EventMessages.UpdateOrderItem);
        }

        if (_data.goodDic.Count == 0 && _data.dropDic.Count == 0)
        {
            CompleteOrder();
            return;
        }

        if (submittedAnyItem)
        {
            Init(_data);
            UIController.Instance.Show<TipView>("已提交部分物品。");
            return;
        }

        UIController.Instance.Show<TipView>("货物不足。");
    }

    private bool SubmitGoodsRequirements()
    {
        bool submitted = false;
        var goodsTypes = _data.goodDic.Keys.ToList();
        for (int i = 0; i < goodsTypes.Count; i++)
        {
            var goodsType = goodsTypes[i];
            var progress = _data.goodDic[goodsType];
            int needNum = progress.target - progress.current;
            if (needNum <= 0)
            {
                continue;
            }

            if (playerController.goodsDic == null ||
                !playerController.goodsDic.TryGetValue(goodsType, out int ownNum) ||
                ownNum <= 0)
            {
                continue;
            }

            int submitNum = Mathf.Min(ownNum, needNum);
            if (submitNum <= 0)
            {
                continue;
            }

            progress.current += submitNum;
            playerController.goodsDic[goodsType] = ownNum - submitNum;
            submitted = true;
        }

        return submitted;
    }

    private bool SubmitDropRequirements()
    {
        bool submitted = false;
        var dropTypes = _data.dropDic.Keys.ToList();
        for (int i = 0; i < dropTypes.Count; i++)
        {
            var dropType = dropTypes[i];
            var progress = _data.dropDic[dropType];
            int needNum = progress.target - progress.current;
            if (needNum <= 0)
            {
                continue;
            }

            if (playerController.dropDic == null ||
                !playerController.dropDic.TryGetValue(dropType, out int ownNum) ||
                ownNum <= 0)
            {
                continue;
            }

            int submitNum = Mathf.Min(ownNum, needNum);
            if (submitNum <= 0)
            {
                continue;
            }

            progress.current += submitNum;
            playerController.dropDic[dropType] = ownNum - submitNum;
            submitted = true;
        }

        return submitted;
    }

    private void RemoveCompletedRequirements()
    {
        foreach (var item in _data.goodDic.Keys.ToList())
        {
            if (_data.goodDic[item].current >= _data.goodDic[item].target)
            {
                _data.goodDic.Remove(item);
            }
        }

        foreach (var item in _data.dropDic.Keys.ToList())
        {
            if (_data.dropDic[item].current >= _data.dropDic[item].target)
            {
                _data.dropDic.Remove(item);
            }
        }
    }

    private void CompleteOrder()
    {
        int id = _data.orderId;
        OrderData orderData = DataController.Instance.orderDataDic[id];
        Dictionary<CurrencyType, int> rewards = new Dictionary<CurrencyType, int>();

        if (orderData.rewardCoin > 0)
        {
            rewards.Add(CurrencyType.TongBi, orderData.rewardCoin);
            PlayerDataModule.Instance.data.tongbi += orderData.rewardCoin;
            EventCenter.Instance.TriggerEvent(EventMessages.MakeTongBiTask, orderData.rewardCoin);
        }

        if (orderData.rewardGold > 0)
        {
            rewards.Add(CurrencyType.JingYuanBao, orderData.rewardGold);
            PlayerDataModule.Instance.data.goldIngot += orderData.rewardGold;
        }

        EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
        UIController.Instance.Show<RewardConfirmView>(rewards);

        PlayerDataModule.Instance.data.orderDataprogressList.Remove(_data);
        EventCenter.Instance.TriggerEvent(EventMessages.UpdateOrderItem);
        UIController.Instance.Show<TipView>("订单完成！");
        gameObject.SetActive(false);
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
            int needNum = Mathf.Max(0, goods.Value.target - goods.Value.current);
            int ownNum = 0;
            if (playerController != null && playerController.goodsDic != null)
            {
                playerController.goodsDic.TryGetValue(goods.Key, out ownNum);
            }
            item.Init(goods.Key, ownNum, needNum);
        }

        foreach (var goods in data.dropDic)
        {
            GameObject obj = Instantiate(assetHandle.Get<GameObject>("OrderNeedItem"), content, false);
            var item = obj.GetComponent<OrderNeedItem>();
            int needNum = Mathf.Max(0, goods.Value.target - goods.Value.current);
            int ownNum = 0;
            if (playerController != null && playerController.dropDic != null)
            {
                playerController.dropDic.TryGetValue(goods.Key, out ownNum);
            }
            item.Init(goods.Key, ownNum, needNum);
        }

        OrderData displayData = DataController.Instance.orderDataDic[data.orderId];
        peopleIcon.sprite = assetHandle.Get<Sprite>(displayData.elderType.ToString());
    }
}
