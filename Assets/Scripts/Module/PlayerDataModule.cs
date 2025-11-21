using System;
using Module.Data;
using Utils;

namespace Module
{
    public class PlayerDataModule : BaseModule
    {
        public PlayerData data = new ();
        public override Type GetDataType() => typeof(PlayerData);
        protected override void OnInitialize()
        {
            base.OnInitialize();
            //处理数据
        }


        public void AddJinYuanBao(int value)
        {
            data.goldIngot += value;
            EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
        }

        public bool  RemoveJinYuanBao(int value)
        {
            if (data.goldIngot >= value)
            {
                data.goldIngot -= value;
                EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
                return true;    
            }
            else
            {
                return  false;
            }
        }
        public void AddYinQian(int value)
        {
            data.silverCoin += value;
            EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
        }

        public bool  RemoveYinQian(int value)
        {
            if (data.silverCoin >= value)
            {
                data.silverCoin -= value;
                EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
                return true;    
            }
            else
            {
                return  false;
            }
        }
    }
}