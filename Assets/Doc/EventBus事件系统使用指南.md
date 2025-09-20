# EventBus 事件系统使用指南

## 📋 目录
- [快速开始](#快速开始)
- [基础概念](#基础概念)
- [定义事件](#定义事件)
- [订阅事件](#订阅事件)
- [触发事件](#触发事件)
- [取消订阅](#取消订阅)
- [高级功能](#高级功能)
- [调试工具](#调试工具)
- [最佳实践](#最佳实践)
- [常见问题](#常见问题)

## 🚀 快速开始

### 1. 创建事件
```csharp
// 定义事件（使用结构体避免GC分配）
public struct PlayerDamagedEvent
{
    public int PlayerId;
    public float Damage;
    public Vector3 HitPosition;
}
```

### 2. 订阅事件
```csharp
public class PlayerController : MonoBehaviour
{
    private void OnEnable()
    {
        // 订阅事件
        EventBus.Instance.Subscribe<PlayerDamagedEvent>(OnPlayerDamaged);
    }
    
    private void OnDisable()
    {
        // 取消订阅事件
        EventBus.Instance.Unsubscribe<PlayerDamagedEvent>(OnPlayerDamaged);
    }
    
    private void OnPlayerDamaged(PlayerDamagedEvent evt)
    {
        Debug.Log($"玩家 {evt.PlayerId} 受到 {evt.Damage} 点伤害");
    }
}
```

### 3. 触发事件
```csharp
// 从任何地方触发事件
EventBus.Instance.Trigger(new PlayerDamagedEvent
{
    PlayerId = 1,
    Damage = 25.5f,
    HitPosition = transform.position
});
```

## 💡 基础概念

### 什么是EventBus？
EventBus是一个**类型安全**、**委托驱动**的全局事件系统，基于单例模式实现：
- **类型安全**: 编译时检查事件类型
- **零GC分配**: 使用结构体避免垃圾回收
- **高性能**: 委托调用，无反射开销
- **可调试**: 内置调试和性能监控工具
- **可扩展**: 支持任何结构体类型的事件

### 核心组件
1. **EventBus**: 主要事件管理器
2. **EventBusEditorWindow**: Unity编辑器调试窗口
3. **事件定义**: 用户定义的结构体
4. **事件处理器**: 订阅事件的回调方法

## 📝 定义事件

### 基本规则
```csharp
// ✅ 正确：使用结构体
public struct GameStartEvent
{
    public string LevelName;
    public int PlayerCount;
}

// ❌ 错误：不要使用类
public class GameStartEvent  // 会产生GC分配
{
    public string LevelName;
}
```

### 命名约定
- 事件名称以 **Event** 结尾
- 使用描述性的名称
- 遵循PascalCase命名法

```csharp
// ✅ 推荐命名
public struct PlayerMoveEvent
public struct ScoreChangedEvent
public struct CardPlayedEvent
public struct GameEndEvent
```

### 复杂事件示例
```csharp
// 卡牌游戏事件
public struct CardPlayedEvent
{
    public int CardId;          // 卡牌ID
    public int PlayerId;        // 玩家ID
    public Vector3 PlayPosition; // 出牌位置
    public float PlayTime;      // 出牌时间
}

// UI事件
public struct UIOpenEvent
{
    public string UIName;       // UI名称
    public bool IsModal;        // 是否为模态窗口
}

// 游戏状态事件
public struct GameStateChangeEvent
{
    public GameState OldState;  // 旧状态
    public GameState NewState;  // 新状态
    public float TransitionTime; // 过渡时间
}
```

## 🔗 订阅事件

### 方法1：直接订阅
```csharp
private void OnEnable()
{
    EventBus.Instance.Subscribe<PlayerDamagedEvent>(OnPlayerDamaged);
    EventBus.Instance.Subscribe<GameStartEvent>(OnGameStart);
}

private void OnDisable()
{
    EventBus.Instance.Unsubscribe<PlayerDamagedEvent>(OnPlayerDamaged);
    EventBus.Instance.Unsubscribe<GameStartEvent>(OnGameStart);
}
```

### 方法2：扩展方法订阅
```csharp
private void OnEnable()
{
    ((Action<PlayerDamagedEvent>)OnPlayerDamaged).Subscribe<PlayerDamagedEvent>();
}

private void OnDisable()
{
    ((Action<PlayerDamagedEvent>)OnPlayerDamaged).Unsubscribe<PlayerDamagedEvent>();
}
```

### 方法3：Lambda表达式
```csharp
private void OnEnable()
{
    EventBus.Instance.Subscribe<PlayerDamagedEvent>(evt =>
    {
        Debug.Log($"玩家受伤: {evt.Damage}");
        // 处理逻辑
    });
}
```

## 🎯 触发事件

### 基本触发
```csharp
// 创建事件实例
var damageEvent = new PlayerDamagedEvent
{
    PlayerId = playerId,
    Damage = damageAmount,
    HitPosition = hitPoint
};

// 触发事件
EventBus.Instance.Trigger(damageEvent);
```

### 内联触发
```csharp
EventBus.Instance.Trigger(new ScoreChangedEvent
{
    OldScore = oldScore,
    NewScore = newScore
});
```

### 从静态方法触发
```csharp
public static class GameEvents
{
    public static void TriggerPlayerMove(Vector3 position)
    {
        EventBus.Instance?.Trigger(new PlayerMoveEvent
        {
            Position = position,
            Timestamp = Time.time
        });
    }
}
```

## ❌ 取消订阅

### 自动取消订阅（推荐）
```csharp
private void OnDisable()
{
    // 在OnDisable中取消订阅，确保组件被禁用时清理
    EventBus.Instance.Unsubscribe<PlayerDamagedEvent>(OnPlayerDamaged);
}
```

### 条件取消订阅
```csharp
public void DisableDamageSystem()
{
    EventBus.Instance.Unsubscribe<PlayerDamagedEvent>(OnPlayerDamaged);
    isDamageSystemEnabled = false;
}
```

## 🚀 高级功能

### 1. 事件统计
```csharp
// 启用统计
EventBus.Instance.EnableStatistics = true;

// 获取统计信息
var stats = EventBus.Instance.GetStatistics();
foreach (var stat in stats)
{
    Debug.Log($"事件: {stat.EventType}");
    Debug.Log($"调用次数: {stat.InvokeCount}");
    Debug.Log($"平均耗时: {stat.AverageInvokeTimeMs:F2}ms");
}
```

### 2. 调试模式
```csharp
// 启用调试日志
EventBus.Instance.EnableDebugLogging = true;

// 打印状态
EventBus.Instance.PrintStatus();
```

### 3. 事件查询
```csharp
// 检查是否有处理器
bool hasHandlers = EventBus.Instance.HasHandler<PlayerDamagedEvent>();

// 获取处理器数量
int handlerCount = EventBus.Instance.GetHandlerCount<PlayerDamagedEvent>();
```

### 4. 清理事件
```csharp
// 清理特定事件
EventBus.Instance.ClearEvent<PlayerDamagedEvent>();

// 清理所有事件
EventBus.Instance.ClearAllEvents();
```

## 🛠️ 调试工具

### EventBus调试器窗口
**打开方式**: `Tools > AsakiFramework > EventBus Debugger`

功能：
- 📊 实时事件监控
- 🔍 事件搜索过滤
- 📈 性能统计图表
- ⚡ 慢事件检测
- 🧪 测试事件触发
- 🧹 事件清理工具

### Inspector集成
- 快速设置开关
- 事件列表显示
- 性能统计信息
- 一键打开调试器

### 控制台输出
```
[EventBus] Subscribed OnPlayerDamaged to event PlayerDamagedEvent
[EventBus] Triggered event PlayerDamagedEvent
[EventBus] Slow event detected: ComplexEvent took 15ms
```

## 🎯 最佳实践

### 1. 事件设计原则
```csharp
// ✅ 好：简洁明了
public struct PlayerJumpEvent
{
    public Vector3 JumpVelocity;
}

// ❌ 避免：过于复杂
public struct PlayerComplexActionEvent
{
    public Vector3 Position;
    public Vector3 Velocity;
    public Quaternion Rotation;
    public int Health;
    public int Mana;
    public bool IsGrounded;
    // ... 太多字段
}
```

### 2. 命名规范
```csharp
// ✅ 推荐
PlayerDamagedEvent     // 玩家受伤事件
ScoreChangedEvent      // 分数变化事件
CardPlayedEvent        // 卡牌打出事件
GameStartEvent         // 游戏开始事件

// ❌ 避免
PlayerHurt             // 缺少Event后缀
OnDamage              // 听起来像方法名
damage_event          // 不符合C#命名规范
```

### 3. 生命周期管理
```csharp
public class GameManager : MonoBehaviour
{
    private void OnEnable()
    {
        // 订阅事件
        EventBus.Instance.Subscribe<GameStartEvent>(OnGameStart);
        EventBus.Instance.Subscribe<GameEndEvent>(OnGameEnd);
    }
    
    private void OnDisable()
    {
        // 必须取消订阅，避免内存泄漏
        EventBus.Instance.Unsubscribe<GameStartEvent>(OnGameStart);
        EventBus.Instance.Unsubscribe<GameEndEvent>(OnGameEnd);
    }
}
```

### 4. 错误处理
```csharp
private void OnPlayerDamaged(PlayerDamagedEvent evt)
{
    try
    {
        // 事件处理逻辑
        ProcessDamage(evt.PlayerId, evt.Damage);
    }
    catch (Exception ex)
    {
        Debug.LogError($"处理玩家伤害事件时出错: {ex.Message}");
    }
}
```

### 5. 性能优化
```csharp
// ✅ 好：快速处理
private void OnScoreChanged(ScoreChangedEvent evt)
{
    scoreText.text = evt.NewScore.ToString();
}

// ❌ 避免：耗时操作
private void OnScoreChanged(ScoreChangedEvent evt)
{
    // 不要在这里做复杂计算
    var complexCalculation = PerformExpensiveOperation(evt.NewScore);
    
    // 不要在这里加载资源
    var texture = Resources.Load<Texture>("complex-texture");
    
    // 如果需要，使用协程或任务系统
    StartCoroutine(ProcessScoreAsync(evt.NewScore));
}
```

## 🎮 实战示例

### 卡牌游戏事件系统
```csharp
// 卡牌事件定义
public struct CardDrawnEvent
{
    public int CardId;
    public int PlayerId;
}

public struct CardPlayedEvent
{
    public int CardId;
    public int PlayerId;
    public Vector3 PlayPosition;
}

public struct TurnChangedEvent
{
    public int PreviousPlayerId;
    public int CurrentPlayerId;
    public int TurnNumber;
}

// 卡牌管理器
public class CardManager : MonoBehaviour
{
    private void OnEnable()
    {
        EventBus.Instance.Subscribe<CardDrawnEvent>(OnCardDrawn);
        EventBus.Instance.Subscribe<CardPlayedEvent>(OnCardPlayed);
    }
    
    private void OnDisable()
    {
        EventBus.Instance.Unsubscribe<CardDrawnEvent>(OnCardDrawn);
        EventBus.Instance.Unsubscribe<CardPlayedEvent>(OnCardPlayed);
    }
    
    private void OnCardDrawn(CardDrawnEvent evt)
    {
        Debug.Log($"玩家 {evt.PlayerId} 抽到了卡牌 {evt.CardId}");
        // 更新手牌显示
    }
    
    private void OnCardPlayed(CardPlayedEvent evt)
    {
        Debug.Log($"玩家 {evt.PlayerId} 打出了卡牌 {evt.CardId}");
        // 处理卡牌效果
    }
    
    public void PlayCard(int cardId, int playerId)
    {
        // 触发卡牌打出事件
        EventBus.Instance.Trigger(new CardPlayedEvent
        {
            CardId = cardId,
            PlayerId = playerId,
            PlayPosition = transform.position
        });
    }
}
```

### UI事件处理
```csharp
// UI事件定义
public struct ButtonClickedEvent
{
    public string ButtonName;
    public int PlayerId;
}

public struct UIOpenEvent
{
    public string UIName;
}

// UI管理器
public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject inventoryPanel;
    
    private void OnEnable()
    {
        EventBus.Instance.Subscribe<ButtonClickedEvent>(OnButtonClicked);
        EventBus.Instance.Subscribe<UIOpenEvent>(OnUIOpen);
    }
    
    private void OnDisable()
    {
        EventBus.Instance.Unsubscribe<ButtonClickedEvent>(OnButtonClicked);
        EventBus.Instance.Unsubscribe<UIOpenEvent>(OnUIOpen);
    }
    
    private void OnButtonClicked(ButtonClickedEvent evt)
    {
        switch (evt.ButtonName)
        {
            case "SettingsButton":
                ShowSettings();
                break;
            case "InventoryButton":
                ShowInventory();
                break;
        }
    }
    
    private void OnUIOpen(UIOpenEvent evt)
    {
        Debug.Log($"打开UI: {evt.UIName}");
    }
    
    public void ShowSettings()
    {
        settingsPanel.SetActive(true);
        EventBus.Instance.Trigger(new UIOpenEvent { UIName = "Settings" });
    }
    
    public void ShowInventory()
    {
        inventoryPanel.SetActive(true);
        EventBus.Instance.Trigger(new UIOpenEvent { UIName = "Inventory" });
    }
}
```

## ❓ 常见问题

### Q: 事件处理器没有被调用？
**A**: 检查以下几点：
1. 确保在`OnEnable`中订阅事件
2. 确保事件名称拼写正确
3. 检查EventBus实例是否存在
4. 确认事件确实被触发了

```csharp
private void OnEnable()
{
    if (EventBus.Instance != null)
    {
        EventBus.Instance.Subscribe<YourEvent>(OnYourEvent);
    }
}
```

### Q: 如何调试事件系统？
**A**: 使用内置调试工具：
```csharp
// 启用调试日志
EventBus.Instance.EnableDebugLogging = true;

// 打印状态
EventBus.Instance.PrintStatus();

// 打开编辑器窗口
Tools > AsakiFramework > EventBus Debugger
```

### Q: 事件处理很慢怎么办？
**A**: 
1. 启用统计功能检查慢事件
2. 优化事件处理逻辑
3. 使用协程处理耗时操作
4. 考虑使用对象池

### Q: 内存泄漏问题？
**A**: 确保在`OnDisable`中取消订阅：
```csharp
private void OnDisable()
{
    EventBus.Instance.Unsubscribe<YourEvent>(OnYourEvent);
}
```

### Q: 可以跨场景使用吗？
**A**: 是的！EventBus继承自Singleton，使用`DontDestroyOnLoad`，可以跨场景使用。

## 🔗 相关链接

- [AsakiFramework文档](IFLOW.md)
- [Singleton模式文档](AsakiFramework/Singleton.cs)
- [对象池系统](AsakiFramework/ObjectPool/)
- [Unity事件系统](https://docs.unity3d.com/Manual/Events.html)

---

**最后更新**: 2025年9月20日  
**版本**: EventBus v1.0  
**作者**: AsakiFramework Team