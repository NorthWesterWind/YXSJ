using System;
using Module.Data;
using Utils;

namespace Module
{
    public class PlayerDataModule : BaseModule
    {
        public PlayerData data = new();
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

        public bool RemoveJinYuanBao(int value)
        {
            if (data.goldIngot >= value)
            {
                data.goldIngot -= value;
                EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void AddYinQian(int value)
        {
            data.silverCoin += value;
            EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
        }

        public bool RemoveYinQian(int value)
        {
            if (data.silverCoin >= value)
            {
                data.silverCoin -= value;
                EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
                return true;
            }
            else
            {
                return false;
            }
        }


        public void UpgradeAccountLevel()
        {
            data.accountLevel += 1;
            if (data.accountLevel >= 2)
            {
                if (data.characterFunction == 0)
                {
                    data.characterFunction = 1;
                }
                else if (data.cardFunction == 0)
                {
                    data.cardFunction = 1;
                }
            }

            if (data.accountLevel >= 5)
            {
                if (data.mapFunction == 0)
                {
                    data.mapFunction = 1;
                }

                if (data.levelLockMapList.Contains(2))
                {
                    data.levelLockMapList.Remove(2);
                }
            }

            if (data.accountLevel >= 10)
            {
                if (data.levelLockMapList.Contains(3))
                {
                    data.levelLockMapList.Remove(3);
                }
            }

            if (data.accountLevel >= 12 && data.ordenFunction == 0)
            {
                data.ordenFunction = 1;
            }

            if (data.accountLevel >= 20)
            {
                if (data.levelLockMapList.Contains(4))
                {
                    data.levelLockMapList.Remove(4);
                }
            }

            if (data.accountLevel >= 30)
            {
                if (data.levelLockMapList.Contains(5))
                {
                    data.levelLockMapList.Remove(5);
                }
            }
        }

        public void UnlockEmployeeFunction()
        {
            data.employeeFunction = 1;
        }
        
    }
}