using System;
using UnityEngine;

namespace Utils
{
    public class StaminaUtil : SingletonBase<StaminaUtil>
    {
        //最大体力值
        public int maxHp =70;
        //当前体力值
        [SerializeField] private int nowHp;

        //封装当前体力值属性 外界无法直接更改 
        public int NowHp
        {
            get { return nowHp; }
        }

        //体力恢复一点的所需时间
        private float recoverTime = 60 * 5;
        //体力完全恢复的时间点
        public DateTime recoverEndTime;
        //当前体力恢复剩余时间
        public string recoverTimeStr;

        private void Start()
        {
            Init();
        }

        private void Init()
        {
            //获取当前体力
            nowHp = PlayerPrefs.GetInt("NowHp", maxHp);
            //获取体力恢复完全的时间
            string time = PlayerPrefs.GetString("RecoverEndTime", string.Empty);
            if (!string.IsNullOrEmpty(time))
            {
                recoverEndTime = DateTime.Parse(time);
            }
            else
            {
                recoverEndTime = DateTime.MinValue;
            }
            //检查体力是否需要恢复
            CheckRecoverHp();
        }

        //消耗体力的方法
        [ContextMenu("消耗体力")]
        public void UseHp(int count)
        {
            if (nowHp > 0)
            {
                //体力减一
                nowHp -= count;
                PlayerPrefs.SetInt("NowHp", nowHp);
                //如果是第一次消耗体力
                if (nowHp == maxHp - count)
                {
                    //体力完全恢复时间
                    recoverEndTime = DateTime.Now.AddSeconds(recoverTime);
                    //保存恢复完成时间
                    PlayerPrefs.SetString("RecoverEndTime", recoverEndTime.ToString());
                }
                else if (nowHp >= 0)  //不是第一次消耗体力
                {
                    //获取体力恢复完全的时间
                    string time = PlayerPrefs.GetString("RecoverEndTime", string.Empty);
                    DateTime lastTime = DateTime.Parse(time);
                    recoverEndTime = lastTime.AddSeconds(recoverTime);
                    //保存恢复完成时间
                    PlayerPrefs.SetString("RecoverEndTime", recoverEndTime.ToString());
                }
            }
            else
            {
                nowHp = 0;
                PlayerPrefs.SetInt("NowHp", 0);
            }
        }

        //获取恢复所有体力的所需时间
        float GetrecoverTime()
        {
            //获取当前时间与 恢复完成时间的时间间隔
            TimeSpan recoverInterval = recoverEndTime - DateTime.Now;
            // 计算剩余恢复时间 (秒)
            float remainingTime = (float)recoverInterval.TotalSeconds;
            //转换成float类型后返回
            return remainingTime;
        }

        //检查体力是否需要恢复
        void CheckRecoverHp()
        {
            //如果是默认值 说明没消耗体力 不继续检测
            if (recoverEndTime == DateTime.MinValue)
            {
                return;
            }
            //获取恢复所有体力的所需时间
            float timer = GetrecoverTime();
            //如果这个结果是负的 说明体力恢复完成
            if (timer <= 0)
            {
                nowHp = maxHp;
                PlayerPrefs.SetInt("NowHp", maxHp);
                recoverEndTime = DateTime.MinValue;
                PlayerPrefs.SetString("RecoverEndTime", string.Empty);
            }
            else
            {
                //如果这个结果大于0  说明还有时间 需要进行倒计时
                //计算回满需要多少体力   回满所需时间 / 一点体力恢复时间  =》 向上取整
                int num = (int)Math.Ceiling(timer / recoverTime);
                //当回满所需值 num大于或者等于5时 此时的体力一定是0点
                if (num >= maxHp)
                    nowHp = maxHp;
                //否则更新当前的体力值   最大值 - 回满所需值 = 当前值
                else
                    nowHp = maxHp - num;
                //保存体力值
                PlayerPrefs.SetInt("NowHp", nowHp);
            }
        }

        //获取当前这一点体力的恢复时间
        TimeSpan GetNowRecoverTime()
        {
            // 计算当前体力对应的恢复时间
            int hpDifference = maxHp - nowHp - 1;
            DateTime nowRecoverTime = recoverEndTime.AddSeconds(-recoverTime * hpDifference);
            // 获取当前时间与恢复完成时间的时间间隔
            TimeSpan recoverInterval = nowRecoverTime - DateTime.Now;
            if(recoverInterval.TotalSeconds < 0)
            {
                recoverInterval = TimeSpan.Zero;
            }
            return recoverInterval;
        }

        private void Update()
        {
            if (nowHp < maxHp)
            {
                TimeSpan timer = GetNowRecoverTime();
                int minutes = (int)timer.TotalMinutes;
                int seconds = timer.Seconds;
                recoverTimeStr = $"{minutes}:{seconds:D2}";
                if (timer.TotalSeconds <= 0)
                {
                    //体力加一
                    AddHp();

                }
            }
        }

        [ContextMenu("增加体力")]
        void AddHp()
        {
            nowHp++;
            if(nowHp > maxHp)
            {
                nowHp = maxHp;
            }
            PlayerPrefs.SetInt("NowHp", nowHp);

        }


    }
} 

