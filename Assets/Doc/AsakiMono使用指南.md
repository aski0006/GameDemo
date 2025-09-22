# AsakiMono 使用说明

## 概述

`AsakiMono` 是 AsakiFramework 的核心基类，继承自 Unity 的 `MonoBehaviour`，为所有框架组件提供统一的日志系统、组件管理、协程工具和线程安全功能。

## 基础特性

### 1. 日志系统

#### 日志级别
- **Info**: 信息日志，仅在 `isVerbose = true` 时显示
- **Warning**: 警告日志，仅在 `isVerbose = true` 时显示
- **Error**: 错误日志，总是显示，不受 `isVerbose` 限制
- **Assert**: 断言日志，条件失败时显示错误

#### 使用示例
```csharp
public class MyComponent : AsakiMono
{
    void Start()
    {
        LogInfo("组件启动");
        LogWarning("这是一个警告");
        LogError("发生错误");
        LogAssert(myVariable != null, "myVariable 不能为空");
    }
}
```

#### 重要特性
- **发布版自动剔除**: 所有日志方法在发布版中会被 JIT 内联为空方法，零性能开销
- **上下文支持**: 可传入 `UnityEngine.Object` 上下文，在编辑器中点击日志可定位到对应对象

### 2. 组件管理

#### 获取或添加组件
```csharp
// 获取或添加自身组件
var renderer = GetOrAddComponent<MeshRenderer>();

// 在父级链中查找
var parentRigidbody = GetOrAddComponent<Rigidbody>(FindComponentMode.Parent);

// 在子级链中查找
var childCollider = GetOrAddComponent<Collider>(FindComponentMode.Children);

// 在整个场景中查找
var sceneLight = GetOrAddComponent<Light>(FindComponentMode.Scene);
```

#### 必需组件检查
```csharp
// 检查并确保组件存在（编辑器下会抛出异常）
private void ValidateComponents()
{
    HasNotNullComponent<Rigidbody>();
    HasNotNullComponent<Collider>(myCollider);
}
```

#### 缓存组件获取
```csharp
private MeshRenderer _cachedRenderer;

private void UpdateRenderer()
{
    // 第一次调用时缓存，后续直接使用缓存
    var renderer = GetCachedComponent(ref _cachedRenderer);
    renderer.material.color = Color.red;
}
```

## 协程工具

### 延迟执行
```csharp
// 延迟 2 秒后执行
DelayTime(2f, () => {
    LogInfo("2 秒延迟执行");
});

// 下一帧执行
RunNextFrame(() => {
    LogInfo("下一帧执行");
});

// 延迟 5 帧后执行
DelayFrames(5, () => {
    LogInfo("延迟 5 帧执行");
});
```

### 条件等待
```csharp
// 等待条件满足
DelayUntil(() => player != null, () => {
    LogInfo("玩家已创建");
});

// 等待条件变为 false
DelayWhile(() => isLoading, () => {
    LogInfo("加载完成");
});
```

### 循环执行
```csharp
// 循环 3 次，每次间隔 1 秒
Loop(3, 1f, (index) => {
    LogInfo($"第 {index + 1} 次循环");
});

// 无限循环（需要手动停止）
var loopCoroutine = LoopForever(0.5f, () => {
    LogInfo("持续执行");
});

// 停止循环
StopCoroutine(loopCoroutine);
```

### 持续执行
```csharp
// 持续执行 5 秒，每帧调用
ExecuteForDuration(5f, (elapsed) => {
    transform.position = Vector3.Lerp(startPos, endPos, elapsed / 5f);
});
```

## 线程安全

### 锁机制
```csharp
// 使用静态锁（适合保护全局单例）
LockStatic(() => {
    // 线程安全的操作
    GlobalData.Instance.counter++;
});

// 使用当前实例作为锁对象
Lock(() => {
    // 保护当前组件状态
    _localData++;
});

// 使用指定锁对象
Lock(myLockObject, () => {
    // 自定义锁保护
    SharedResource.Update();
});
```

## 分帧创建

### 大数据集处理
当需要创建大量对象时，使用分帧创建避免卡顿：

```csharp
public class CardSpawner : AsakiMono
{
    [System.Serializable]
    public class CardCreationHandler : IFrameCreationHandler<CardData, CardViewer>
    {
        public CardViewer Create(CardData data)
        {
            var card = ObjectPool.Get(cardPrefab);
            var viewer = card.GetComponent<CardViewer>();
            viewer.Setup(data);
            return viewer;
        }

        public void OnError(CardData data, Exception e)
        {
            Debug.LogError($"创建卡牌失败: {data.name}, 错误: {e.Message}");
        }
    }

    private void SpawnCards(List<CardData> cardDatas)
    {
        var handler = new CardCreationHandler();
        
        CreateOverFrames(
            cardDatas,
            handler,
            perFrame: 2,                    // 每帧创建 2 个
            maxMillisPerFrame: 16f,         // 单帧最大耗时 16ms
            onProgress: (current, total) => {
                LogInfo($"创建进度: {current}/{total}");
            },
            onComplete: (results) => {
                LogInfo($"所有卡牌创建完成，共 {results.Count} 个");
            }
        );
    }
}
```

## 最佳实践

### 1. 继承使用
所有游戏逻辑组件都应该继承 `AsakiMono` 而不是直接继承 `MonoBehaviour`：
```csharp
// ✅ 正确
public class PlayerController : AsakiMono
{
    // 可以使用所有 AsakiMono 功能
}

// ❌ 错误
public class PlayerController : MonoBehaviour
{
    // 无法使用 AsakiMono 提供的功能
}
```

### 2. 日志使用
- **开发阶段**: 启用 `isVerbose` 获得详细日志
- **调试阶段**: 使用 `LogAssert` 进行关键条件检查
- **发布版本**: 日志会自动剔除，无需手动清理

### 3. 组件缓存
对于频繁访问的组件，使用 `GetCachedComponent` 进行缓存：
```csharp
private Rigidbody _rigidbody;
private Animator _animator;

private void Update()
{
    // 第一次调用时缓存
    var rb = GetCachedComponent(ref _rigidbody);
    var anim = GetCachedComponent(ref _animator);
    
    // 后续直接使用缓存
    rb.AddForce(movement);
    anim.SetFloat("Speed", rb.velocity.magnitude);
}
```

### 4. 协程管理
- 使用框架提供的协程方法，避免手动编写协程逻辑
- 需要时保存协程引用以便停止
- 注意循环协程的停止时机

### 5. 线程安全
- 使用适当的锁机制保护共享资源
- 优先考虑使用 `LockStatic` 保护全局状态
- 避免在锁内进行耗时操作

## 性能优化

1. **零开销日志**: 发布版中日志方法会被 JIT 内联剔除，完全无性能影响
2. **组件缓存**: `GetCachedComponent` 避免重复 `GetComponent` 调用
3. **分帧处理**: 大数据集操作使用分帧创建避免卡顿
4. **线程安全**: 锁机制经过优化，减少线程竞争

## 注意事项

1. **编辑器依赖**: 某些功能（如 `HasNotNullComponent` 的异常抛出）仅在编辑器中生效
2. **协程生命周期**: 协程会随着组件的销毁而自动停止，但最好手动管理重要的协程
3. **锁超时**: 默认锁操作没有超时限制，可通过参数设置超时时间
4. **分帧参数**: 合理设置 `perFrame` 和 `maxMillisPerFrame` 参数，平衡性能和响应性

---

*文档版本: v1.0*  
*最后更新: 2025年9月22日*