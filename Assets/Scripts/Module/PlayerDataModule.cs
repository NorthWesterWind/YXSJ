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
    }
}