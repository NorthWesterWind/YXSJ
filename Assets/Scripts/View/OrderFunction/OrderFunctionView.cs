using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace View.OrderFunction
{
    public class OrderFunctionView : BaseView
    {
        public UIButton closeBtn;
        public UIButton huoquBtn;
        public HuoQuPop huoQuPop;
        public OrderData orderData;
        private PlayerData playerData;
        public OrderDetailPop orderDetailPop;
        public GameObject orderDetailPopObj;

        public Transform content;

        public GameObject fillContent;
        public Image fill;
        public TextMeshProUGUI numtxt;

        protected override void AddEventListener()
        {
            base.AddEventListener();
            closeBtn.onClick.RemoveAllListeners();
            closeBtn.onClick.AddListener((() =>
            {
                Hide();
            }));

            EventCenter.Instance.AddListener(EventMessages.ShowOrderDetail, ShowDetailPop);
            EventCenter.Instance.AddListener(EventMessages.UpdateOrderItem, HandleUpdateOrderItem);
            huoquBtn.onClick.RemoveAllListeners();
            huoquBtn.onClick.AddListener(() =>
            {
                huoQuPop.gameObject.SetActive(true);
            });
        }

        public override void RemoveEventListener()
        {
            base.RemoveEventListener();
            EventCenter.Instance.RemoveListener(EventMessages.ShowOrderDetail, ShowDetailPop);
            EventCenter.Instance.RemoveListener(EventMessages.UpdateOrderItem, HandleUpdateOrderItem);
        }

        public void UpdateFillAmount(params object[] args)
        {
            {
            }
        }

        void Update()
        {
            if (playerData.orderDataprogressList.Count < 4)
            {
                fillContent.SetActive(true);
                fill.fillAmount = PlayerDataModule.Instance.orderRefreshProgress;
            }
            else
            {
                fillContent.SetActive(false);
            }
        }

        public override void UpdateViewWithArgs(params object[] args)
        {
            base.UpdateViewWithArgs(args);
            playerData = PlayerDataModule.Instance.data;

            var orderDataprogressList = playerData.orderDataprogressList;
            Extensions.ClearChildren(content);
            if (orderDataprogressList.Count < 1)
            {
                numtxt.text = "顾客人数：0。";
            }
            else
            {
                for (int i = 0; i < orderDataprogressList.Count; i++)
                {
                    GameObject obj = Instantiate(_assetHandle.Get<GameObject>("orderItem"), content, false);
                    obj.GetComponent<OrderItem>().Init(orderDataprogressList[i]);
                }
                numtxt.text = "顾客人数：" + orderDataprogressList.Count + "。";
            }
        }

        public void HandleUpdateOrderItem(params object[] args)
        {
            playerData = PlayerDataModule.Instance.data;
            var orderDataprogressList = playerData.orderDataprogressList;
            Extensions.ClearChildren(content);
            numtxt.text = "顾客人数：" + orderDataprogressList.Count + "。";
            if (orderDataprogressList.Count < 1)
            {
                return;
            }

            for (int i = 0; i < orderDataprogressList.Count; i++)
            {
                GameObject obj = Instantiate(_assetHandle.Get<GameObject>("orderItem"), content, false);
                obj.GetComponent<OrderItem>().Init(orderDataprogressList[i]);
            }
        }

        private void ShowDetailPop(params object[] args)
        {
            orderDetailPopObj.SetActive(true);
            OrderDataProgress dataProgress = args[0] as OrderDataProgress;
            orderDetailPop.Init(dataProgress);
        }

        protected override void OnHideComplete()
        {
            base.OnHideComplete();
            EventCenter.Instance.TriggerEvent(EventMessages.ShowPlayerInfoViewCartoon);
        }
    }
}
