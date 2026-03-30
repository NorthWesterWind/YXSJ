using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utils
{
    public class EventCenter : SingletonBase<EventCenter>
    {
        private const bool EnableVerboseLogs = false;

        public delegate void EventHandler(params object[] args);

        private readonly Dictionary<string, Delegate> eventHandlers = new();

        public void AddListener(string eventType, EventHandler handler)
        {
            if (string.IsNullOrEmpty(eventType) || handler == null) return;

            if (eventHandlers.TryGetValue(eventType, out var existingHandler))
            {
                if (existingHandler != null && existingHandler.GetInvocationList().Contains((Delegate)handler))
                {
                    return;
                }

                eventHandlers[eventType] = Delegate.Combine(existingHandler, handler);
            }
            else
            {
                eventHandlers[eventType] = handler;
            }

            if (EnableVerboseLogs)
            {
                Debug.Log("yj == > 娣诲姞鐩戝惉閫昏緫 " + eventType);
            }
        }

        public void RemoveListener(string eventType, EventHandler handler)
        {
            if (string.IsNullOrEmpty(eventType) || handler == null) return;

            if (eventHandlers.TryGetValue(eventType, out var existingHandler))
            {
                var newHandler = Delegate.Remove(existingHandler, handler);
                if (newHandler == null)
                {
                    eventHandlers.Remove(eventType);
                }
                else
                {
                    eventHandlers[eventType] = newHandler;
                }
            }
        }

        public void TriggerEvent(string eventType, params object[] data)
        {
            if (string.IsNullOrEmpty(eventType)) return;

            if (!eventHandlers.TryGetValue(eventType, out var handler))
            {
                return;
            }

            if (handler is not EventHandler eventHandler)
            {
                Debug.LogWarning($"[EventCenter] 浜嬩欢 {eventType} 鐨勫鎵樼被鍨嬩笉鍖归厤");
                return;
            }

            var invocationList = eventHandler.GetInvocationList();
            for (int i = 0; i < invocationList.Length; i++)
            {
                try
                {
                    ((EventHandler)invocationList[i]).Invoke(data);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[EventCenter] Event '{eventType}' listener threw: {ex}");
                }
            }

            if (EnableVerboseLogs)
            {
                Debug.Log("yj == > 瑙﹀彂浜嬩欢 " + eventType);
            }
        }

        public void ClearEvent(string eventType)
        {
            if (string.IsNullOrEmpty(eventType)) return;

            if (eventHandlers.ContainsKey(eventType))
            {
                eventHandlers.Remove(eventType);
            }
        }

        public void ClearAllEvents()
        {
            eventHandlers.Clear();
        }

        public bool HasListeners(string eventType)
        {
            return !string.IsNullOrEmpty(eventType) &&
                   eventHandlers.TryGetValue(eventType, out var handler) &&
                   handler != null;
        }

        public int GetListenerCount(string eventType)
        {
            if (string.IsNullOrEmpty(eventType)) return 0;

            if (eventHandlers.TryGetValue(eventType, out var handler) && handler != null)
            {
                return handler.GetInvocationList().Length;
            }

            return 0;
        }
    }
}
