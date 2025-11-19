using System;
using UnityEngine;

namespace Utils
{
    /// <summary>
    /// 所有模块逻辑的基类
    /// </summary>
    public abstract class BaseModule
    {
        public BaseData Data { get; private set; }

        /// <summary>
        /// 模块初始化（绑定数据）
        /// </summary>
        public virtual void Initialize(BaseData data)
        {
            Data = data;
            OnInitialize();
        }

        /// <summary>
        /// 返回该模块对应的数据类型（用于ModuleMgr自动创建）
        /// </summary>
        public virtual Type GetDataType() => null;

        /// <summary>
        /// 子类可在此执行额外初始化逻辑
        /// </summary>
        protected virtual void OnInitialize() { }

        /// <summary>
        /// 销毁模块时的清理逻辑
        /// </summary>
        public virtual void OnDestroy()
        {
            Data = null;
        }
    }
}