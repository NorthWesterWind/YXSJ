using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    /// <summary>
    ///     计时器单元（存储单个计时器的数据）
    /// </summary>
    public class Timer
    {
        public Timer(float duration, Action callback, bool isLoop, string id = null)
        {
            ID = id ?? Guid.NewGuid().ToString();
            Duration = duration;
            Callback = callback;
            IsLoop = isLoop;
        }

        public string ID { get; }              // 唯一标识
        public float Duration { get; set; }    // 目标时长
        public Action Callback { get; set; }   // 回调函数
        public float ElapsedTime { get; set; } // 已流逝时间
        public bool IsLoop { get; set; }       // 是否循环
        public bool IsPaused { get; set; }     // 是否暂停
    }

    /// <summary>
    ///     计时器管理器（静态类）
    /// </summary>
    public static class TimerSystem
    {
        private static readonly List<Timer> activeTimers = new();
        private static readonly Queue<Timer> timerPool = new();

        /// <summary>
        ///     创建计时器
        /// </summary>
        /// <param name="duration">持续时间（秒）</param>
        /// <param name="callback">回调函数</param>
        /// <param name="isLoop">是否循环</param>
        /// <param name="id">自定义ID（可选）</param>
        /// <returns>计时器ID</returns>
        public static string Create(float duration, Action callback, bool isLoop = false, string id = null)
        {
            if (callback == null)
            {
                Debug.LogWarning("计时器回调函数不能为null");
                return null;
            }

            var timer = GetOrCreateTimer(duration, callback, isLoop, id);
            activeTimers.Add(timer);
            return timer.ID;
        }

        /// <summary>
        ///     取消计时器
        /// </summary>
        public static bool Cancel(string timerID)
        {
            for (var i = 0; i < activeTimers.Count; i++)
                if (activeTimers[i].ID == timerID)
                {
                    ReturnTimerToPool(activeTimers[i]);
                    activeTimers.RemoveAt(i);
                    return true;
                }

            return false;
        }
        
        
        public static bool Exists(string timerID)
        {
            return activeTimers.Exists(t => t.ID == timerID);
        }
        
        /// <summary>
        ///     暂停/恢复计时器
        /// </summary>
        public static bool SetPause(string timerID, bool pause)
        {
            var timer = activeTimers.Find(t => t.ID == timerID);
            if (timer != null)
            {
                timer.IsPaused = pause;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     每帧更新（由MonoBehaviour驱动）
        /// </summary>
        public static void Update(float deltaTime)
        {
            for (var i = activeTimers.Count - 1; i >= 0; i--)
            {
                var timer = activeTimers[i];

                if (timer.IsPaused) continue;

                timer.ElapsedTime += deltaTime;

                if (timer.ElapsedTime >= timer.Duration)
                {
                    try
                    {
                        timer.Callback?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"计时器回调执行失败: {ex.Message}");
                    }

                    if (timer.IsLoop)
                    {
                        timer.ElapsedTime = 0;
                    }
                    else
                    {
                        ReturnTimerToPool(timer);
                        activeTimers.RemoveAt(i);
                    }
                }
            }
        }

        // 对象池管理
        private static Timer GetOrCreateTimer(float duration, Action callback, bool isLoop, string id)
        {
            var timer = timerPool.Count > 0 ? timerPool.Dequeue() : null;
            if (timer == null) return new Timer(duration, callback, isLoop, id);

            timer.Duration = duration;
            timer.Callback = callback;
            timer.IsLoop = isLoop;
            timer.ElapsedTime = 0;
            timer.IsPaused = false;
            return timer;
        }

        private static void ReturnTimerToPool(Timer timer)
        {
            timer.Callback = null;
            timerPool.Enqueue(timer);
        }
    }
}