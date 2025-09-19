using AsakiFramework.ObjectPool;
using UnityEngine;

namespace AsakiFramework.Tests
{
    /// <summary>
    /// 对象池编译测试
    /// 用于验证对象池系统没有编译错误
    /// </summary>
    public class ObjectPoolCompilationTest : MonoBehaviour
    {
        [SerializeField] private GameObject testPrefab;

        private void Start()
        {
            TestCompilation();
        }

        [ContextMenu("测试编译")]
        private void TestCompilation()
        {
            Debug.Log("=== 对象池编译测试开始 ===");

            try
            {
                // 测试GameObject池创建
                if (testPrefab != null)
                {
                    var gameObjectPool = ObjectPool.Create(testPrefab, 5, 10);
                    Debug.Log($"✓ GameObject池创建成功: {gameObjectPool.PoolName}");

                    // 测试获取和归还
                    var obj = ObjectPool.Get(testPrefab);
                    Debug.Log($"✓ GameObject获取成功: {obj.name}");
                    ObjectPool.Return(obj);
                    Debug.Log("✓ GameObject归还成功");
                }
                else
                {
                    Debug.LogWarning("! 测试预设为空，跳过GameObject池测试");
                }

                // 测试组件池创建
                var componentPool = ObjectPool.Create<Transform>(new GameObject("TestComponent"));
                Debug.Log($"✓ Component池创建成功: {componentPool.PoolName}");

                // 测试通用对象池
                var genericPool = ObjectPool.Create(() => new TestObject(), 3, 6);
                Debug.Log($"✓ 通用对象池创建成功: {genericPool.PoolName}");

                // 测试池管理器
                var stats = ObjectPool.GetStats();
                Debug.Log($"✓ 池统计信息获取成功:\n{stats}");

                // 测试清理
                ObjectPool.ReturnAll();
                Debug.Log("✓ 归还所有对象成功");

                Debug.Log("=== 对象池编译测试完成 - 所有测试通过！ ===");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"✗ 测试失败: {e.Message}\n{e.StackTrace}");
            }
        }

        private void OnDestroy()
        {
            // 清理
            if (Application.isPlaying)
            {
                ObjectPool.ClearAll();
                Debug.Log("测试清理完成");
            }
        }

        // 测试用简单类
        private class TestObject
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public TestObject()
            {
                Id = UnityEngine.Random.Range(1, 1000);
                Name = $"TestObject_{Id}";
            }
        }
    }
}