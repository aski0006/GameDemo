using System;
using System.Collections.Generic;
using UnityEngine;

namespace AsakiFramework.ObjectPool
{
    /// <summary>
    /// 对象池基类
    /// 提供通用的对象池功能
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    public abstract class ObjectPoolBase<T> : IObjectPool<T> where T : class
    {
        /// <summary>
        /// 对象池名称
        /// </summary>
        public string PoolName { get; protected set; }

        /// <summary>
        /// 池中对象总数
        /// </summary>
        public int TotalCount => ActiveCount + InactiveCount;

        /// <summary>
        /// 活跃对象数量
        /// </summary>
        public int ActiveCount => _activeObjects.Count;

        /// <summary>
        /// 非活跃对象数量
        /// </summary>
        public int InactiveCount => _inactiveObjects.Count;

        /// <summary>
        /// 创建新对象的委托
        /// </summary>
        public Func<T> CreateFunc { get; set; }

        /// <summary>
        /// 对象激活时的回调
        /// </summary>
        public Action<T> OnGetAction { get; set; }

        /// <summary>
        /// 对象归还时的回调
        /// </summary>
        public Action<T> OnReturnAction { get; set; }

        /// <summary>
        /// 对象销毁时的回调
        /// </summary>
        public Action<T> OnDestroyAction { get; set; }

        /// <summary>
        /// 最大池大小，超过此数量时多余对象会被销毁
        /// </summary>
        public int MaxPoolSize { get; set; } = 100;

        /// <summary>
        /// 是否自动扩展池大小
        /// </summary>
        public bool AutoExpand { get; set; } = true;

        /// <summary>
        /// 初始池大小
        /// </summary>
        public int InitialSize { get; set; } = 10;

        protected readonly Stack<T> _inactiveObjects = new Stack<T>();
        protected readonly HashSet<T> _activeObjects = new HashSet<T>();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="poolName">对象池名称</param>
        protected ObjectPoolBase(string poolName = null)
        {
            PoolName = poolName ?? $"ObjectPool_{typeof(T).Name}_{Guid.NewGuid():N}";
        }

        /// <summary>
        /// 初始化对象池
        /// </summary>
        public virtual void Initialize()
        {
            if (CreateFunc == null)
            {
                throw new InvalidOperationException("CreateFunc must be set before initializing the pool.");
            }

            // 预先创建初始数量的对象
            for (int i = 0; i < InitialSize; i++)
            {
                var obj = CreateFunc();
                if (obj != null)
                {
                    _inactiveObjects.Push(obj);
                }
            }
        }

        /// <summary>
        /// 从池中获取对象
        /// </summary>
        /// <returns>池中的对象</returns>
        public virtual T Get()
        {
            T obj;

            if (_inactiveObjects.Count > 0)
            {
                obj = _inactiveObjects.Pop();
            }
            else if (AutoExpand)
            {
                obj = CreateFunc();
                if (obj == null)
                {
                    throw new InvalidOperationException("Failed to create a new object. CreateFunc returned null.");
                }
            }
            else
            {
                throw new InvalidOperationException($"Pool '{PoolName}' is empty and AutoExpand is disabled.");
            }

            _activeObjects.Add(obj);
            OnGetAction?.Invoke(obj);
            
            return obj;
        }

        /// <summary>
        /// 将对象归还到池中
        /// </summary>
        /// <param name="obj">要归还的对象</param>
        public virtual void Return(T obj)
        {
            if (obj == null)
            {
                Debug.LogWarning($"[{PoolName}] Attempted to return null object to pool.");
                return;
            }

            if (!_activeObjects.Contains(obj))
            {
                Debug.LogWarning($"[{PoolName}] Attempted to return an object that was not obtained from this pool.");
                return;
            }

            _activeObjects.Remove(obj);

            // 如果池已满，销毁对象
            if (_inactiveObjects.Count >= MaxPoolSize)
            {
                DestroyObject(obj);
            }
            else
            {
                _inactiveObjects.Push(obj);
                OnReturnAction?.Invoke(obj);
            }
        }

        /// <summary>
        /// 清空对象池
        /// </summary>
        public virtual void Clear()
        {
            // 销毁所有非活跃对象
            while (_inactiveObjects.Count > 0)
            {
                var obj = _inactiveObjects.Pop();
                DestroyObject(obj);
            }

            // 销毁所有活跃对象
            var activeObjectsArray = new T[_activeObjects.Count];
            _activeObjects.CopyTo(activeObjectsArray);
            foreach (var obj in activeObjectsArray)
            {
                DestroyObject(obj);
            }
            _activeObjects.Clear();
        }

        /// <summary>
        /// 销毁对象
        /// </summary>
        /// <param name="obj">要销毁的对象</param>
        protected virtual void DestroyObject(T obj)
        {
            OnDestroyAction?.Invoke(obj);
        }

        /// <summary>
        /// 预创建指定数量的对象
        /// </summary>
        /// <param name="count">要预创建的对象数量</param>
        public virtual void Prewarm(int count)
        {
            if (CreateFunc == null)
            {
                Debug.LogWarning($"[{PoolName}] Cannot prewarm - CreateFunc is not set.");
                return;
            }

            for (int i = 0; i < count; i++)
            {
                var obj = CreateFunc();
                if (obj != null)
                {
                    _inactiveObjects.Push(obj);
                }
            }
        }

        /// <summary>
        /// 获取池的统计信息
        /// </summary>
        /// <returns>统计信息字符串</returns>
        public virtual string GetStats()
        {
            return $"Pool: {PoolName}\n" +
                   $"Type: {typeof(T).Name}\n" +
                   $"Total: {TotalCount}\n" +
                   $"Active: {ActiveCount}\n" +
                   $"Inactive: {InactiveCount}\n" +
                   $"Max Size: {MaxPoolSize}\n" +
                   $"Auto Expand: {AutoExpand}";
        }
    }
}