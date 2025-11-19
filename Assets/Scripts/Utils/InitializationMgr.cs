using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utils
{
    /// <summary>
    /// 统一初始化管理器（支持优先级）
    /// </summary>
    public class InitializationMgr : MonoSingleton<InitializationMgr>
    {

        private readonly List<Initializable> _initializables = new();

        /// <summary>
        /// 注册一个初始化对象
        /// </summary>
        public void Register(Initializable initializable)
        {
            if (initializable == null)
                return;

            if (_initializables.Contains(initializable))
                return;

            _initializables.Add(initializable);
        }

        /// <summary>
        /// 执行所有注册对象的初始化（按优先级排序）
        /// </summary>
        public void ExecuteAll()
        {
            Debug.Log("🟢 [InitializationManager] 开始执行所有初始化...");

            // 按优先级从小到大排序
            var sorted = _initializables.OrderBy(i => i.Priority).ToList();

            foreach (var item in sorted)
            {
                try
                {
                    Debug.Log($"➡️ 初始化: {item.GetType().Name} (Priority={item.Priority})");
                    item.Initialize();
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"❌ 初始化失败 [{item.GetType().Name}]: {ex}");
                }
            }

            Debug.Log("✅ [InitializationManager] 所有初始化完成");
        }
    }
}