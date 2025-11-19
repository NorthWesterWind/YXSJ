using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utils
{
    public class RedPointCenter : SingletonBase<RedPointCenter>
    {
        // 事件委托定义
        public delegate void EventHandler(params object[] args);

        // 存储所有事件监听器的字典
        private readonly Dictionary<string, Delegate> eventHandlers = new();

        #region 注册事件监听

        public void AddListener(string eventType, EventHandler handler)
        {
            if (string.IsNullOrEmpty(eventType) || handler == null) return;

            if (eventHandlers.TryGetValue(eventType, out var existingHandler))
            {
                if (existingHandler != null && existingHandler.GetInvocationList().Contains((Delegate)handler))
                    return;
                eventHandlers[eventType] = Delegate.Combine(existingHandler, handler);
            }
            else
                eventHandlers[eventType] = handler;
        }

        #endregion

        #region 触发事件

        /// <summary>
        ///     触发带参数的事件
        /// </summary>
        public void TriggerEvent(string eventType, params object[] data)
        {
            if (string.IsNullOrEmpty(eventType)) return;

            if (eventHandlers.TryGetValue(eventType, out var handler))
            {
                if (handler is EventHandler eventHandler)
                    eventHandler.Invoke(data);
                else
                    Debug.LogWarning($"[EventCenter] 事件 {eventType} 的委托类型不匹配");
            }
        }

        #endregion
        
        public void RemoveListener(string eventType, EventHandler handler)
        {
            if (string.IsNullOrEmpty(eventType) || handler == null) return;

            if (eventHandlers.TryGetValue(eventType, out var existingHandler))
            {
                var newHandler = Delegate.Remove(existingHandler, handler);
                if (newHandler == null)
                    eventHandlers.Remove(eventType);
                else
                    eventHandlers[eventType] = newHandler;
            }
        }
    }
}