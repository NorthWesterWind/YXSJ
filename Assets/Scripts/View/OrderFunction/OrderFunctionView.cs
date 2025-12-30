using Controller;
using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using Utils;

namespace View.OrderFunction
{
    public class OrderFunctionView : BaseView
    {
        public UIButton closeBtn;
        public TextMeshProUGUI goldtxt;
        public TextMeshProUGUI  slivertxt;
    
        public DetailPop detailPop;
        public HuoQuPop huoQuPop;
        public  OrderData orderData;
        private PlayerData playerData;

       
        public override void UpdateViewWithArgs(params object[] args)
        {
            base.UpdateViewWithArgs(args);
            playerData = ModuleMgr.Instance.GetModule<PlayerDataModule>().data;
        
            var orderDataprogressList = playerData.orderDataprogressList;
            if(orderDataprogressList.Count < 1)
            {
                //没有订单
            }
            else if(orderDataprogressList.Count == 4)
            {
                //订单已满
            }
            else
            {
                //有订单
                int orderIndex = orderDataprogressList.Count -1;
                var currentOrderData = orderDataprogressList[orderIndex];
               // orderData = DataController.Instance.orderDataDic[currentOrderData.id];
                goldtxt.text = "金元宝奖励: " + orderData.rewardGold;
                slivertxt.text = "银钱奖励: " + orderData.rewardCoin;
            }
        }
    }
}
