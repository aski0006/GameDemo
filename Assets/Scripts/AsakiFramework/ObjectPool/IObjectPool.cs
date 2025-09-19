using System;
using UnityEngine;

namespace AsakiFramework.ObjectPool
{
    /// <summary>
    /// 对象池接口
    /// 定义了对象池的基本操作
    /// </summary>
    public interface IObjectPool
    {
        /// <summary>
        /// 对象池名称
        /// </summary>
        string PoolName { get; }

        /// <summary>
        /// 池中对象总数（活跃 + 非活跃）
        /// </summary>
        int TotalCount { get; }

        /// <summary>
        /// 活跃对象数量
        /// </summary>
        int ActiveCount { get; }

        /// <summary>
        /// 非活跃对象数量
        /// </summary>
        int InactiveCount { get; }

        /// <summary>
        /// 清空对象池
        /// </summary>
        void Clear();
    }

    /// <summary>
    /// 泛型对象池接口
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    public interface IObjectPool<T> : IObjectPool where T : class
    {
        /// <summary>
        /// 从池中获取对象
        /// </summary>
        /// <returns>池中的对象</returns>
        T Get();

        /// <summary>
        /// 将对象归还到池中
        /// </summary>
        /// <param name="obj">要归还的对象</param>
        void Return(T obj);

        /// <summary>
        /// 创建新对象的委托
        /// </summary>
        Func<T> CreateFunc { get; set; }

        /// <summary>
        /// 对象激活时的回调
        /// </summary>
        Action<T> OnGetAction { get; set; }

        /// <summary>
        /// 对象归还时的回调
        /// </summary>
        Action<T> OnReturnAction { get; set; }

        /// <summary>
        /// 对象销毁时的回调
        /// </summary>
        Action<T> OnDestroyAction { get; set; }
    }
}
