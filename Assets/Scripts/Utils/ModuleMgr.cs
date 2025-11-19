using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    /// <summary>
    /// 模块总控中心，负责模块的创建、获取与销毁。
    /// </summary>
    public class ModuleMgr : SingletonBase<ModuleMgr>, Initializable
    {
        private readonly Dictionary<Type, BaseModule> modules = new();
        
        /// <summary>
        /// 注册模块（自动生成并绑定对应数据对象）
        /// </summary>
        public T RegisterModule<T>() where T : BaseModule, new()
        {
            var type = typeof(T);
            if (modules.ContainsKey(type))
            {
                Debug.LogWarning($"⚠️ 模块 {type.Name} 已注册过！");
                return (T)modules[type];
            }

            // 创建模块实例
            var module = new T();

            // 自动创建模块指定的数据类型（如果存在）
            var dataType = module.GetDataType();
            if (dataType != null && typeof(BaseData).IsAssignableFrom(dataType))
            {
                var data = (BaseData)Activator.CreateInstance(dataType);
                module.Initialize(data);
                Debug.Log($"✅ 注册模块: {type.Name}，创建数据: {dataType.Name}");
            }
            else
            {
                module.Initialize(null);
                Debug.Log($"✅ 注册模块: {type.Name}（无数据类型）");
            }

            modules[type] = module;
            return module;
        }

        /// <summary>
        /// 获取模块实例
        /// </summary>
        public T GetModule<T>() where T : BaseModule
        {
            modules.TryGetValue(typeof(T), out var module);
            return module as T;
        }

        /// <summary>
        /// 注销模块
        /// </summary>
        public void UnregisterModule<T>() where T : BaseModule
        {
            var type = typeof(T);
            if (modules.TryGetValue(type, out var module))
            {
                module.OnDestroy();
                modules.Remove(type);
                Debug.Log($"❎ 注销模块: {type.Name}");
            }
        }

        /// <summary>
        /// 清空所有模块
        /// </summary>
        public void Clear()
        {
            foreach (var module in modules.Values)
                module.OnDestroy();
            modules.Clear();
        }

        public void Initialize() { }
        public int Priority => 0;
    }
}
