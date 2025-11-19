using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    [DefaultExecutionOrder(-100)]
    public class AssetHandle : MonoBehaviour
    {
        public List<Object> assets = new List<Object>();
        
        private Dictionary<string, Object> _assetDict;

        private void Awake()
        {
            BuildDict();
        }

        private void BuildDict()
        {
            if (_assetDict == null) _assetDict = new Dictionary<string, Object>();
            else _assetDict.Clear();

            foreach (var entry in assets)
            {
                if (entry == null)
                {
                    Debug.LogWarning($"{name}: assets contains null entry");
                    continue;
                }

                var key = entry.name;
                if (_assetDict.ContainsKey(key))
                {
                    Debug.LogWarning($"{name}: duplicate asset name '{key}' (type {entry.GetType().Name}) - skipping");
                    continue;
                }

                _assetDict.Add(key, entry);
                Debug.Log($"{name}: added asset '{key}' ({entry.GetType().Name})");
            }
        }

        public Object Get(string key)
        {
            if (_assetDict == null) BuildDict();
            if (!_assetDict.TryGetValue(key, out var obj))
            {
                Debug.LogError($"{name}: Asset not found for key '{key}'. Available keys: {string.Join(", ", _assetDict.Keys)}");
                return null;
            }

            return obj;
        }

        public T Get<T>(string key) where T : Object
        {
            var obj = Get(key);
            if (obj == null)
            {
                Debug.LogError($" yj => {name}: Asset not found for key '{key}'");
                return null;
            }
                
            //  自动把 Texture2D 转 Sprite
            if (typeof(T) == typeof(Sprite))
            {
                if (obj is Sprite sprite)
                {
                    return sprite as T;
                }
                else if (obj is Texture2D tex)
                {
                    // 创建 sprite
                    var rect = new Rect(0, 0, tex.width, tex.height);
                    var pivot = new Vector2(0.5f, 0.5f);
                    Sprite newSprite = Sprite.Create(tex, rect, pivot);
                    return newSprite as T;
                }
            }

            
            if (obj is T t)
            {
                Debug.Log($"{name}: asset '{key}' ({obj.GetType().Name})");
                return t;
            }

            Debug.LogError($"{name}: Asset '{key}' exists but is {obj.GetType().Name}, requested {typeof(T).Name}");
            return null;
        }

        // 在编辑器里也能看到字典内容（仅编辑器）
        private void OnValidate()
        {
            if (Application.isPlaying) return;
            if (assets == null) return;

            _assetDict = new Dictionary<string, Object>();
            foreach (var entry in assets)
            {
                if (entry == null) continue;
                if (!_assetDict.ContainsKey(entry.name))
                    _assetDict.Add(entry.name, entry);
            }
        }
    }
}
