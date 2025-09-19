# CoroutineUtility 使用文档

## 概述

`CoroutineUtility` 是 AsakiFramework 提供的通用协程工具类，旨在简化 Unity 中的协程操作。它提供了便捷的延迟执行、条件等待、重复执行等功能，无需手动创建 MonoBehaviour 即可使用协程。

## 核心特性

- 🚀 **零依赖调用**：无需继承 MonoBehaviour 即可使用协程
- 🔄 **自动管理**：自动创建和管理协程运行器
- 🛡️ **线程安全**：安全的协程创建和销毁机制
- 📋 **丰富功能**：延迟、条件、重复等多种执行模式
- 💡 **简单易用**：链式调用，直观易懂

---

## 基础使用方法

### 1. 直接调用
在任何类中直接使用静态方法：

```csharp
using AsakiFramework;

// 延迟2秒执行
CoroutineUtility.Delay(2f, () => {
    Debug.Log("2秒后执行！");
});
```

### 2. 在 MonoBehaviour 中使用
继承 `AsakiMono` 的类可以直接使用封装方法：

```csharp
using AsakiFramework;

public class PlayerController : AsakiMono
{
    void Start()
    {
        // 使用基类封装的方法
        Delay(1f, () => {
            Debug.Log("1秒后执行！");
        });
    }
}
```

---

## 详细功能示例

### 🔥 延迟执行（时间）

#### 基本延迟
```csharp
// 延迟指定秒数后执行
CoroutineUtility.Delay(3f, () => {
    Debug.Log("3秒后显示这条消息");
});

// 延迟0.5秒执行攻击动画
CoroutineUtility.Delay(0.5f, () => {
    PlayAttackAnimation();
});
```

#### 实际应用：技能冷却
```csharp
public class SkillSystem
{
    private bool canUseSkill = true;
    private Coroutine cooldownCoroutine;
    
    public void UseSkill()
    {
        if (!canUseSkill) return;
        
        // 执行技能逻辑
        ExecuteSkill();
        
        // 开始冷却
        canUseSkill = false;
        cooldownCoroutine = CoroutineUtility.Delay(5f, () => {
            canUseSkill = true;
            Debug.Log("技能冷却完成！");
        });
    }
    
    public void CancelCooldown()
    {
        if (cooldownCoroutine != null)
        {
            CoroutineUtility.StopCoroutine(cooldownCoroutine);
            canUseSkill = true;
        }
    }
}
```

### 🎯 延迟执行（帧数）

#### 基本帧延迟
```csharp
// 延迟1帧执行（下一帧）
CoroutineUtility.DelayOneFrame(() => {
    Debug.Log("下一帧执行");
});

// 延迟指定帧数
CoroutineUtility.DelayFrames(60, () => {
    Debug.Log("60帧后执行（约1秒）");
});
```

#### 实际应用：分帧加载
```csharp
public class WorldGenerator
{
    public void GenerateWorld()
    {
        StartCoroutine(GenerateWorldCoroutine());
    }
    
    private IEnumerator GenerateWorldCoroutine()
    {
        for (int x = 0; x < 100; x++)
        {
            for (int z = 0; z < 100; z++)
            {
                GenerateChunk(x, z);
                
                // 每生成10个区块，等待一帧，避免卡顿
                if ((x * 100 + z) % 10 == 0)
                {
                    yield return null;
                }
            }
        }
    }
    
    private void GenerateChunk(int x, int z)
    {
        // 生成地图区块逻辑
        Debug.Log($"生成区块 ({x}, {z})");
    }
}
```

### ⏳ 条件等待

#### 基础条件等待
```csharp
// 等待条件满足
CoroutineUtility.DelayUntil(() => player.health <= 0, () => {
    Debug.Log("玩家死亡，游戏结束！");
});

// 等待条件不满足
CoroutineUtility.DelayWhile(() => isLoading, () => {
    Debug.Log("加载完成！");
});
```

#### 实际应用：异步资源加载
```csharp
public class ResourceLoader
{
    private bool isLoading = false;
    private ResourceRequest loadRequest;
    
    public void LoadResourceAsync(string path)
    {
        isLoading = true;
        loadRequest = Resources.LoadAsync<GameObject>(path);
        
        // 等待加载完成
        CoroutineUtility.DelayUntil(() => loadRequest.isDone, () => {
            isLoading = false;
            GameObject prefab = loadRequest.asset as GameObject;
            Debug.Log($"资源加载完成：{prefab.name}");
        });
    }
}
```

#### 实际应用：敌人巡逻AI
```csharp
public class EnemyAI : AsakiMono
{
    private Transform player;
    private bool isPlayerInRange = false;
    
    void Start()
    {
        // 巡逻逻辑
        StartPatrol();
        
        // 等待玩家进入范围
        DelayUntil(() => Vector3.Distance(transform.position, player.position) < 5f, () => {
            isPlayerInRange = true;
            StopPatrol();
            StartChase();
        });
    }
    
    void StartPatrol() { /* 巡逻逻辑 */ }
    void StopPatrol() { /* 停止巡逻 */ }
    void StartChase() { /* 追击逻辑 */ }
}
```

### 🔄 重复执行

#### 基础重复
```csharp
// 重复执行指定次数
CoroutineUtility.Repeat(() => {
    Debug.Log("这是第几次执行");
}, 5, 1f); // 重复5次，每次间隔1秒

// 无限重复（需要手动停止）
Coroutine coroutine = CoroutineUtility.RepeatForever(() => {
    Debug.Log("每秒执行一次");
}, 1f);

// 停止重复
CoroutineUtility.StopCoroutine(coroutine);
```

#### 实际应用：心跳效果
```csharp
public class HeartbeatEffect : AsakiMono
{
    private Coroutine heartbeatCoroutine;
    
    public void StartHeartbeat()
    {
        heartbeatCoroutine = RepeatForever(() => {
            // 缩放效果
            transform.DOScale(1.2f, 0.2f).OnComplete(() => {
                transform.DOScale(1f, 0.2f);
            });
        }, 1f); // 每秒心跳一次
    }
    
    public void StopHeartbeat()
    {
        if (heartbeatCoroutine != null)
        {
            StopCoroutine(heartbeatCoroutine);
            heartbeatCoroutine = null;
        }
    }
}
```

#### 实际应用：倒计时器
```csharp
public class CountdownTimer : AsakiMono
{
    private int timeRemaining = 10;
    
    public void StartCountdown()
    {
        timeRemaining = 10;
        
        Repeat(() => {
            Debug.Log($"倒计时: {timeRemaining}秒");
            timeRemaining--;
            
            if (timeRemaining <= 0)
            {
                Debug.Log("时间到！");
            }
        }, 10, 1f); // 倒计时10秒，每秒更新一次
    }
}
```

### 📈 持续执行

#### 基础持续执行
```csharp
// 持续执行指定时间，每帧调用
CoroutineUtility.ExecuteForDuration((elapsed) => {
    Debug.Log($"已执行时间: {elapsed:F2}秒");
}, 3f); // 持续3秒
```

#### 实际应用：渐进式效果
```csharp
public class FadeEffect : AsakiMono
{
    public SpriteRenderer spriteRenderer;
    
    public void FadeOut(float duration = 2f)
    {
        Color startColor = spriteRenderer.color;
        
        ExecuteForDuration((elapsed) => {
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
        }, duration);
    }
    
    public void FadeIn(float duration = 2f)
    {
        Color startColor = spriteRenderer.color;
        
        ExecuteForDuration((elapsed) => {
            float alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
        }, duration);
    }
}
```

#### 实际应用：平滑移动
```csharp
public class SmoothMovement : AsakiMono
{
    public void MoveToPosition(Transform target, Vector3 endPosition, float duration = 1f)
    {
        Vector3 startPosition = target.position;
        
        ExecuteForDuration((elapsed) => {
            float t = elapsed / duration;
            // 使用平滑插值
            t = Mathf.SmoothStep(0f, 1f, t);
            target.position = Vector3.Lerp(startPosition, endPosition, t);
        }, duration);
    }
}
```

---

## 高级使用技巧

### 🎯 协程组合
```csharp
public class ComplexSequence : AsakiMono
{
    void Start()
    {
        // 创建复杂的执行序列
        Delay(1f, () => {
            Debug.Log("第一步：等待1秒");
            
            Delay(2f, () => {
                Debug.Log("第二步：再等待2秒");
                
                Repeat(() => {
                    Debug.Log("第三步：重复3次");
                }, 3, 0.5f);
            });
        });
    }
}
```

### 🔄 协程取消
```csharp
public class CancellableTask : AsakiMono
{
    private Coroutine currentTask;
    
    public void StartLongTask()
    {
        // 取消之前的任务
        if (currentTask != null)
        {
            StopCoroutine(currentTask);
        }
        
        currentTask = Delay(5f, () => {
            Debug.Log("长时间任务完成！");
            currentTask = null;
        });
    }
    
    public void CancelTask()
    {
        if (currentTask != null)
        {
            StopCoroutine(currentTask);
            currentTask = null;
            Debug.Log("任务已取消");
        }
    }
}
```

### 🎮 游戏状态管理
```csharp
public class GameStateManager : AsakiMono
{
    private GameState currentState = GameState.Menu;
    
    public void StartGame()
    {
        currentState = GameState.Playing;
        
        // 游戏开始倒计时
        Repeat(() => {
            Debug.Log("游戏开始倒计时...");
        }, 3, 1f);
        
        // 等待倒计时结束后开始游戏逻辑
        Delay(3f, () => {
            Debug.Log("游戏开始！");
            StartGameLoop();
        });
    }
    
    private void StartGameLoop()
    {
        RepeatForever(() => {
            if (currentState == GameState.Playing)
            {
                UpdateGame();
            }
        }, 0.1f); // 每0.1秒更新一次
    }
    
    private void UpdateGame()
    {
        // 游戏逻辑更新
        Debug.Log("更新游戏状态");
    }
    
    private enum GameState
    {
        Menu,
        Playing,
        Paused,
        GameOver
    }
}
```

---

## 性能优化建议

### 1. 合理选择延迟方式
- **时间延迟** (`Delay`)：适用于需要精确时间控制的场景
- **帧延迟** (`DelayFrames`)：适用于需要同步渲染帧的场景
- **条件等待** (`DelayUntil/DelayWhile`)：适用于状态依赖的操作

### 2. 协程生命周期管理
```csharp
public class OptimizedManager : AsakiMono
{
    private List<Coroutine> activeCoroutines = new List<Coroutine>();
    
    protected override void OnDestroy()
    {
        // 清理所有活动的协程
        foreach (var coroutine in activeCoroutines)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
        activeCoroutines.Clear();
        
        base.OnDestroy();
    }
    
    // 安全地添加协程
    protected Coroutine StartManagedCoroutine(IEnumerator routine)
    {
        var coroutine = StartCoroutine(routine);
        activeCoroutines.Add(coroutine);
        return coroutine;
    }
}
```

### 3. 避免频繁创建协程
```csharp
// 不推荐：每帧都可能创建新协程
void Update()
{
    if (condition)
    {
        CoroutineUtility.Delay(0.1f, DoSomething);
    }
}

// 推荐：复用协程或控制创建频率
private Coroutine updateCoroutine;
void Start()
{
    updateCoroutine = CoroutineUtility.RepeatForever(() => {
        if (condition)
        {
            DoSomething();
        }
    }, 0.1f);
}
```

---

## 常见问题解答

### Q: 协程和线程的区别？
A: Unity 协程在主线程中运行，只是将执行分散到多个帧，不存在线程安全问题。

### Q: 协程会影响性能吗？
A: 大量同时运行的协程会有一定开销，建议合理控制数量，及时停止不需要的协程。

### Q: 如何在场景切换时保持协程？
A: `CoroutineUtility` 会自动创建 `DontDestroyOnLoad` 的运行器，协程会在场景切换时保持运行。

### Q: 可以同时运行多个协程吗？
A: 可以，每个协程都是独立的，可以同时运行任意数量的协程。

---

## 总结

`CoroutineUtility` 提供了游戏开发中常用的各种协程模式，包括：

- ✅ **延迟执行**（时间、帧数、条件）
- ✅ **重复执行**（指定次数、无限循环）
- ✅ **持续执行**（带进度参数）
- ✅ **自动管理**（无需手动创建 MonoBehaviour）
- ✅ **线程安全**（安全的创建和销毁）

使用这些工具可以大大简化异步操作的代码编写，提高开发效率。记住合理管理协程生命周期，避免不必要的性能开销。

---

*文档更新时间：2025年9月19日*  
*CoroutineUtility 版本：1.0.0*