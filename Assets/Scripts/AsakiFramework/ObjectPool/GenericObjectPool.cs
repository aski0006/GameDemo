using System;
using UnityEngine;

namespace AsakiFramework.ObjectPool
{
    /// <summary>
    /// 通用对象池实现
    /// 用于管理非Unity对象的通用对象池
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    public class GenericObjectPool<T> : ObjectPoolBase<T> where T : class
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="poolName">对象池名称</param>
        public GenericObjectPool(string poolName = null) : base(poolName)
        {
        }

        /// <summary>
        /// 销毁对象
        /// </summary>
        /// <param name="obj">要销毁的对象</param>
        protected override void DestroyObject(T obj)
        {
            // 对于非Unity对象，调用销毁回调即可
            OnDestroyAction?.Invoke(obj);
            
            // 如果对象实现了IDisposable，调用Dispose
            if (obj is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        /// <summary>
        /// 获取池的统计信息
        /// </summary>
        /// <returns>统计信息字符串</returns>
        public override string GetStats()
        {
            return $"Generic Object Pool: {PoolName}\n" +
                $"Type: {typeof(T).Name}\n" +
                $"Total: {TotalCount}\n" +
                $"Active: {ActiveCount}\n" +
                $"Inactive: {InactiveCount}\n" +
                $"Max Size: {MaxPoolSize}\n" +
                $"Auto Expand: {AutoExpand}\n" +
                $"Create Function: {(CreateFunc != null ? "Set" : "Not Set")}";
        }
    }
}
