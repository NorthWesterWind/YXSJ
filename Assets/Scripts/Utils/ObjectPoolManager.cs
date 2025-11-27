using System.Collections.Generic;
using Controller.Pickups;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Utils
{
    /// <summary>
    ///     对象池管理类，用于管理多种类型的游戏对象池
    /// </summary>
    public class ObjectPoolManager : MonoSingleton<ObjectPoolManager>
    {
        // 对象池配置：初始大小和最大大小
        private int defaultPoolSize = 10;

        private int maxPoolSize = 300;

        // 存储所有对象池的字典，键为预制体，值为对象池
        private readonly Dictionary<string, ObjectPool> poolDictionary = new();

        private readonly Dictionary<GameObject, GameObject> instanceToPrefab = new();


        private void Awake()
        {
            // 监听场景切换
            SceneManager.activeSceneChanged += OnSceneChanged;
        }

        private void OnDestroy()
        {
            SceneManager.activeSceneChanged -= OnSceneChanged;
        }

        private void OnSceneChanged(Scene oldScene, Scene newScene)
        {
            ClearAllPools();
            poolDictionary.Clear();
            instanceToPrefab.Clear();
        }


        /// <summary>
        ///     从对象池获取一个对象实例
        /// </summary>
        /// <param name="prefab">需要的预制体</param>
        /// <returns>可用的游戏对象</returns>
        public GameObject GetObject(string name)
        {
            // 检查是否已经存在该预制体的对象池
            if (!poolDictionary.ContainsKey(name))
            {
                Debug.LogError($"不存在{name}对象池");
            }

            // 从对应的对象池获取对象
            var obj = poolDictionary[name].GetObject();
            if (obj == null)
                return null;
            return obj;
        }

        /// <summary>
        /// 将对象返回对象池
        /// </summary>
        /// <param name="obj">要返回的游戏对象</param>
        /// <param name="prefab">对应的预制体类型</param>
        public void ReturnObject(string name, GameObject obj)
        {
            if (obj == null) return;
            if (obj.GetComponent<BasePickup>() != null)
            {
                obj.GetComponent<BasePickup>().StopAllCoroutines();
            }
            if (instanceToPrefab.TryGetValue(obj, out var prefab))
            {
                poolDictionary[name].ReturnObject(obj);
            }
            else
            {
                // 如果没有对应的对象池，直接销毁对象
                Debug.LogWarning($"尝试返回对象到不存在的对象池，预制体: {obj.name}");
                Destroy(obj);
            }
        }

        /// <summary>
        ///     预 warm 对象池，提前创建一定数量的对象
        /// </summary>
        /// <param name="prefab">要预热的预制体</param>
        /// <param name="count">预热数量</param>
        public void WarmPool(string name, GameObject prefab, int count)
        {
            if (prefab == null) return;
        
            // 如果不存在该预制体的池子，先创建
            if (!poolDictionary.ContainsKey(name)) CreateNewPool(name, prefab);

            // 获取池子
            var pool = poolDictionary[name];

            // 当前池子已有数量
            var currentCount = pool.Count;

            // 需要补充的数量
            var need = count - currentCount;

            if (need > 0) pool.Warm(need);
        }


        /// <summary>
        ///     清空特定预制体的对象池
        /// </summary>
        /// <param name="prefab">要清空的预制体</param>
        public void ClearPool(string name)
        {
            if (poolDictionary.ContainsKey(name))
            {
                poolDictionary[name].Clear();
                poolDictionary.Remove(name);
            }
        }

        /// <summary>
        ///     清空所有对象池
        /// </summary>
        public void ClearAllPools()
        {
            foreach (var pool in poolDictionary.Values) pool.Clear();
        }

        private void CreateNewPool(string name, GameObject prefab)
        {
            // 为这种预制体创建专用的容器
            var poolTypeContainer = new GameObject($"Pool_{name}").transform;

            poolTypeContainer.SetParent(transform);
            poolTypeContainer.localPosition = Vector3.zero;
            poolTypeContainer.localScale = Vector3.one;
            // 创建新的对象池实例
            var newPool = new ObjectPool(prefab, defaultPoolSize, maxPoolSize, poolTypeContainer);
            poolDictionary.Add(name, newPool);

            Debug.Log($"创建了新的对象池: {name}");
        }

        /// <summary>
        ///     内部对象池类
        /// </summary>
        private class ObjectPool
        {
            // 所有已创建的对象列表（用于清理）
            private readonly List<GameObject> allObjects = new();

            // 可用的对象队列
            private readonly Queue<GameObject> availableObjects = new();
            private readonly Transform container;
            private readonly int maxSize;
            private readonly GameObject prefab;

            public ObjectPool(GameObject prefab, int initialSize, int maxSize, Transform container)
            {
                this.prefab = prefab;
                this.maxSize = maxSize;
                this.container = container;
                Warm(initialSize);
            }

            public int Count => availableObjects.Count;

            public GameObject GetObject()
            {
                GameObject obj;

                // 有可用对象 → 直接取
                if (availableObjects.Count > 0)
                {
                    obj = availableObjects.Dequeue();
                }
                else
                {
                    // 无可用对象 → 自动扩容
                    if (allObjects.Count < maxSize)
                    {
                        obj = CreateNewObject();
                    }
                    else
                    {
                        Debug.LogWarning($"对象池 {prefab.name} 已满，无法创建更多对象");
                        return null;
                    }
                }

                obj.SetActive(true);
                return obj;
            }



            public void ReturnObject(GameObject obj)
            {
                if (obj == null) return;

                // 已经在池中就不要重复加入（避免重复回收）
                if (availableObjects.Contains(obj))
                    return;

                obj.transform.SetParent(container);
                obj.SetActive(false);
                availableObjects.Enqueue(obj);
            }


            public void Warm(int count)
            {
                count = Mathf.Min(count, maxSize - allObjects.Count);

                for (var i = 0; i < count; i++)
                {
                    var obj = CreateNewObject();
                    ReturnObject(obj);
                }
            }

            public void Clear()
            {
                foreach (var obj in allObjects)
                    if (obj != null)
                        Destroy(obj);

                availableObjects.Clear();
                allObjects.Clear();
            }

            public string GetStats()
            {
                return $"预制体: {prefab.name}, 总数: {allObjects.Count}, 可用: {availableObjects.Count}, 最大: {maxSize}";
            }

            private GameObject CreateNewObject()
            {
                var obj = Instantiate(prefab, container, false);
                Instance.instanceToPrefab.TryAdd(obj, prefab);
                obj.name = $"{prefab.name}_{allObjects.Count:000}";
                obj.SetActive(false);
                allObjects.Add(obj);
                return obj;
            }
        }
    }
}