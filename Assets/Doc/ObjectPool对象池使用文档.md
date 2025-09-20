# AsakiFramework 对象池系统使用文档

## 第一部分：快速使用

### 1. 基础概念

对象池（Object Pool）是一种设计模式，用于管理和重用对象实例，避免频繁创建和销毁对象带来的性能开销。

### 2. 快速开始

#### 步骤1：引入命名空间

```csharp
using AsakiFramework.ObjectPool;
```

#### 步骤2：创建对象池

```csharp
// 也可以使用 ObjectPoolConfig 来进行编辑器配置
[Serializable]
public class ObjectPoolConfig
{
    [Header("对象池名称"), SerializeField] private string poolName = "DefaultPool";
    [Header("初始容量"), SerializeField, Min(0)] private int initialCapacity = 10;
    [Header("最大容量"), SerializeField, Min(1)] private int maxCapacity = 100;
    [Header("预设体（仅GameObject和Component池需要）"), SerializeField] private GameObject prefab;
    
    public string PoolName => poolName;
    public int InitialCapacity => initialCapacity;
    public int MaxCapacity => maxCapacity;
    public GameObject Prefab => prefab; 
}

// 创建GameObject对象池
ObjectPool.Create(bulletPrefab, initialSize: 10, maxSize: 50);

// 创建Component对象池  
ObjectPool.Create<Enemy>(enemyPrefab, initialSize: 5, maxSize: 20);
```

#### 步骤3：使用对象池

```csharp
// 从池中获取对象
GameObject bullet = ObjectPool.Get(bulletPrefab, spawnPosition, spawnRotation);

// 使用完成后归还
ObjectPool.Return(bullet);
```

### 3. 核心API速查

| 操作            | API调用                                                |
|---------------|------------------------------------------------------|
| 创建GameObject池 | `ObjectPool.Create(prefab, initialSize, maxSize)`    |
| 创建Component池  | `ObjectPool.Create<T>(prefab, initialSize, maxSize)` |
| 获取对象          | `ObjectPool.Get(prefab, position, rotation)`         |
| 归还对象          | `ObjectPool.Return(obj)`                             |
| 清空所有池         | `ObjectPool.ClearAll()`                              |
| 获取统计信息        | `ObjectPool.GetStats()`                              |

---

## 第二部分：使用例子

### 示例1：子弹对象池系统

```csharp
using AsakiFramework.ObjectPool;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    
    private void Start()
    {
        // 创建子弹对象池，预创建20个，最大50个
        ObjectPool.Create(bulletPrefab, 20, 50, "BulletPool");
    }
    
    public void FireBullet()
    {
        // 从池中获取子弹
        GameObject bullet = ObjectPool.Get(bulletPrefab, firePoint.position, firePoint.rotation);
        
        // 设置子弹速度
        bullet.GetComponent<Rigidbody>().velocity = firePoint.forward * 20f;
        
        // 2秒后自动归还
        StartCoroutine(ReturnBulletAfterDelay(bullet, 2f));
    }
    
    private System.Collections.IEnumerator ReturnBulletAfterDelay(GameObject bullet, float delay)
    {
        yield return new WaitForSeconds(delay);
        ObjectPool.Return(bullet);
    }
}
```

### 示例2：敌人对象池系统

```csharp
using AsakiFramework.ObjectPool;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnInterval = 2f;
    
    private void Start()
    {
        // 创建敌人类型的组件池
        ObjectPool.Create<Enemy>(enemyPrefab, 5, 15, "EnemyPool");
        
        // 开始定时生成敌人
        InvokeRepeating(nameof(SpawnEnemy), 0f, spawnInterval);
    }
    
    private void SpawnEnemy()
    {
        Vector3 randomPosition = GetRandomSpawnPosition();
        
        // 从池中获取敌人组件
        Enemy enemy = ObjectPool.Get<Enemy>(enemyPrefab, randomPosition, Quaternion.identity);
        
        // 初始化敌人
        enemy.Initialize(health: Random.Range(50f, 100f));
    }
    
    private Vector3 GetRandomSpawnPosition()
    {
        return new Vector3(
            Random.Range(-10f, 10f),
            0,
            Random.Range(-10f, 10f)
        );
    }
}
```

### 示例3：可池化对象接口

```csharp
using AsakiFramework.ObjectPool;
using UnityEngine;

public class Bullet : MonoBehaviour, IPoolable
{
    private Rigidbody rb;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    
    // 当从池中获取时调用
    public void OnGetFromPool()
    {
        gameObject.SetActive(true);
        rb.velocity = Vector3.zero;
    }
    
    // 当归还到池时调用
    public void OnReturnToPool()
    {
        gameObject.SetActive(false);
        rb.velocity = Vector3.zero;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
    }
    
    // 当从池中销毁时调用
    public void OnDestroyFromPool()
    {
        Debug.Log($"Bullet {name} destroyed from pool");
    }
    
    // 使用示例：碰撞后自动归还
    private void OnCollisionEnter(Collision collision)
    {
        // 碰撞后延迟归还
        Invoke(nameof(ReturnToPool), 0.1f);
    }
    
    private void ReturnToPool()
    {
        ObjectPool.Return(gameObject);
    }
}
```

### 示例4：高级用法 - 自定义创建逻辑

```csharp
using AsakiFramework.ObjectPool;
using System;
using UnityEngine;

public class AdvancedPoolExample : MonoBehaviour
{
    private void Start()
    {
        // 创建带有自定义逻辑的通用对象池
        var customPool = ObjectPool.Create(() => 
        {
            var obj = new CustomData();
            obj.Initialize();
            return obj;
        }, 10, 20, "CustomDataPool");
        
        // 设置回调函数
        customPool.OnGetAction = obj => 
        {
            Debug.Log($"Got object: {obj.Id}");
            obj.Reset();
        };
        
        customPool.OnReturnAction = obj => 
        {
            Debug.Log($"Returned object: {obj.Id}");
            obj.Cleanup();
        };
    }
}

public class CustomData
{
    public int Id { get; private set; }
    public string Data { get; set; }
    
    public void Initialize()
    {
        Id = UnityEngine.Random.Range(1, 1000);
        Data = $"Data_{Id}";
    }
    
    public void Reset()
    {
        Data = $"Reset_{Id}";
    }
    
    public void Cleanup()
    {
        Data = string.Empty;
    }
}
```

---

## 第三部分：设计理念与实现方式介绍

### 1. 设计原则

#### 1.1 性能优先

- **对象复用**：避免频繁的GC分配和销毁
- **高效存储**：使用`Stack<T>`存储空闲对象，O(1)时间复杂度
- **最小化开销**：精简的接口设计，减少不必要的功能调用

#### 1.2 类型安全

- **泛型支持**：完整的泛型实现，编译时类型检查
- **专用实现**：针对GameObject和Component提供专门的对象池类
- **接口隔离**：清晰的接口分层，避免类型混淆

#### 1.3 框架集成

- **单例模式**：遵循AsakiFramework的Singleton模式
- **生命周期管理**：与Unity的生命周期完美集成
- **日志系统**：集成框架的日志系统，便于调试

### 2. 架构设计

```
IObjectPool (接口)
    ↑
IObjectPool<T> (泛型接口)
    ↑
ObjectPoolBase<T> (抽象基类)
    ↑
├── GenericObjectPool<T> (通用对象池)
├── GameObjectPool (GameObject专用池)
└── ComponentPool<T> (Component专用池)
```

### 3. 核心组件详解

#### 3.1 ObjectPoolBase<T>

- **职责**：提供通用的对象池功能
- **特性**：
    - 自动扩展和大小限制
    - 对象生命周期管理
    - 统计信息收集
    - 线程安全的对象管理

#### 3.2 GameObjectPool

- **职责**：专门处理GameObject的池化
- **特性**：
    - 预设实例化
    - Transform管理
    - 活跃状态控制
    - IPoolable接口支持

#### 3.3 ComponentPool<T>

- **职责**：专门处理Component的池化
- **特性**：
    - 自动组件获取
    - GameObject生命周期管理
    - 类型安全的组件操作

#### 3.4 ObjectPoolManager

- **职责**：统一管理所有对象池
- **特性**：
    - 单例模式
    - 自动池创建和管理
    - 全局统计信息
    - 批量操作支持

### 4. 实现细节

#### 4.1 对象存储结构

```csharp
protected readonly Stack<T> _inactiveObjects;  // 空闲对象栈
protected readonly HashSet<T> _activeObjects;  // 活跃对象集合
```

#### 4.2 对象生命周期

```csharp
// 获取对象时
public T Get()
{
    T obj = _inactiveObjects.Count > 0 ? _inactiveObjects.Pop() : CreateNewObject();
    _activeObjects.Add(obj);
    OnGetAction?.Invoke(obj);
    return obj;
}

// 归还对象时
public void Return(T obj)
{
    _activeObjects.Remove(obj);
    if (_inactiveObjects.Count >= MaxPoolSize)
        DestroyObject(obj);
    else
        _inactiveObjects.Push(obj);
    OnReturnAction?.Invoke(obj);
}
```

#### 4.3 性能优化策略

- **预创建**：初始化时预先创建对象，避免运行时分配
- **懒加载**：对象不足时才创建新对象
- **大小限制**：防止内存无限增长
- **快速查找**：使用HashSet管理活跃对象，O(1)查找性能

### 5. 使用建议

#### 5.1 适用场景

- 频繁创建销毁的对象（子弹、粒子效果）
- 大量相似对象（敌人、道具）
- 需要精确控制生命周期的对象
- 性能敏感的游戏系统

#### 5.2 最佳实践

- **合理设置池大小**：根据游戏需求调整初始大小和最大大小
- **及时归还对象**：避免内存泄漏，用完即还
- **使用IPoolable接口**：实现生命周期回调，正确重置对象状态
- **批量操作**：使用批量获取/归还功能提高效率

#### 5.3 注意事项

- 对象池不适合所有场景，简单对象可能不需要池化
- 注意线程安全问题，Unity对象必须在主线程操作
- 定期监控池的统计信息，优化参数设置
- 场景切换时记得清理对象池

### 6. 扩展性

对象池系统设计具有良好的扩展性：

- 可以轻松添加新的专用池类型
- 支持自定义对象创建和销毁逻辑
- 可扩展的统计和监控功能
- 与Unity的新系统（ECS、DOTS）兼容

---

## 总结

AsakiFramework对象池系统提供了一个完整、高效、易用的对象管理解决方案。通过合理的使用，可以显著提升游戏性能，减少GC压力，是Unity游戏开发中不可或缺的工具。

系统设计遵循了软件工程的最佳实践，具有良好的可维护性和扩展性，适合各种规模的项目使用。