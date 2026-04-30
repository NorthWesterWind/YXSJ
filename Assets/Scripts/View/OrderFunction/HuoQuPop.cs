using Module;
using UnityEngine;
using Utils;
using View;

public class HuoQuPop : MonoBehaviour
{
    public UIButton closeBtn;
    public UIButton confirmBtn;

    void Start()
    {
        closeBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
        });

        confirmBtn.onClick.RemoveAllListeners();
        confirmBtn.onClick.AddListener(() =>
        {
            OnClickConfirm();
        });
    }

    public void OnClickConfirm()
    {
        if (PlayerDataModule.Instance.data.orderDataprogressList.Count == 4)
        {
            Debug.Log("订单已满，无法获取更多订单。");
            UIController.Instance.Show<TipView>("订单已满，无法获取更多订单。");
            return;
        }

        if (PlayerDataModule.Instance.data.lingJing < 100)
        {
            UIController.Instance.Show<TipView>("灵晶不足！");
            return;
        }

        PlayerDataModule.Instance.data.lingJing -= 100;
        PlayerDataModule.Instance.AddOrderData();
        EventCenter.Instance.TriggerEvent(EventMessages.UpdateOrderItem);
        UIController.Instance.Show<TipView>("获取新订单成功！");
        gameObject.SetActive(false);
    }
}
