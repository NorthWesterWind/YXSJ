
using Sirenix.OdinInspector;
using UnityEngine;

namespace Utils
{
    public class MonoUtil<T> : SerializedMonoBehaviour where T : MonoUtil<T>
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();
                    if (_instance == null)
                    {
                        var go = new GameObject(typeof(T).Name);
                        _instance = go.AddComponent<T>();
                        // 切场景时会自动销毁，不再使用 DontDestroyOnLoad
                    }
                }

                return _instance;
            }
        }

        public virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject); // 保证同场景唯一
            }
            else
            {
                _instance = this as T;
                // 不使用 DontDestroyOnLoad，切场景自动销毁
            }
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
                _instance = null; // 清空静态引用，避免残留
        }
    }
}
