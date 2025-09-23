using System;
using UnityEngine;

namespace AsakiFramework.ObjectPool
{
    /// <summary>
    /// 对象池静态类
    /// 提供简化的对象池访问接口
    /// </summary>
    public static class ObjectPool
    {
        /// <summary>
        /// 创建GameObject对象池
        /// </summary>
        /// <param name="prefab">预设对象</param>
        /// <param name="initialSize">初始大小</param>
        /// <param name="maxSize">最大大小</param>
        /// <param name="poolName">池名称</param>
        /// <returns>GameObject对象池</returns>
        public static GameObjectPool Create(GameObject prefab, int initialSize = 10, int maxSize = 100, string poolName = null)
        {
            return ObjectPoolManager.Instance.CreateGameObjectPool(prefab, initialSize, maxSize, poolName);
        }

        /// <summary>
        /// 创建Component对象池
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <param name="prefab">预设对象</param>
        /// <param name="initialSize">初始大小</param>
        /// <param name="maxSize">最大大小</param>
        /// <param name="poolName">池名称</param>
        /// <returns>Component对象池</returns>
        public static ComponentPool<T> Create<T>(GameObject prefab, int initialSize = 10, int maxSize = 100, string poolName = null) where T : Component
        {
            return ObjectPoolManager.Instance.CreateComponentPool<T>(prefab, initialSize, maxSize, poolName);
        }

        public static GameObjectPool Create(ObjectPoolConfig config)
        {
            return ObjectPoolManager.Instance.CreateGameObjectPool(config.Prefab,
                config.InitialCapacity, config.MaxCapacity, config.PoolName);
        }

        public static ComponentPool<T> Create<T>(ObjectPoolConfig config) where T : Component
        {
            return ObjectPoolManager.Instance.CreateComponentPool<T>(config.Prefab,
                config.InitialCapacity, config.MaxCapacity, config.PoolName);
        }

        /// <summary>
        /// 创建通用对象池
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="createFunc">创建函数</param>
        /// <param name="initialSize">初始大小</param>
        /// <param name="maxSize">最大大小</param>
        /// <param name="poolName">池名称</param>
        /// <returns>对象池</returns>
        public static GenericObjectPool<T> Create<T>(Func<T> createFunc, int initialSize = 10, int maxSize = 100, string poolName = null) where T : class
        {
            return ObjectPoolManager.Instance.CreatePool(createFunc, initialSize, maxSize, poolName);
        }

        /// <summary>
        /// 从池中获取GameObject
        /// </summary>
        /// <param name="prefab">预设对象</param>
        /// <param name="parent">父对象</param>
        /// <returns>获取的GameObject</returns>
        public static GameObject Get(GameObject prefab, Transform parent = null)
        {
            return ObjectPoolManager.Instance.GetGameObject(prefab, parent);
        }

        /// <summary>
        /// 从池中获取GameObject
        /// </summary>
        /// <param name="prefab">预设对象</param>
        /// <param name="position">位置</param>
        /// <param name="rotation">旋转</param>
        /// <param name="parent">父对象</param>
        /// <returns>获取的GameObject</returns>
        public static GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            var pool = ObjectPoolManager.Instance.GetGameObjectPool(prefab);
            if (pool == null)
            {
                pool = ObjectPoolManager.Instance.CreateGameObjectPool(prefab);
            }

            return pool?.Get(position, rotation, parent);
        }

        /// <summary>
        /// 从池中获取Component
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <param name="prefab">预设对象</param>
        /// <param name="parent">父对象</param>
        /// <returns>获取的Component</returns>
        public static T Get<T>(GameObject prefab, Transform parent = null) where T : Component
        {
            return ObjectPoolManager.Instance.GetComponent<T>(prefab, parent);
        }

        /// <summary>
        /// 从池中获取Component
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <param name="prefab">预设对象</param>
        /// <param name="position">位置</param>
        /// <param name="rotation">旋转</param>
        /// <param name="parent">父对象</param>
        /// <returns>获取的Component</returns>
        public static T Get<T>(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null) where T : Component
        {
            var pool = ObjectPoolManager.Instance.GetComponentPool<T>(prefab);
            if (pool == null)
            {
                pool = ObjectPoolManager.Instance.CreateComponentPool<T>(prefab);
            }

            return pool?.Get(position, rotation, parent);
        }

        /// <summary>
        /// 归还GameObject到池中
        /// </summary>
        /// <param name="obj">要归还的GameObject</param>
        public static void Return(GameObject obj)
        {
            ObjectPoolManager.Instance.ReturnGameObject(obj);
        }

        /// <summary>
        /// 归还Component到池中
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <param name="component">要归还的Component</param>
        public static void Return<T>(T component) where T : Component
        {
            ObjectPoolManager.Instance.ReturnComponent(component);
        }

        /// <summary>
        /// 清空所有对象池
        /// </summary>
        public static void ClearAll()
        {
            ObjectPoolManager.Instance.ClearAllPools();
        }

        /// <summary>
        /// 归还所有活跃对象
        /// </summary>
        public static void ReturnAll()
        {
            ObjectPoolManager.Instance.ReturnAllToPools();
        }

        /// <summary>
        /// 获取所有池的统计信息
        /// </summary>
        /// <returns>统计信息字符串</returns>
        public static string GetStats()
        {
            return ObjectPoolManager.Instance.GetAllPoolStats();
        }
    }
}
