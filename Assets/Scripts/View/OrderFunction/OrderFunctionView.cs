using System.Collections.Generic;
using Controller;
using Module;
using Module.Data;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Utils;

namespace View.OrderFunction
{
    public class OrderFunctionView : BaseView
    {
        public UIButton closeBtn;
        public DetailPop detailPop;
        public HuoQuPop huoQuPop;
        public  OrderData orderData;
        private PlayerData playerData;
        public OrderDetailPop orderDetailPop;

        public Transform content;


        protected override void AddEventListener()
        {
            base.AddEventListener();
            closeBtn.onClick.RemoveAllListeners();
            closeBtn.onClick.AddListener((() =>
            {
                Hide();
            }));

        }

       
        public override void UpdateViewWithArgs(params object[] args)
        {
            base.UpdateViewWithArgs(args);
            playerData = PlayerDataModule.Instance.data;
        
            var orderDataprogressList = playerData.orderDataprogressList;
            Extensions.ClearChildren(content);
            if(orderDataprogressList.Count < 1)
            {
                //没有订单
            }
            else
            {
                for(int i = 0 ; i < orderDataprogressList.Count; i++)
                {
                    GameObject obj = Instantiate(_assetHandle.Get<GameObject>("OrderItem"), content, false);
                    
                    obj.GetComponent<OrderItem>().Init( orderDataprogressList[i]);
                }
            }
        }


        private void ShowDetailPop(params object[] args)
        {
            OrderDataProgress dataProgress = args[0] as  OrderDataProgress;
        }

        protected override void OnHideComplete()
        {
            base.OnHideComplete();
             EventCenter.Instance.TriggerEvent(EventMessages.ShowPlayerInfoViewCartoon);
        }
    }
}
