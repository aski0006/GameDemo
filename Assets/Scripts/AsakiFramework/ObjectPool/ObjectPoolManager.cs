using System;
using System.Collections.Generic;
using UnityEngine;

namespace AsakiFramework.ObjectPool
{
    /// <summary>
    /// 对象池管理器
    /// 统一管理多个对象池的单例管理器
    /// </summary>
    public class ObjectPoolManager : Singleton<ObjectPoolManager>
    {
        /// <summary>
        /// 对象池字典
        /// </summary>
        private readonly Dictionary<string, IObjectPool> _pools = new Dictionary<string, IObjectPool>();

        /// <summary>
        /// GameObject池字典
        /// </summary>
        private readonly Dictionary<GameObject, GameObjectPool> _gameObjectPools = new Dictionary<GameObject, GameObjectPool>();

        /// <summary>
        /// 组件池字典
        /// </summary>
        private readonly Dictionary<Type, Dictionary<GameObject, object>> _componentPools = new Dictionary<Type, Dictionary<GameObject, object>>();

        /// <summary>
        /// 对象池根节点
        /// </summary>
        public Transform PoolRoot { get; private set; }

        /// <summary>
        /// 是否自动创建根节点
        /// </summary>
        public bool AutoCreateRoot { get; set; } = true;

        /// <summary>
        /// 根节点名称
        /// </summary>
        public string RootName { get; set; } = "ObjectPoolRoot";

        protected override void Awake()
        {
            base.Awake();
            InitializePoolRoot();
        }

        /// <summary>
        /// 初始化对象池根节点
        /// </summary>
        private void InitializePoolRoot()
        {
            if (PoolRoot == null && AutoCreateRoot)
            {
                var rootObject = GameObject.Find(RootName);
                if (rootObject == null)
                {
                    rootObject = new GameObject(RootName);
                    DontDestroyOnLoad(rootObject);
                }
                PoolRoot = rootObject.transform;
            }
        }

        #region Editor Frendly 
        public IReadOnlyDictionary<GameObject, GameObjectPool> GetGameObjectPools() => _gameObjectPools;
        public IReadOnlyDictionary<Type, Dictionary<GameObject, object>> GetComponentPools() => _componentPools;
        public IReadOnlyDictionary<string, IObjectPool> GetAllPools() => _pools;
        #endregion
        
        #region GameObject Pool Methods

        /// <summary>
        /// 创建或获取GameObject对象池
        /// </summary>
        /// <param name="prefab">预设对象</param>
        /// <param name="initialSize">初始大小</param>
        /// <param name="maxSize">最大大小</param>
        /// <param name="poolName">池名称</param>
        /// <returns>GameObject对象池</returns>
        public GameObjectPool CreateGameObjectPool(GameObject prefab, int initialSize = 10, int maxSize = 100, string poolName = null)
        {
            if (prefab == null)
            {
                Debug.LogError("[ObjectPoolManager] Cannot create pool for null prefab.");
                return null;
            }

            InitializePoolRoot();

            var poolKey = poolName ?? $"Pool_{prefab.name}_{prefab.GetInstanceID()}";
            
            if (_pools.ContainsKey(poolKey))
            {
                return _pools[poolKey] as GameObjectPool;
            }

            var pool = new GameObjectPool(prefab, PoolRoot, poolKey)
            {
                InitialSize = initialSize,
                MaxPoolSize = maxSize
            };

            pool.Initialize();
            
            _pools[poolKey] = pool;
            _gameObjectPools[prefab] = pool;

            return pool;
        }

        /// <summary>
        /// 获取GameObject对象池
        /// </summary>
        /// <param name="prefab">预设对象</param>
        /// <returns>GameObject对象池</returns>
        public GameObjectPool GetGameObjectPool(GameObject prefab)
        {
            if (prefab == null) return null;
            
            _gameObjectPools.TryGetValue(prefab, out var pool);
            return pool;
        }

        /// <summary>
        /// 从池中获取GameObject
        /// </summary>
        /// <param name="prefab">预设对象</param>
        /// <param name="parent">父对象</param>
        /// <returns>获取的GameObject</returns>
        public GameObject GetGameObject(GameObject prefab, Transform parent = null)
        {
            var pool = GetGameObjectPool(prefab);
            if (pool == null)
            {
                pool = CreateGameObjectPool(prefab);
            }
            
            return pool?.Get(parent);
        }

        /// <summary>
        /// 归还GameObject到池中
        /// </summary>
        /// <param name="obj">要归还的GameObject</param>
        public void ReturnGameObject(GameObject obj)
        {
            if (!obj) return;

            // 尝试找到对应的对象池
            foreach (var pool in _gameObjectPools.Values)
            {
                if (pool.ActiveCount > 0 && pool.Prefab != null)
                {
                    // 检查对象名称是否匹配池的预设名称模式
                    if (obj.name.Contains(pool.Prefab.name))
                    {
                        pool.Return(obj);
                        return;
                    }
                }
            }

            // 如果没找到对应的池，直接销毁对象
            Debug.LogWarning($"[ObjectPoolManager] No pool found for GameObject '{obj.name}'. Destroying instead.");
            if (Application.isPlaying)
            {
                Destroy(obj);
            }
            else
            {
                DestroyImmediate(obj);
            }
        }

        #endregion

        #region Component Pool Methods

        /// <summary>
        /// 创建或获取Component对象池
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <param name="prefab">预设对象</param>
        /// <param name="initialSize">初始大小</param>
        /// <param name="maxSize">最大大小</param>
        /// <param name="poolName">池名称</param>
        /// <returns>Component对象池</returns>
        public ComponentPool<T> CreateComponentPool<T>(GameObject prefab, int initialSize = 10, int maxSize = 100, string poolName = null) where T : Component
        {
            if (prefab == null)
            {
                Debug.LogError($"[ObjectPoolManager] Cannot create pool for null prefab.");
                return null;
            }

            InitializePoolRoot();

            var poolKey = poolName ?? $"Pool_{typeof(T).Name}_{prefab.name}";
            var componentType = typeof(T);

            if (!_componentPools.ContainsKey(componentType))
            {
                _componentPools[componentType] = new Dictionary<GameObject, object>();
            }

            if (_componentPools[componentType].ContainsKey(prefab))
            {
                return _componentPools[componentType][prefab] as ComponentPool<T>;
            }

            var pool = new ComponentPool<T>(prefab, PoolRoot, poolKey)
            {
                InitialSize = initialSize,
                MaxPoolSize = maxSize
            };

            pool.Initialize();
            
            _pools[poolKey] = pool;
            _componentPools[componentType][prefab] = pool;

            return pool;
        }

        /// <summary>
        /// 获取Component对象池
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <param name="prefab">预设对象</param>
        /// <returns>Component对象池</returns>
        public ComponentPool<T> GetComponentPool<T>(GameObject prefab) where T : Component
        {
            if (prefab == null) return null;
            
            var componentType = typeof(T);
            if (_componentPools.TryGetValue(componentType, out var pools) &&
                pools.TryGetValue(prefab, out var pool))
            {
                return pool as ComponentPool<T>;
            }
            
            return null;
        }

        /// <summary>
        /// 从池中获取Component
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <param name="prefab">预设对象</param>
        /// <param name="parent">父对象</param>
        /// <returns>获取的Component</returns>
        public T GetComponent<T>(GameObject prefab, Transform parent = null) where T : Component
        {
            var pool = GetComponentPool<T>(prefab);
            if (pool == null)
            {
                pool = CreateComponentPool<T>(prefab);
            }
            
            return pool?.Get(parent);
        }

        /// <summary>
        /// 归还Component到池中
        /// </summary>
        /// <param name="component">要归还的Component</param>
        public void ReturnComponent<T>(T component) where T : Component
        {
            if (component == null) return;

            var componentType = typeof(T);
            if (_componentPools.TryGetValue(componentType, out var pools))
            {
                foreach (var poolEntry in pools)
                {
                    var pool = poolEntry.Value as ComponentPool<T>;
                    if (pool != null && pool.ActiveCount > 0)
                    {
                        // 检查组件是否属于这个池
                        if (pool.ActiveCount > 0)
                        {
                            pool.Return(component);
                            return;
                        }
                    }
                }
            }

            // 如果没找到对应的池，直接销毁对象
            Debug.LogWarning($"[ObjectPoolManager] No pool found for Component '{component.GetType().Name}'. Destroying GameObject instead.");
            if (component.gameObject != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(component.gameObject);
                }
                else
                {
                    DestroyImmediate(component.gameObject);
                }
            }
        }

        #endregion

        #region Generic Pool Methods

        /// <summary>
        /// 创建或获取通用对象池
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="createFunc">创建函数</param>
        /// <param name="initialSize">初始大小</param>
        /// <param name="maxSize">最大大小</param>
        /// <param name="poolName">池名称</param>
        /// <returns>对象池</returns>
        public GenericObjectPool<T> CreatePool<T>(Func<T> createFunc, int initialSize = 10, int maxSize = 100, string poolName = null) where T : class
        {
            if (createFunc == null)
            {
                Debug.LogError($"[ObjectPoolManager] Cannot create pool with null create function.");
                return null;
            }

            var poolKey = poolName ?? $"Pool_{typeof(T).Name}";
            
            if (_pools.ContainsKey(poolKey))
            {
                return _pools[poolKey] as GenericObjectPool<T>;
            }

            var pool = new GenericObjectPool<T>(poolKey)
            {
                CreateFunc = createFunc,
                InitialSize = initialSize,
                MaxPoolSize = maxSize
            };

            pool.Initialize();
            _pools[poolKey] = pool;

            return pool;
        }

        /// <summary>
        /// 获取通用对象池
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="poolName">池名称</param>
        /// <returns>对象池</returns>
        public GenericObjectPool<T> GetPool<T>(string poolName) where T : class
        {
            _pools.TryGetValue(poolName, out var pool);
            return pool as GenericObjectPool<T>;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// 获取所有池的统计信息
        /// </summary>
        /// <returns>统计信息字符串</returns>
        public string GetAllPoolStats()
        {
            var stats = $"ObjectPoolManager Stats - Total Pools: {_pools.Count}";
            stats += "=".PadRight(50, '=') + " ";
            foreach (IObjectPool pool in _pools.Values)
            {
                if (pool is ObjectPoolBase<object> basePool)
                {
                    stats += basePool.GetStats() + " ";
                }
                else
                {
                    stats += $"Pool: {pool.PoolName} ";
                    stats += $"Type: Unknown ";
                    stats += $"Total: {pool.TotalCount} ";
                    stats += $"Active: {pool.ActiveCount} ";
                    stats += $"Inactive: {pool.InactiveCount} ";
                }
                stats += "-".PadRight(50, '-') + " ";
            }
            
            return stats;
        }

        /// <summary>
        /// 清空所有对象池
        /// </summary>
        public void ClearAllPools()
        {
            foreach (var pool in _pools.Values)
            {
                pool.Clear();
            }
            
            _pools.Clear();
            _gameObjectPools.Clear();
            _componentPools.Clear();
        }

        /// <summary>
        /// 归还所有活跃对象
        /// </summary>
        public void ReturnAllToPools()
        {
            foreach (var pool in _gameObjectPools.Values)
            {
                pool.ReturnAll();
            }

            foreach (var componentPoolDict in _componentPools.Values)
            {
                foreach (var pool in componentPoolDict.Values)
                {
                    if (pool is ComponentPool<Component> componentPool)
                    {
                        componentPool.ReturnAll();
                    }
                }
            }
        }

        #endregion
    }
}