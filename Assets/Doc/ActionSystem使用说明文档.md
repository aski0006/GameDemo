# AsakiFramework ActionSystem 使用文档

## 概述

ActionSystem 是 AsakiFramework 的核心系统之一，专为游戏动作系统设计。它提供了一个强大的、基于订阅者的动作处理框架，支持动作的前置/后置反应、执行器绑定、以及防止死循环的机制。

### 核心特性

- **类型安全的动作系统**：基于泛型和抽象类设计
- **三段式动作流程**：Pre（前置）→ Perform（执行）→ Post（后置）
- **反应链支持**：动作可以触发其他动作，形成复杂的反应链
- **死循环防护**：内置调用栈检测，防止动作循环调用
- **协程支持**：执行器可以使用协程实现复杂逻辑
- **去重订阅**：自动防止重复订阅相同的回调函数

## 核心组件

### 1. GameAction - 动作基类

所有游戏动作都必须继承自 `GameAction` 基类：

```csharp
public abstract class GameAction
{
    public List<GameAction> PreReactions { get; private set; } = new();     // 动作前置反应
    public List<GameAction> CurPerformReactions { get; private set; } = new(); // 动作当前执行反应
    public List<GameAction> PostReactions { get; private set; } = new();   // 动作后置反应
}
```

### 2. ActionSystem - 动作系统管理器

单例模式的管理器，负责协调整个动作系统的运行：

```csharp
public class ActionSystem : Singleton<ActionSystem>
{
    public void PerformGameAction(GameAction action, Action onFinish = null);
    public void SubscribePre<T>(Action<T> callback) where T : GameAction;
    public void SubscribePost<T>(Action<T> callback) where T : GameAction;
    public void AttachPerformer<T>(Func<T, IEnumerator> performer) where T : GameAction;
}
```

## 使用指南

### 第一步：定义游戏动作

创建自定义动作类，继承自 `GameAction`：

```csharp
using AsakiFramework;

// 基础攻击动作
public class AttackAction : GameAction
{
    public int AttackerId { get; set; }
    public int TargetId { get; set; }
    public float Damage { get; set; }
    public DamageType DamageType { get; set; }
}

// 治疗动作
public class HealAction : GameAction
{
    public int HealerId { get; set; }
    public int TargetId { get; set; }
    public float HealAmount { get; set; }
}

// 抽牌动作（卡牌游戏）
public class DrawCardAction : GameAction
{
    public int PlayerId { get; set; }
    public int DrawCount { get; set; }
}

// 使用技能动作
public class UseSkillAction : GameAction
{
    public int UserId { get; set; }
    public int SkillId { get; set; }
    public Vector3 TargetPosition { get; set; }
}
```

### 第二步：设置动作执行器

为动作类型绑定执行器（可选，如果不设置，动作将只触发订阅者）：

```csharp
public class GameActionPerformers : MonoBehaviour
{
    private void Awake()
    {
        // 绑定攻击动作的执行器
        ActionSystem.Instance.AttachPerformer<AttackAction>(PerformAttack);
        
        // 绑定治疗动作的执行器
        ActionSystem.Instance.AttachPerformer<HealAction>(PerformHeal);
        
        // 绑定抽牌动作的执行器
        ActionSystem.Instance.AttachPerformer<DrawCardAction>(PerformDrawCard);
        
        // 绑定技能动作的执行器
        ActionSystem.Instance.AttachPerformer<UseSkillAction>(PerformUseSkill);
    }
    
    private IEnumerator PerformAttack(AttackAction action)
    {
        Debug.Log($"[ActionSystem] 执行攻击: 玩家{action.AttackerId} 对 玩家{action.TargetId} 造成 {action.Damage} 点{action.DamageType}伤害");
        
        // 模拟攻击动画时间
        yield return new WaitForSeconds(0.5f);
        
        // 这里可以添加实际的伤害计算逻辑
        // 例如：GameManager.Instance.ApplyDamage(action.TargetId, action.Damage);
    }
    
    private IEnumerator PerformHeal(HealAction action)
    {
        Debug.Log($"[ActionSystem] 执行治疗: 玩家{action.HealerId} 治疗 玩家{action.TargetId} {action.HealAmount} 点生命值");
        
        // 模拟治疗特效时间
        yield return new WaitForSeconds(0.3f);
        
        // 实际治疗逻辑
        // GameManager.Instance.ApplyHeal(action.TargetId, action.HealAmount);
    }
    
    private IEnumerator PerformDrawCard(DrawCardAction action)
    {
        Debug.Log($"[ActionSystem] 执行抽牌: 玩家{action.PlayerId} 抽取 {action.DrawCount} 张牌");
        
        for (int i = 0; i < action.DrawCount; i++)
        {
            // 模拟逐张抽牌
            yield return new WaitForSeconds(0.2f);
            Debug.Log($"[ActionSystem] 玩家{action.PlayerId} 抽到第 {i + 1} 张牌");
        }
    }
    
    private IEnumerator PerformUseSkill(UseSkillAction action)
    {
        Debug.Log($"[ActionSystem] 执行技能: 玩家{action.UserId} 使用技能 {action.SkillId} 目标位置 {action.TargetPosition}");
        
        // 技能施法时间
        yield return new WaitForSeconds(1.0f);
        
        // 技能效果
        Debug.Log($"[ActionSystem] 技能 {action.SkillId} 释放完成");
    }
}
```

### 第三步：订阅动作事件

订阅动作的各个阶段事件：

```csharp
public class GameActionSubscribers : MonoBehaviour
{
    private void OnEnable()
    {
        // 订阅攻击动作的前置事件
        ActionSystem.Instance.SubscribePre<AttackAction>(OnAttackPre);
        
        // 订阅攻击动作的后置事件
        ActionSystem.Instance.SubscribePost<AttackAction>(OnAttackPost);
        
        // 订阅治疗动作的前置事件
        ActionSystem.Instance.SubscribePre<HealAction>(OnHealPre);
        
        // 订阅治疗动作的后置事件
        ActionSystem.Instance.SubscribePost<HealAction>(OnHealPost);
        
        // 订阅抽牌动作
        ActionSystem.Instance.SubscribePre<DrawCardAction>(OnDrawCardPre);
        ActionSystem.Instance.SubscribePost<DrawCardAction>(OnDrawCardPost);
    }
    
    private void OnDisable()
    {
        // 清理订阅（可选，ActionSystem会在场景卸载时自动清理）
        ActionSystem.Instance.ClearAll();
    }
    
    // 攻击动作前置处理
    private void OnAttackPre(AttackAction action)
    {
        Debug.Log($"[ActionSystem] 攻击前置: 检查攻击合法性");
        
        // 检查攻击者是否有足够的能量
        // 检查目标是否有效
        // 应用攻击修正值等
    }
    
    // 攻击动作后置处理
    private void OnAttackPost(AttackAction action)
    {
        Debug.Log($"[ActionSystem] 攻击后置: 处理攻击结果");
        
        // 触发连击效果
        // 更新UI
        // 检查死亡等
    }
    
    // 治疗动作前置处理
    private void OnHealPre(HealAction action)
    {
        Debug.Log($"[ActionSystem] 治疗前置: 检查治疗合法性");
        
        // 检查治疗者是否有治疗能力
        // 检查目标是否需要治疗
    }
    
    // 治疗动作后置处理
    private void OnHealPost(HealAction action)
    {
        Debug.Log($"[ActionSystem] 治疗后置: 处理治疗结果");
        
        // 更新生命值UI
        // 触发治疗特效
    }
    
    // 抽牌动作前置处理
    private void OnDrawCardPre(DrawCardAction action)
    {
        Debug.Log($"[ActionSystem] 抽牌前置: 检查牌库数量");
        
        // 检查牌库是否有足够的牌
        // 如果牌库为空，可能需要触发洗牌动作
    }
    
    // 抽牌动作后置处理
    private void OnDrawCardPost(DrawCardAction action)
    {
        Debug.Log($"[ActionSystem] 抽牌后置: 更新手牌UI");
        
        // 更新手牌显示
        // 检查手牌上限
    }
}
```

### 第四步：创建带有反应链的动作

动作可以包含其他动作作为反应，形成复杂的动作链：

```csharp
public class ComboActionExample : MonoBehaviour
{
    // 创建一个连击动作
    public void CreateComboAttack(int attackerId, int targetId)
    {
        var comboAttack = new AttackAction
        {
            AttackerId = attackerId,
            TargetId = targetId,
            Damage = 30f,
            DamageType = DamageType.Physical
        };
        
        // 为主攻击添加前置反应：增加攻击力
        comboAttack.PreReactions.Add(new BuffAction
        {
            TargetId = attackerId,
            BuffType = BuffType.AttackBoost,
            Value = 10f,
            Duration = 1
        });
        
        // 为主攻击添加执行反应：触发额外攻击
        comboAttack.CurPerformReactions.Add(new AttackAction
        {
            AttackerId = attackerId,
            TargetId = targetId,
            Damage = 15f,
            DamageType = DamageType.Magic
        });
        
        // 为主攻击添加后置反应：应用流血效果
        comboAttack.PostReactions.Add(new DOTAction
        {
            TargetId = targetId,
            DamagePerTick = 5f,
            TickCount = 3,
            DamageType = DamageType.Bleed
        });
        
        // 执行连击动作
        ActionSystem.Instance.PerformGameAction(comboAttack, OnComboComplete);
    }
    
    private void OnComboComplete()
    {
        Debug.Log("[ActionSystem] 连击动作完成！");
    }
    
    // 创建一个治疗链
    public void CreateHealChain(int healerId, List<int> targetIds)
    {
        GameAction firstHeal = null;
        GameAction previousHeal = null;
        
        foreach (var targetId in targetIds)
        {
            var healAction = new HealAction
            {
                HealerId = healerId,
                TargetId = targetId,
                HealAmount = 25f
            };
            
            if (firstHeal == null)
            {
                firstHeal = healAction;
            }
            else
            {
                // 将当前治疗动作添加到前一个治疗的后置反应中
                previousHeal.PostReactions.Add(healAction);
            }
            
            previousHeal = healAction;
        }
        
        // 执行治疗链
        if (firstHeal != null)
        {
            ActionSystem.Instance.PerformGameAction(firstHeal);
        }
    }
}

// 定义其他动作类型
public class BuffAction : GameAction
{
    public int TargetId { get; set; }
    public BuffType BuffType { get; set; }
    public float Value { get; set; }
    public int Duration { get; set; }
}

public class DOTAction : GameAction
{
    public int TargetId { get; set; }
    public float DamagePerTick { get; set; }
    public int TickCount { get; set; }
    public DamageType DamageType { get; set; }
}

public enum DamageType
{
    Physical,
    Magic,
    Fire,
    Ice,
    Lightning,
    Bleed,
    Poison
}

public enum BuffType
{
    AttackBoost,
    DefenseBoost,
    SpeedBoost,
    CriticalChance
}
```

### 第五步：实际使用动作系统

```csharp
public class GameController : MonoBehaviour
{
    private void Start()
    {
        // 执行简单的攻击
        var simpleAttack = new AttackAction
        {
            AttackerId = 1,
            TargetId = 2,
            Damage = 20f,
            DamageType = DamageType.Physical
        };
        
        ActionSystem.Instance.PerformGameAction(simpleAttack);
        
        // 延迟执行复杂动作
        StartCoroutine(ExecuteComplexActions());
    }
    
    private IEnumerator ExecuteComplexActions()
    {
        yield return new WaitForSeconds(2f);
        
        // 执行治疗动作
        var healAction = new HealAction
        {
            HealerId = 3,
            TargetId = 1,
            HealAmount = 30f
        };
        
        ActionSystem.Instance.PerformGameAction(healAction, () =>
        {
            Debug.Log("治疗完成回调！");
        });
        
        yield return new WaitForSeconds(1f);
        
        // 执行抽牌动作
        var drawAction = new DrawCardAction
        {
            PlayerId = 1,
            DrawCount = 3
        };
        
        ActionSystem.Instance.PerformGameAction(drawAction);
    }
    
    private void Update()
    {
        // 检查动作系统是否在运行中
        if (ActionSystem.Instance.IsRunning)
        {
            Debug.Log("[ActionSystem] 有动作正在执行中...");
        }
        
        // 测试键盘输入触发的动作
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var skillAction = new UseSkillAction
            {
                UserId = 1,
                SkillId = 101,
                TargetPosition = Vector3.forward * 5f
            };
            
            ActionSystem.Instance.PerformGameAction(skillAction);
        }
    }
}
```

## 高级用法

### 1. 条件动作系统

```csharp
public class ConditionalActionExample : MonoBehaviour
{
    private void Start()
    {
        // 创建一个条件攻击动作
        var conditionalAttack = new ConditionalAttackAction
        {
            AttackerId = 1,
            TargetId = 2,
            BaseDamage = 25f,
            Condition = () => HasCriticalBuff(1)
        };
        
        ActionSystem.Instance.PerformGameAction(conditionalAttack);
    }
    
    private bool HasCriticalBuff(int playerId)
    {
        // 检查玩家是否有暴击buff
        return true; // 简化示例
    }
}

public class ConditionalAttackAction : GameAction
{
    public int AttackerId { get; set; }
    public int TargetId { get; set; }
    public float BaseDamage { get; set; }
    public Func<bool> Condition { get; set; }
}

// 条件执行器
public class ConditionalActionPerformer : MonoBehaviour
{
    private void Awake()
    {
        ActionSystem.Instance.AttachPerformer<ConditionalAttackAction>(PerformConditionalAttack);
    }
    
    private IEnumerator PerformConditionalAttack(ConditionalAttackAction action)
    {
        if (action.Condition != null && action.Condition())
        {
            Debug.Log($"条件满足，执行暴击攻击！伤害: {action.BaseDamage * 2}");
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            Debug.Log($"条件不满足，执行普通攻击。伤害: {action.BaseDamage}");
            yield return new WaitForSeconds(0.3f);
        }
    }
}
```

### 2. 动作序列系统

```csharp
public class ActionSequenceExample : MonoBehaviour
{
    public void ExecuteActionSequence()
    {
        var sequence = new ActionSequence
        {
            Actions = new List<GameAction>
            {
                new AttackAction { AttackerId = 1, TargetId = 2, Damage = 20f },
                new WaitAction { Duration = 1f },
                new HealAction { HealerId = 3, TargetId = 1, HealAmount = 15f },
                new WaitAction { Duration = 0.5f },
                new AttackAction { AttackerId = 2, TargetId = 1, Damage = 25f }
            }
        };
        
        ActionSystem.Instance.PerformGameAction(sequence);
    }
}

public class ActionSequence : GameAction
{
    public List<GameAction> Actions { get; set; }
}

public class WaitAction : GameAction
{
    public float Duration { get; set; }
}

public class SequencePerformer : MonoBehaviour
{
    private void Awake()
    {
        ActionSystem.Instance.AttachPerformer<ActionSequence>(PerformSequence);
        ActionSystem.Instance.AttachPerformer<WaitAction>(PerformWait);
    }
    
    private IEnumerator PerformSequence(ActionSequence sequence)
    {
        foreach (var action in sequence.Actions)
        {
            bool completed = false;
            ActionSystem.Instance.PerformGameAction(action, () => completed = true);
            
            // 等待当前动作完成
            yield return new WaitUntil(() => completed);
        }
    }
    
    private IEnumerator PerformWait(WaitAction waitAction)
    {
        yield return new WaitForSeconds(waitAction.Duration);
    }
}
```

### 3. 动作日志和调试

```csharp
public class ActionSystemDebugger : MonoBehaviour
{
    private void Awake()
    {
        // 订阅所有动作的前置和后置事件用于调试
        SubscribeToAllActions();
    }
    
    private void SubscribeToAllActions()
    {
        // 获取所有GameAction的子类
        var actionTypes = typeof(GameAction).Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(GameAction)) && !t.IsAbstract);
        
        foreach (var actionType in actionTypes)
        {
            // 使用反射创建通用的订阅方法
            SubscribeToActionType(actionType);
        }
    }
    
    private void SubscribeToActionType(Type actionType)
    {
        // 这里简化处理，实际项目中可以更精细地控制
        Debug.Log($"[ActionSystemDebugger] 订阅动作类型: {actionType.Name}");
    }
    
    private void Update()
    {
        // 显示动作系统状态
        if (Input.GetKeyDown(KeyCode.F1))
        {
            PrintActionSystemStatus();
        }
    }
    
    private void PrintActionSystemStatus()
    {
        Debug.Log($"[ActionSystemDebugger] 动作系统运行状态: {(ActionSystem.Instance.IsRunning ? "运行中" : "空闲")}");
        Debug.Log($"[ActionSystemDebugger] 时间: {Time.time}");
    }
}
```

## 最佳实践

### 1. 动作设计原则

- **保持动作简单**：每个动作应该只负责一个明确的任务
- **避免副作用**：动作执行器不应该有意外副作用
- **使用结构体**：对于简单的事件数据，考虑使用结构体以避免GC分配

### 2. 性能优化

- **缓存动作实例**：对于频繁使用的动作，可以缓存实例
- **批量处理**：相关的动作可以组合成序列批量处理
- **异步执行**：长时间运行的动作使用协程避免阻塞

### 3. 错误处理

```csharp
public class SafeActionPerformer : MonoBehaviour
{
    private void Awake()
    {
        ActionSystem.Instance.AttachPerformer<RiskyAction>(PerformRiskyAction);
    }
    
    private IEnumerator PerformRiskyAction(RiskyAction action)
    {
        try
        {
            Debug.Log($"[SafeActionPerformer] 开始执行风险动作: {action.ActionId}");
            
            // 模拟可能失败的操作
            if (UnityEngine.Random.value < 0.3f)
            {
                throw new System.InvalidOperationException("模拟的动作执行失败");
            }
            
            yield return new WaitForSeconds(1f);
            
            Debug.Log($"[SafeActionPerformer] 风险动作执行成功: {action.ActionId}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SafeActionPerformer] 风险动作执行失败: {ex.Message}");
            
            // 可以在这里触发错误处理动作
            var errorAction = new ErrorAction
            {
                ErrorMessage = ex.Message,
                FailedActionId = action.ActionId
            };
            
            ActionSystem.Instance.PerformGameAction(errorAction);
        }
    }
}

public class RiskyAction : GameAction
{
    public int ActionId { get; set; }
    public float RiskFactor { get; set; }
}

public class ErrorAction : GameAction
{
    public string ErrorMessage { get; set; }
    public int FailedActionId { get; set; }
}
```

### 4. 动作组合模式

```csharp
public static class ActionBuilder
{
    public static AttackAction CreateCriticalAttack(int attackerId, int targetId, float baseDamage)
    {
        var attack = new AttackAction
        {
            AttackerId = attackerId,
            TargetId = targetId,
            Damage = baseDamage * 2f,
            DamageType = DamageType.Physical
        };
        
        // 添加暴击特效
        attack.PreReactions.Add(new VFXAction
        {
            EffectType = VFXType.CriticalHit,
            Position = Vector3.zero,
            Scale = 1.5f
        });
        
        return attack;
    }
    
    public static HealAction CreateAreaHeal(int healerId, List<int> targetIds, float baseHeal)
    {
        var mainHeal = new HealAction
        {
            HealerId = healerId,
            TargetId = healerId, // 治疗者自己也获得治疗
            HealAmount = baseHeal
        };
        
        // 为每个目标添加治疗动作
        foreach (var targetId in targetIds)
        {
            if (targetId != healerId) // 避免重复治疗治疗者
            {
                mainHeal.PostReactions.Add(new HealAction
                {
                    HealerId = healerId,
                    TargetId = targetId,
                    HealAmount = baseHeal * 0.8f // 区域治疗效果稍低
                });
            }
        }
        
        return mainHeal;
    }
}

public class VFXAction : GameAction
{
    public VFXType EffectType { get; set; }
    public Vector3 Position { get; set; }
    public float Scale { get; set; }
}

public enum VFXType
{
    CriticalHit,
    MagicHit,
    Healing,
    BuffApplication
}
```

## 常见问题

### Q: 动作系统与事件总线有什么区别？

A: 动作系统专注于**游戏逻辑的执行流程**，具有明确的执行顺序（Pre→Perform→Post）和协程支持。事件总线更适用于**组件间的解耦通信**，是简单的发布-订阅模式。

### Q: 如何处理动作的取消？

A: 可以通过在动作中添加取消标记，在执行器中定期检查：

```csharp
public class CancellableAction : GameAction
{
    public bool IsCancelled { get; set; }
    public string ActionData { get; set; }
}

// 在执行器中
private IEnumerator PerformCancellableAction(CancellableAction action)
{
    for (int i = 0; i < 10; i++)
    {
        if (action.IsCancelled)
        {
            Debug.Log("动作被取消");
            yield break;
        }
        
        yield return new WaitForSeconds(0.1f);
    }
}
```

### Q: 动作系统是否支持网络同步？

A: 当前版本主要设计用于单机游戏，但可以通过扩展支持网络同步。建议在动作执行前后添加网络同步点。

## 总结

ActionSystem 提供了一个强大而灵活的游戏动作处理框架，特别适合需要复杂动作交互的RPG、卡牌、策略等游戏类型。通过合理使用动作系统，可以：

- 实现清晰的游戏逻辑流程
- 构建复杂的动作反应链
- 保持代码的高度可维护性
- 支持协程和异步操作
- 避免常见的循环调用问题

建议在实际项目中结合具体需求，灵活运用动作系统的各项特性。

---

**文档版本**: 1.0  
**最后更新**: 2025年9月20日  
**框架版本**: AsakiFramework v1.0

---

# AsakiFramework ActionSystem 使用文档（v1.1 修订版）

**最近更新**：2025-09-20  
**框架版本**：AsakiFramework v1.1  
**变更重点**：
- 补充 `DetachPerformer<T>()` 用法与最佳实践
- 增加“热切换规则集”示例
- 细化性能与内存注意事项

---

## 1. 快速导航（新增/修正章节）
| 章节 | 变更类型 | 说明 |
|---|---|---|
| 2.3 释放执行器 | **新增** | 防止脏委托堆积 |
| 5.4 热切换规则集 | **新增** | 演示 `DetachPerformer` 典型使用场景 |
| 7.1 性能与内存 | **补充** | 明确“去重≠释放” |

---

## 2. 核心API增补

### 2.1 订阅执行器（Attach）
```csharp
ActionSystem.Instance.AttachPerformer<AttackAction>(PerformAttack);
```

### 2.2 释放执行器（Detach）★NEW
```csharp
// 卸下当前类型的执行器，允许后续重新绑定新逻辑
ActionSystem.Instance.DetachPerformer<AttackAction>();
```

**何时调用**
- 赛季轮换 / 模式切换（PVP→PVE）
- 场景卸载但 `ActionSystem` 单例仍在
- 动态MOD卸载

### 2.3 一键清空（ClearAll）
```csharp
ActionSystem.Instance.ClearAll();   // 预/后置、执行器全部清空
```

---

## 3. 热切换规则集示例 ★NEW

```csharp
public class RuleSetManager : MonoBehaviour
{
    private void SwitchToPVP()
    {
        // 1. 卸下旧规则
        ActionSystem.Instance.DetachPerformer<AttackAction>();
        ActionSystem.Instance.DetachPerformer<HealAction>();

        // 2. 绑定PVP专用逻辑
        ActionSystem.Instance.AttachPerformer<AttackAction>(PVPAttack);
        ActionSystem.Instance.AttachPerformer<HealAction>(PVPHeal);
    }

    private IEnumerator PVPAttack(AttackAction a)
    {
        /* 伤害减免、平衡修正等 */
        yield return new WaitForSeconds(0.2f);
    }

    private IEnumerator PVPHeal(HealAction h)
    {
        /* PVP治疗衰减 */
        yield return new WaitForSeconds(0.3f);
    }
}
```

---

## 4. 最佳实践更新

| 场景 | 推荐做法 |
|---|---|
| 多次绑定同一类型 | 系统会自动 **覆盖** 旧执行器，**无需手动Detach** |
| 动态MOD/插件 | 卸载前 **必须Detach**，否则残留委托 |
| 单元测试 | 在 `[TearDown]` 调用 `ClearAll()` 保证测试隔离 |

---

## 5. 常见问题FAQ（增补）

**Q**: 重复 `AttachPerformer` 会泄漏吗？  
**A**: 不会，字典 **同Key覆盖**；但 **旧委托实例仍在字典里占位**，长期热切换建议 **先Detach再Attach**。

**Q**: 是否需要配套 `UnsubscribePre/Post`？  
**A**: 文档暂未提供，如需运行时动态取消订阅，可参照 `DetachPerformer` 模式自行扩展 `UnsubscribePre<T>(Action<T> callback)`。

---

## 6. 版本历史

| 版本 | 日期 | 说明 |
|---|---|---|
| v1.0 | 2025-09-20 | 初始文档 |
| v1.1 | 2025-09-20 | 补充 `DetachPerformer`、热切换示例、性能提示 |

---

**一句话总结**
> **“Attach了就要能Detach，热切换不Detach才会真泄漏。”**