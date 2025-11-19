using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
namespace Utils
{
    public class EventCenter : SingletonBase<EventCenter>
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

        #region 移除事件监听

        public void RemoveListener(string eventType, EventHandler handler)
        {
            if (string.IsNullOrEmpty(eventType) || handler == null) return;
            Debug.LogWarning("清除监听 >" +  eventType);
            if (eventHandlers.TryGetValue(eventType, out var existingHandler))
            {
                var newHandler = Delegate.Remove(existingHandler, handler);
                if (newHandler == null)
                    eventHandlers.Remove(eventType);
                else
                    eventHandlers[eventType] = newHandler;
            }
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

        #region 清空事件

        /// <summary>
        ///     清空特定类型的所有事件监听
        /// </summary>
        public void ClearEvent(string eventType)
        {
            if (string.IsNullOrEmpty(eventType)) return;

            if (eventHandlers.ContainsKey(eventType)) eventHandlers.Remove(eventType);
        }

        /// <summary>
        ///     清空所有事件监听
        /// </summary>
        public void ClearAllEvents()
        {
            eventHandlers.Clear();
        }

        #endregion

        #region 安全检查方法

        /// <summary>
        ///     检查特定事件类型是否有监听器
        /// </summary>
        public bool HasListeners(string eventType)
        {
            return !string.IsNullOrEmpty(eventType) &&
                   eventHandlers.TryGetValue(eventType, out var handler) &&
                   handler != null;
        }

        /// <summary>
        ///     获取特定事件类型的监听器数量
        /// </summary>
        public int GetListenerCount(string eventType)
        {
            if (string.IsNullOrEmpty(eventType)) return 0;

            if (eventHandlers.TryGetValue(eventType, out var handler) && handler != null)
                return handler.GetInvocationList().Length;
            return 0;
        }

        #endregion
    }
}