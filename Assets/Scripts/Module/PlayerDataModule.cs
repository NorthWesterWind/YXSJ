using System;
using System.Collections.Generic;
using System.Linq;
using Controller;
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
            data.tongbi += value;
            EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
        }

        public bool RemoveYinQian(int value)
        {
            if (data.tongbi >= value)
            {
                data.tongbi -= value;
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

        public void GetTaskReward(int rewardId)
        {
            RewardData rewardData = DataController.Instance.taskRewardDataDic[rewardId];
            data.jingMangZhu += rewardData.Jmz;
            data.tongbi += rewardData.Yq;
            data.goldIngot += rewardData.Jyb;
            EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
        }

        public void GetSevenDayReward(int day)
        {
            SevenDayRewardData _data = DataController.Instance.sevenDayRewardDataDic[day];
            data.goldIngot += _data.Jyb;
            data.tongbi += _data.Jyb;
            data.lingJing += _data.Lj;
            data.sevenDayRecordList.Add(day);
            data.sevenDayRecordTime = DateTime.Now.ToString("yyyy/MM/dd");
            EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
        }




        public Dictionary<int,int> LotteryCard(GiftpackData giftpack)
        {
            Dictionary<int, int> dict = new Dictionary<int, int>();
            DrawFixedCards(dict, giftpack.linYunNum,  CardLevelType.LingYun);
            DrawFixedCards(dict, giftpack.xianYunNum,  CardLevelType.XianYun);

            int already = giftpack.linYunNum + giftpack.xianYunNum;
            int needMore = giftpack.totalnum - already;
            for (int i = 0; i < needMore; i++)
            {
               int value =  GetRandomCardIdByQuality(DrawQualityByPackLevel(giftpack.level));
               if (!dict.ContainsKey(value))
               {
                   dict.Add(value, 1);
               }
               else
               {
                   dict[value]++;
               }
            }
            foreach (var value in dict)
            {
                var card = data.cardUpProgressesList.FirstOrDefault(c => c.id == value.Key);
                if (card != null)
                {
                    card.currentNum += value.Value;
                }
                else
                {
                    data.cardUpProgressesList.Add(new CardUpProgress(value.Key, value.Value));
                }
            }
            return dict;
        }
        private void DrawFixedCards(Dictionary<int,int> dict, int count, CardLevelType type)
        {
            if (count <= 0) return;
            var pool = DataController.Instance.cardLevelDataList
                .Where(c => c.levelType == type)
                .ToList();

            for (int i = 0; i < count; i++)
            {
                var card = pool[UnityEngine.Random.Range(0, pool.Count)];

                if (!dict.ContainsKey(card.id))
                    dict[card.id] = 0;

                dict[card.id]++;
            }
        }
        private CardLevelType DrawQualityByPackLevel(int level)
        {
            int roll = UnityEngine.Random.Range(0, 100); // 0..99

            switch (level)
            {
                case 1: // 低级
                    if (roll < 80) return CardLevelType.FanPing;
                    return CardLevelType.LingYun; // 20%

                case 2:                                          // 中级
                    if (roll < 80) return CardLevelType.FanPing; // 0-79
                    if (roll < 95) return CardLevelType.LingYun; // 80-94 => 15%
                    return CardLevelType.XianYun;                // 95-99 => 5%

                case 3:                                          // 高级
                    if (roll < 75) return CardLevelType.FanPing; // 0-74 => 75%
                    if (roll < 90) return CardLevelType.LingYun; // 75-89 => 15%
                    return CardLevelType.XianYun;                // 90-99 => 10%

                default:
                    return CardLevelType.FanPing;
            }
        }
        private int GetRandomCardIdByQuality(CardLevelType levelType)
        {
            var pool = DataController.Instance.cardLevelDataList.Where(c => c.levelType == levelType).ToList();
            if (pool.Count == 0)
                return -1;
            var selected = pool[UnityEngine.Random.Range(0, pool.Count)];
            return selected.id;
        }
    }
    
}