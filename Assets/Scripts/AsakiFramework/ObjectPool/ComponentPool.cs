using System;
using UnityEngine;

namespace AsakiFramework.ObjectPool
{
    /// <summary>
    /// Component对象池
    /// 专门用于管理Component实例的对象池
    /// </summary>
    /// <typeparam name="T">组件类型</typeparam>
    public class ComponentPool<T> : ObjectPoolBase<T> where T : Component
    {
        /// <summary>
        /// 预设对象
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
        /// 是否自动设置GameObject的活跃状态
        /// </summary>
        public bool AutoSetActive { get; set; } = true;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="prefab">预设对象</param>
        /// <param name="poolRoot">对象池根节点</param>
        /// <param name="poolName">对象池名称</param>
        public ComponentPool(GameObject prefab, Transform poolRoot = null, string poolName = null)
            : base(poolName ?? $"ComponentPool_{typeof(T).Name}")
        {
            Prefab = prefab ?? throw new ArgumentNullException(nameof(prefab));
            PoolRoot = poolRoot;

            // 设置创建函数
            CreateFunc = CreateComponent;

            // 设置回调函数
            OnGetAction = OnGetComponent;
            OnReturnAction = OnReturnComponent;
            // OnDestroyAction 不需要设置，使用基类的DestroyObject方法
        }

        /// <summary>
        /// 创建Component实例
        /// </summary>
        /// <returns>创建的Component</returns>
        private T CreateComponent()
        {
            GameObject obj;
            
            if (Prefab != null)
            {
                obj = UnityEngine.Object.Instantiate(Prefab);
            }
            else
            {
                obj = new GameObject($"PooledComponent_{typeof(T).Name}");
            }

            // 设置对象名称
            obj.name = $"{Prefab.name}_{typeof(T).Name}_Pooled";

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

            // 获取组件
            var component = obj.GetComponent<T>();
            if (component == null)
            {
                component = obj.AddComponent<T>();
            }

            // 初始设置为非活跃
            if (AutoSetActive)
            {
                obj.SetActive(false);
            }

            return component;
        }

        /// <summary>
        /// 获取对象时的回调
        /// </summary>
        /// <param name="component">获取的组件</param>
        private void OnGetComponent(T component)
        {
            if (component == null) return;

            var gameObject = component.gameObject;

            // 设置为活跃
            if (AutoSetActive)
            {
                gameObject.SetActive(true);
            }

            // 调用IPoolable接口（如果实现了）
            if (component is IPoolable poolable)
            {
                poolable.OnGetFromPool();
            }
        }

        /// <summary>
        /// 归还对象时的回调
        /// </summary>
        /// <param name="component">归还的组件</param>
        private void OnReturnComponent(T component)
        {
            if (component == null) return;

            var gameObject = component.gameObject;

            // 设置为非活跃
            if (AutoSetActive)
            {
                gameObject.SetActive(false);
            }

            // 重置Transform
            if (PoolRoot != null)
            {
                gameObject.transform.SetParent(PoolRoot, false);
                gameObject.transform.localPosition = Vector3.zero;
                gameObject.transform.localRotation = Quaternion.identity;
                gameObject.transform.localScale = Vector3.one;
            }

            // 调用IPoolable接口（如果实现了）
            if (component is IPoolable poolable)
            {
                poolable.OnReturnToPool();
            }
        }

        /// <summary>
        /// 销毁对象时的回调
        /// </summary>
        /// <param name="component">要销毁的组件</param>
        protected override void DestroyObject(T component)
        {
            if (component == null) return;

            var gameObject = component.gameObject;

            // 调用IPoolable接口（如果实现了）
            if (component is IPoolable poolable)
            {
                poolable.OnDestroyFromPool();
            }

            // 销毁GameObject
            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(gameObject);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        /// <summary>
        /// 获取组件并设置父对象
        /// </summary>
        /// <param name="parent">父对象</param>
        /// <returns>获取的组件</returns>
        public T Get(Transform parent = null)
        {
            var component = base.Get();
            
            if (component != null && parent != null)
            {
                component.transform.SetParent(parent, false);
            }
            
            return component;
        }

        /// <summary>
        /// 获取组件并设置位置和旋转
        /// </summary>
        /// <param name="position">位置</param>
        /// <param name="rotation">旋转</param>
        /// <param name="parent">父对象</param>
        /// <returns>获取的组件</returns>
        public T Get(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            var component = Get(parent);
            
            if (component != null)
            {
                component.transform.position = position;
                component.transform.rotation = rotation;
            }
            
            return component;
        }

        /// <summary>
        /// 获取组件并设置位置、旋转和缩放
        /// </summary>
        /// <param name="position">位置</param>
        /// <param name="rotation">旋转</param>
        /// <param name="scale">缩放</param>
        /// <param name="parent">父对象</param>
        /// <returns>获取的组件</returns>
        public T Get(Vector3 position, Quaternion rotation, Vector3 scale, Transform parent = null)
        {
            var component = Get(position, rotation, parent);
            
            if (component != null)
            {
                component.transform.localScale = scale;
            }
            
            return component;
        }

        /// <summary>
        /// 批量获取组件
        /// </summary>
        /// <param name="count">要获取的组件数量</param>
        /// <param name="parent">父对象</param>
        /// <returns>获取的组件列表</returns>
        public System.Collections.Generic.List<T> GetBatch(int count, Transform parent = null)
        {
            var components = new System.Collections.Generic.List<T>(count);
            
            for (int i = 0; i < count; i++)
            {
                var component = Get(parent);
                if (component != null)
                {
                    components.Add(component);
                }
            }
            
            return components;
        }

        /// <summary>
        /// 归还所有活跃组件
        /// </summary>
        public void ReturnAll()
        {
            var activeComponents = new System.Collections.Generic.List<T>(_activeObjects);
            foreach (var component in activeComponents)
            {
                Return(component);
            }
        }

        /// <summary>
        /// 销毁所有活跃组件（强制销毁，不归还到池）
        /// </summary>
        public void DestroyAllActive()
        {
            var activeComponents = new System.Collections.Generic.List<T>(_activeObjects);
            foreach (var component in activeComponents)
            {
                if (component != null)
                {
                    _activeObjects.Remove(component);
                    DestroyObject(component);
                }
            }
        }
    }
}