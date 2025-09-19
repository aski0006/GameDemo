using AsakiFramework.ObjectPool;
using UnityEngine;

namespace AsakiFramework.Tests
{
    /// <summary>
    /// 测试可池化组件
    /// </summary>
    public class TestPoolableComponent : MonoBehaviour, IPoolable
    {
        [SerializeField] private int testId = 0;
        [SerializeField] private bool isPooled = false;

        public int TestId => testId;
        public bool IsPooled => isPooled;

        public void SetTestId(int id)
        {
            testId = id;
        }

        // IPoolable接口实现
        public void OnGetFromPool()
        {
            isPooled = false;
            Debug.Log($"[{nameof(TestPoolableComponent)}] 从池中获取 - ID: {testId}");
        }

        public void OnReturnToPool()
        {
            isPooled = true;
            testId = 0; // 重置状态
            Debug.Log($"[{nameof(TestPoolableComponent)}] 归还到池 - ID: {testId}");
        }

        public void OnDestroyFromPool()
        {
            Debug.Log($"[{nameof(TestPoolableComponent)}] 从池中销毁 - ID: {testId}");
        }
    }

    /// <summary>
    /// 对象池测试类
    /// </summary>
    public class TestObjectPool : MonoBehaviour
    {
        [Header("测试预设")]
        [SerializeField] private GameObject testPrefab;
        [SerializeField] private GameObject testComponentPrefab;

        [Header("测试参数")]
        [SerializeField] private int testPoolSize = 5;
        [SerializeField] private bool autoRunTests = false;

        private void Start()
        {
            if (autoRunTests)
            {
                RunAllTests();
            }
        }

        [ContextMenu("运行所有测试")]
        private void RunAllTests()
        {
            Debug.Log("=== 开始对象池测试 ===");
            
            TestGameObjectPool();
            TestComponentPool();
            TestStaticAPI();
            TestPoolableInterface();
            TestPoolManager();
            
            Debug.Log("=== 对象池测试完成 ===");
            Debug.Log(ObjectPool.GetStats());
        }

        [ContextMenu("测试GameObject池")]
        private void TestGameObjectPool()
        {
            Debug.Log("--- 测试GameObject池 ---");
            
            if (testPrefab == null)
            {
                Debug.LogError("测试预设为空！");
                return;
            }

            // 创建对象池
            var pool = ObjectPool.Create(testPrefab, testPoolSize, testPoolSize * 2);
            Debug.Log($"创建GameObject池: {pool.PoolName}");

            // 获取对象
            var obj1 = pool.Get();
            var obj2 = pool.Get();
            Debug.Log($"获取对象: {obj1.name}, {obj2.name}");

            // 设置父对象
            var parent = new GameObject("TestParent");
            var obj3 = pool.Get(parent.transform);
            Debug.Log($"获取对象并设置父对象: {obj3.name}, 父对象: {obj3.transform.parent.name}");

            // 设置位置和旋转
            var obj4 = pool.Get(Vector3.one, Quaternion.Euler(45, 45, 45));
            Debug.Log($"获取对象并设置位置: {obj4.name}, 位置: {obj4.transform.position}");

            // 批量获取
            var batchObjects = pool.GetBatch(3);
            Debug.Log($"批量获取 {batchObjects.Count} 个对象");

            // 归还对象
            pool.Return(obj1);
            pool.Return(obj2);
            Debug.Log("归还对象到池");

            // 归还所有对象
            pool.ReturnAll();
            Debug.Log("归还所有活跃对象");

            // 清理
            DestroyImmediate(parent);
            Debug.Log("GameObject池测试完成");
        }

        [ContextMenu("测试Component池")]
        private void TestComponentPool()
        {
            Debug.Log("--- 测试Component池 ---");
            
            if (testComponentPrefab == null)
            {
                Debug.LogError("测试组件预设为空！");
                return;
            }

            // 创建组件对象池
            var pool = ObjectPool.Create<TestPoolableComponent>(testComponentPrefab, testPoolSize, testPoolSize * 2);
            Debug.Log($"创建Component池: {pool.PoolName}");

            // 获取组件
            var comp1 = pool.Get();
            var comp2 = pool.Get();
            comp1.SetTestId(1);
            comp2.SetTestId(2);
            Debug.Log($"获取组件: {comp1.name} (ID: {comp1.TestId}), {comp2.name} (ID: {comp2.TestId})");

            // 设置父对象
            var parent = new GameObject("TestComponentParent");
            var comp3 = pool.Get(parent.transform);
            comp3.SetTestId(3);
            Debug.Log($"获取组件并设置父对象: {comp3.name} (ID: {comp3.TestId})");

            // 设置位置和旋转
            var comp4 = pool.Get(Vector3.one * 2, Quaternion.Euler(90, 0, 0));
            comp4.SetTestId(4);
            Debug.Log($"获取组件并设置位置: {comp4.name} (ID: {comp4.TestId}), 位置: {comp4.transform.position}");

            // 批量获取
            var batchComponents = pool.GetBatch(2);
            for (int i = 0; i < batchComponents.Count; i++)
            {
                batchComponents[i].SetTestId(100 + i);
            }
            Debug.Log($"批量获取 {batchComponents.Count} 个组件");

            // 归还组件
            pool.Return(comp1);
            pool.Return(comp2);
            Debug.Log("归还组件到池");

            // 归还所有组件
            pool.ReturnAll();
            Debug.Log("归还所有活跃组件");

            // 清理
            DestroyImmediate(parent);
            Debug.Log("Component池测试完成");
        }

        [ContextMenu("测试静态API")]
        private void TestStaticAPI()
        {
            Debug.Log("--- 测试静态API ---");
            
            if (testPrefab == null)
            {
                Debug.LogError("测试预设为空！");
                return;
            }

            // 使用静态API获取GameObject
            var obj1 = ObjectPool.Get(testPrefab);
            var obj2 = ObjectPool.Get(testPrefab, Vector3.one * 3, Quaternion.identity);
            Debug.Log($"静态API获取GameObject: {obj1.name}, {obj2.name}");

            // 归还
            ObjectPool.Return(obj1);
            ObjectPool.Return(obj2);
            Debug.Log("静态API归还GameObject");

            // 测试组件（如果有测试预设）
            if (testComponentPrefab != null)
            {
                var comp1 = ObjectPool.Get<TestPoolableComponent>(testComponentPrefab);
                var comp2 = ObjectPool.Get<TestPoolableComponent>(testComponentPrefab, Vector3.one * 4, Quaternion.identity);
                Debug.Log($"静态API获取Component: {comp1.name} (ID: {comp1.TestId}), {comp2.name} (ID: {comp2.TestId})");

                // 归还
                ObjectPool.Return(comp1);
                ObjectPool.Return(comp2);
                Debug.Log("静态API归还Component");
            }

            Debug.Log("静态API测试完成");
        }

        [ContextMenu("测试可池化接口")]
        private void TestPoolableInterface()
        {
            Debug.Log("--- 测试可池化接口 ---");
            
            if (testComponentPrefab == null)
            {
                Debug.LogError("测试组件预设为空！");
                return;
            }

            // 创建池
            var pool = ObjectPool.Create<TestPoolableComponent>(testComponentPrefab, 3, 6);
            
            // 获取组件（会触发OnGetFromPool）
            var comp = pool.Get();
            comp.SetTestId(999);
            
            // 归还组件（会触发OnReturnToPool）
            pool.Return(comp);
            
            // 再次获取（会触发OnGetFromPool）
            var comp2 = pool.Get();
            Debug.Log($"重新获取的组件ID: {comp2.TestId} (应该为0，因为已重置)");

            Debug.Log("可池化接口测试完成");
        }

        [ContextMenu("测试池管理器")]
        private void TestPoolManager()
        {
            Debug.Log("--- 测试池管理器 ---");
            
            if (testPrefab == null || testComponentPrefab == null)
            {
                Debug.LogError("测试预设为空！");
                return;
            }

            // 创建多个池
            var pool1 = ObjectPool.Create(testPrefab, 2, 4, "TestPool1");
            var pool2 = ObjectPool.Create<TestPoolableComponent>(testComponentPrefab, 3, 6, "TestPool2");
            
            // 获取一些对象
            var obj1 = pool1.Get();
            var obj2 = pool1.Get();
            var comp1 = pool2.Get();
            var comp2 = pool2.Get();
            var comp3 = pool2.Get();

            Debug.Log($"活跃对象数量 - Pool1: {pool1.ActiveCount}, Pool2: {pool2.ActiveCount}");

            // 显示统计信息
            Debug.Log("池管理器统计信息:");
            Debug.Log(ObjectPool.GetStats());

            // 测试归还所有
            ObjectPool.ReturnAll();
            Debug.Log($"归还所有后活跃对象数量 - Pool1: {pool1.ActiveCount}, Pool2: {pool2.ActiveCount}");

            Debug.Log("池管理器测试完成");
        }

        [ContextMenu("清理测试")]
        private void CleanupTest()
        {
            ObjectPool.ClearAll();
            
            // 清理测试创建的父对象
            var testParents = GameObject.FindObjectsOfType<Transform>();
            foreach (var parent in testParents)
            {
                if (parent.name.Contains("TestParent") || parent.name.Contains("TestComponentParent"))
                {
                    DestroyImmediate(parent.gameObject);
                }
            }
            
            Debug.Log("测试清理完成");
        }

        private void OnDestroy()
        {
            // 场景销毁时清理所有池
            if (Application.isPlaying)
            {
                ObjectPool.ClearAll();
            }
        }
    }
}