using System;
using UnityEngine;

namespace AsakiFramework.ObjectPool
{
    /// <summary>
    /// GameObject对象池
    /// 专门用于管理GameObject实例的对象池
    /// </summary>
    public class GameObjectPool : ObjectPoolBase<GameObject>
    {
        /// <summary>
        /// 预设引用
        /// </summary>
        public GameObject Prefab { get; private set; }

        /// <summary>
        /// 对象池的根Transform
        /// </summary>
        public Transform PoolRoot { get; private set; }

        /// <summary>
        /// 是否将池中的对象设置为DontDestroyOnLoad
        /// </summary>
        public bool DontDestroyOnLoad { get; set; } = false;

        /// <summary>
        /// 是否自动设置对象的活跃状态
        /// </summary>
        public bool AutoSetActive { get; set; } = true;

        private readonly string _defaultPoolName;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="prefab">预设对象</param>
        /// <param name="poolRoot">对象池根节点</param>
        /// <param name="poolName">对象池名称</param>
        public GameObjectPool(GameObject prefab, Transform poolRoot = null, string poolName = null) 
            : base(poolName ?? $"GameObjectPool_{prefab?.name ?? "Unknown"}")
        {
            Prefab = prefab ?? throw new ArgumentNullException(nameof(prefab));
            PoolRoot = poolRoot;
            _defaultPoolName = $"Pool_{Prefab.name}";

            // 设置创建函数
            CreateFunc = CreateGameObject;

            // 设置回调函数
            OnGetAction = OnGetGameObject;
            OnReturnAction = OnReturnGameObject;
            // OnDestroyAction 不需要设置，使用基类的DestroyObject方法
        }

        /// <summary>
        /// 创建GameObject实例
        /// </summary>
        /// <returns>创建的GameObject</returns>
        private GameObject CreateGameObject()
        {
            GameObject obj;
            
            if (Prefab != null)
            {
                obj = UnityEngine.Object.Instantiate(Prefab);
            }
            else
            {
                obj = new GameObject($"PooledObject_{PoolName}");
            }

            // 设置对象名称
            obj.name = $"{Prefab.name}_Pooled";

            // 设置父对象
            if (PoolRoot != null)
            {
                obj.transform.SetParent(PoolRoot, false);
            }

            // 设置为DontDestroyOnLoad（如果需要）
            if (DontDestroyOnLoad)
            {
                UnityEngine.Object.DontDestroyOnLoad(obj);
            }

            // 初始设置为非活跃
            if (AutoSetActive)
            {
                obj.SetActive(false);
            }

            return obj;
        }

        /// <summary>
        /// 获取对象时的回调
        /// </summary>
        /// <param name="obj">获取的对象</param>
        private void OnGetGameObject(GameObject obj)
        {
            if (obj == null) return;

            // 设置为活跃
            if (AutoSetActive)
            {
                obj.SetActive(true);
            }

            // 调用IPoolable接口（如果实现了）
            if (obj.TryGetComponent<IPoolable>(out var poolable))
            {
                poolable.OnGetFromPool();
            }
        }

        /// <summary>
        /// 归还对象时的回调
        /// </summary>
        /// <param name="obj">归还的对象</param>
        private void OnReturnGameObject(GameObject obj)
        {
            if (obj == null) return;

            // 设置为非活跃
            if (AutoSetActive)
            {
                obj.SetActive(false);
            }

            // 重置Transform
            if (PoolRoot != null)
            {
                obj.transform.SetParent(PoolRoot, false);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;
                obj.transform.localScale = Vector3.one;
            }

            // 调用IPoolable接口（如果实现了）
            if (obj.TryGetComponent<IPoolable>(out var poolable))
            {
                poolable.OnReturnToPool();
            }
        }

        /// <summary>
        /// 销毁对象时的回调
        /// </summary>
        /// <param name="obj">要销毁的对象</param>
        protected override void DestroyObject(GameObject obj)
        {
            if (obj == null) return;

            // 调用IPoolable接口（如果实现了）
            if (obj.TryGetComponent<IPoolable>(out var poolable))
            {
                poolable.OnDestroyFromPool();
            }

            // 销毁GameObject
            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(obj);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(obj);
            }
        }

        /// <summary>
        /// 获取对象并设置父对象
        /// </summary>
        /// <param name="parent">父对象</param>
        /// <returns>获取的对象</returns>
        public GameObject Get(Transform parent = null)
        {
            var obj = base.Get();
            
            if (obj != null && parent != null)
            {
                obj.transform.SetParent(parent, false);
            }
            
            return obj;
        }

        /// <summary>
        /// 获取对象并设置位置和旋转
        /// </summary>
        /// <param name="position">位置</param>
        /// <param name="rotation">旋转</param>
        /// <param name="parent">父对象</param>
        /// <returns>获取的对象</returns>
        public GameObject Get(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            var obj = Get(parent);
            
            if (obj != null)
            {
                obj.transform.position = position;
                obj.transform.rotation = rotation;
            }
            
            return obj;
        }

        /// <summary>
        /// 获取对象并设置位置、旋转和缩放
        /// </summary>
        /// <param name="position">位置</param>
        /// <param name="rotation">旋转</param>
        /// <param name="scale">缩放</param>
        /// <param name="parent">父对象</param>
        /// <returns>获取的对象</returns>
        public GameObject Get(Vector3 position, Quaternion rotation, Vector3 scale, Transform parent = null)
        {
            var obj = Get(position, rotation, parent);
            
            if (obj != null)
            {
                obj.transform.localScale = scale;
            }
            
            return obj;
        }

        /// <summary>
        /// 批量获取对象
        /// </summary>
        /// <param name="count">要获取的对象数量</param>
        /// <param name="parent">父对象</param>
        /// <returns>获取的对象列表</returns>
        public System.Collections.Generic.List<GameObject> GetBatch(int count, Transform parent = null)
        {
            var objects = new System.Collections.Generic.List<GameObject>(count);
            
            for (int i = 0; i < count; i++)
            {
                var obj = Get(parent);
                if (obj != null)
                {
                    objects.Add(obj);
                }
            }
            
            return objects;
        }

        /// <summary>
        /// 归还所有活跃对象
        /// </summary>
        public void ReturnAll()
        {
            var activeObjects = new System.Collections.Generic.List<GameObject>(_activeObjects);
            foreach (var obj in activeObjects)
            {
                Return(obj);
            }
        }

        /// <summary>
        /// 销毁所有活跃对象（强制销毁，不归还到池）
        /// </summary>
        public void DestroyAllActive()
        {
            var activeObjects = new System.Collections.Generic.List<GameObject>(_activeObjects);
            foreach (var obj in activeObjects)
            {
                if (obj != null)
                {
                    _activeObjects.Remove(obj);
                    DestroyObject(obj);
                }
            }
        }
    }
}