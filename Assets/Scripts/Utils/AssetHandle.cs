using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    [DefaultExecutionOrder(-100)]
    public class AssetHandle : MonoBehaviour
    {
        public List<Object> assets = new List<Object>();

        private Dictionary<string, Object> _assetDict;
        private Dictionary<string, Sprite> _spriteCache;

        private void Awake()
        {
            BuildDict();
        }

        private void BuildDict()
        {
            if (_assetDict == null)
            {
                _assetDict = new Dictionary<string, Object>();
            }
            else
            {
                _assetDict.Clear();
            }

            if (_spriteCache == null)
            {
                _spriteCache = new Dictionary<string, Sprite>();
            }
            else
            {
                _spriteCache.Clear();
            }

            foreach (var entry in assets)
            {
                if (entry == null)
                {
                    continue;
                }

                var key = entry.name;
                if (_assetDict.ContainsKey(key))
                {
                    continue;
                }

                _assetDict.Add(key, entry);
            }
        }

        public Object Get(string key)
        {
            if (_assetDict == null)
            {
                BuildDict();
            }

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
                return null;
            }

            if (typeof(T) == typeof(Sprite))
            {
                if (obj is Sprite sprite)
                {
                    return sprite as T;
                }

                if (obj is Texture2D tex)
                {
                    if (_spriteCache == null)
                    {
                        _spriteCache = new Dictionary<string, Sprite>();
                    }

                    if (!_spriteCache.TryGetValue(key, out var cachedSprite) || cachedSprite == null)
                    {
                        var rect = new Rect(0, 0, tex.width, tex.height);
                        var pivot = new Vector2(0.5f, 0.5f);
                        cachedSprite = Sprite.Create(tex, rect, pivot);
                        _spriteCache[key] = cachedSprite;
                    }

                    return cachedSprite as T;
                }
            }

            if (obj is T t)
            {
                return t;
            }

            Debug.LogError($"{name}: Asset '{key}' exists but is {obj.GetType().Name}, requested {typeof(T).Name}");
            return null;
        }

        private void OnValidate()
        {
            if (Application.isPlaying || assets == null)
            {
                return;
            }

            _assetDict = new Dictionary<string, Object>();
            foreach (var entry in assets)
            {
                if (entry == null) continue;
                if (!_assetDict.ContainsKey(entry.name))
                {
                    _assetDict.Add(entry.name, entry);
                }
            }
        }
    }
}
