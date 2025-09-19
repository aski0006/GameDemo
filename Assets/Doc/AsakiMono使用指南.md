# AsakiMono 使用文档

## 概述

`AsakiMono` 是 AsakiFramework 提供的基础 MonoBehaviour 类，旨在为 Unity 游戏开发提供一套标准化的基础功能。它集成了日志系统、组件管理工具，并作为框架内所有 MonoBehaviour 组件的基类。

## 核心特性

- 📝 **智能日志系统**：支持详细日志控制，发布版零开销
- 🔧 **组件管理工具**：GetOrAddComponent、缓存组件、非空检查
- 🛡️ **编辑器增强**：编辑器下严格检查，发布版性能优化
- 🎯 **标准化基类**：统一的生命周期管理和工具方法
- ⚡ **高性能设计**：JIT 内联优化，零GC分配

---

## 基础使用方法

### 1. 继承 AsakiMono
代替直接继承 MonoBehaviour，让你的类继承 AsakiMono：

```csharp
using AsakiFramework;

public class PlayerController : AsakiMono
{
    void Start()
    {
        LogInfo("玩家控制器初始化完成");
    }
    
    void Update()
    {
        LogInfo("玩家更新中...", this);
    }
}
```

### 2. 启用详细日志
在 Inspector 面板中勾选 `Is Verbose` 来启用详细日志输出：

```csharp
public class GameManager : AsakiMono
{
    void Start()
    {
        // 这条日志只有在 Is Verbose 勾选时才会显示
        LogInfo("游戏管理器启动");
        
        // 错误日志不受 Is Verbose 限制，总是显示
        LogError("这是一个错误信息");
    }
}
```

---

## 日志系统详解

### 📝 基础日志方法

#### 信息日志 (LogInfo)
```csharp
public class Example : AsakiMono
{
    void Start()
    {
        // 基础信息日志
        LogInfo("程序启动");
        
        // 带上下文的日志（点击日志可定位到对象）
        LogInfo("对象初始化完成", this);
        
        // 格式化日志
        LogInfo($"当前时间：{Time.time}");
        LogInfo($"玩家位置：{transform.position}");
    }
}
```

#### 警告日志 (LogWarning)
```csharp
public class WarningExample : AsakiMono
{
    void Update()
    {
        if (health < 20)
        {
            // 警告日志，需要开启 Is Verbose
            LogWarning("血量过低！");
        }
    }
    
    private int health = 15;
}
```

#### 错误日志 (LogError)
```csharp
public class ErrorExample : AsakiMono
{
    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            // 错误日志总是显示，不受 Is Verbose 限制
            LogError("未找到 Rigidbody 组件！");
        }
    }
}
```

#### 断言日志 (LogAssert)
```csharp
public class AssertExample : AsakiMono
{
    public void TakeDamage(int damage)
    {
        // 确保伤害值不为负
        LogAssert(damage >= 0, "伤害值不能为负数！");
        
        if (damage >= 0)
        {
            health -= damage;
            LogInfo($"受到伤害：{damage}，当前血量：{health}");
        }
    }
    
    private int health = 100;
}
```

### 🎯 实际应用：完整日志系统示例

```csharp
using AsakiFramework;

public class PlayerSystem : AsakiMono
{
    [Header("玩家属性")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float moveSpeed = 5f;
    
    private int currentHealth;
    private bool isAlive = true;
    
    void Awake()
    {
        LogInfo("玩家系统初始化开始");
        InitializeComponents();
        LogInfo("玩家系统初始化完成", this);
    }
    
    void Start()
    {
        currentHealth = maxHealth;
        LogInfo($"玩家血量设置为：{currentHealth}/{maxHealth}");
    }
    
    void Update()
    {
        if (!isAlive) return;
        
        HandleInput();
        UpdateMovement();
        
        // 只在详细模式下显示每帧更新
        LogInfo($"玩家位置：{transform.position}", this);
    }
    
    public void TakeDamage(int damage)
    {
        LogAssert(damage > 0, "伤害值必须大于0");
        
        if (damage <= 0)
        {
            LogWarning($"无效的伤害值：{damage}");
            return;
        }
        
        int actualDamage = Mathf.Min(damage, currentHealth);
        currentHealth -= actualDamage;
        
        LogInfo($"玩家受到 {actualDamage} 点伤害，剩余血量：{currentHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        isAlive = false;
        LogError($"玩家死亡！位置：{transform.position}");
        // 死亡逻辑...
    }
    
    private void InitializeComponents()
    {
        // 确保必要的组件存在
        if (!GetComponent<Rigidbody>())
        {
            LogError("玩家缺少 Rigidbody 组件！");
        }
        
        if (!GetComponent<Collider>())
        {
            LogWarning("玩家缺少 Collider 组件");
        }
    }
    
    private void HandleInput()
    {
        // 输入处理逻辑
        LogInfo("处理玩家输入");
    }
    
    private void UpdateMovement()
    {
        // 移动逻辑
        LogInfo($"移动速度：{moveSpeed}");
    }
}
```

---

## 组件管理工具详解

### 🔧 GetOrAddComponent 智能获取

#### 基础用法
```csharp
public class ComponentExample : AsakiMono
{
    void Start()
    {
        // 获取或添加 Rigidbody（默认模式：Self）
        Rigidbody rb = GetOrAddComponent<Rigidbody>();
        LogInfo($"Rigidbody 获取成功：{rb != null}");
        
        // 获取或添加 Collider
        Collider col = GetOrAddComponent<Collider>();
        LogInfo($"Collider 获取成功：{col != null}");
    }
}
```

#### 不同搜索模式
```csharp
public class SearchModes : AsakiMono
{
    void Start()
    {
        // 1. Self 模式（默认）：在当前 GameObject 上查找或添加
        Rigidbody selfRb = GetOrAddComponent<Rigidbody>(FindComponentMode.Self);
        LogInfo("Self 模式：在当前对象上获取 Rigidbody");
        
        // 2. Parent 模式：在父对象链中查找（不自动添加）
        Rigidbody parentRb = GetOrAddComponent<Rigidbody>(FindComponentMode.Parent);
        if (parentRb != null)
        {
            LogInfo("Parent 模式：在父对象中找到 Rigidbody");
        }
        else
        {
            LogWarning("Parent 模式：在父对象中未找到 Rigidbody");
        }
        
        // 3. Children 模式：在子对象中查找（不自动添加）
        Collider childCol = GetOrAddComponent<Collider>(FindComponentMode.Children);
        if (childCol != null)
        {
            LogInfo("Children 模式：在子对象中找到 Collider");
        }
        else
        {
            LogWarning("Children 模式：在子对象中未找到 Collider");
        }
        
        // 4. Scene 模式：在整个场景中查找（不自动添加）
        Light sceneLight = GetOrAddComponent<Light>(FindComponentMode.Scene);
        if (sceneLight != null)
        {
            LogInfo("Scene 模式：在场景中找到 Light");
        }
        else
        {
            LogWarning("Scene 模式：在场景中未找到 Light");
        }
    }
}
```

#### 实际应用：UI 组件自动配置
```csharp
public class UIAutoSetup : AsakiMono
{
    [Header("UI 组件")]
    [SerializeField] private bool autoSetupComponents = true;
    
    void Awake()
    {
        if (autoSetupComponents)
        {
            SetupRequiredComponents();
        }
    }
    
    private void SetupRequiredComponents()
    {
        LogInfo("开始自动配置 UI 组件");
        
        // 确保有 Canvas Renderer
        CanvasRenderer renderer = GetOrAddComponent<CanvasRenderer>();
        LogInfo($"CanvasRenderer 配置完成：{renderer != null}");
        
        // 获取或添加 Image 组件
        UnityEngine.UI.Image image = GetOrAddComponent<UnityEngine.UI.Image>();
        if (image != null)
        {
            LogInfo("Image 组件已配置");
            SetupDefaultImage(image);
        }
        
        // 检查交互组件
        UnityEngine.UI.Button button = GetComponent<UnityEngine.UI.Button>();
        if (button != null)
        {
            LogInfo("找到 Button 组件，配置交互事件");
            SetupButtonEvents(button);
        }
    }
    
    private void SetupDefaultImage(UnityEngine.UI.Image image)
    {
        if (image.sprite == null)
        {
            LogWarning("Image 组件缺少 Sprite，使用默认设置");
            image.color = Color.white;
        }
    }
    
    private void SetupButtonEvents(UnityEngine.UI.Button button)
    {
        // 按钮事件配置逻辑
        LogInfo("按钮事件配置完成");
    }
}
```

### 🛡️ NotNullComponent 非空检查

#### 基础用法
```csharp
public class NotNullExample : AsakiMono
{
    [Header("必需组件")]
    [NotNullComponent] // 标记这个字段必须有对应的组件
    [SerializeField] private Rigidbody playerRigidbody;
    
    [NotNullComponent]
    [SerializeField] private Collider playerCollider;
    
    void Awake()
    {
        // 检查所有标记为 NotNullComponent 的字段
        ValidateRequiredComponents();
    }
    
    private void ValidateRequiredComponents()
    {
        try
        {
            // 检查 Rigidbody
            HasNotNullComponent<Rigidbody>();
            LogInfo("Rigidbody 检查通过");
            
            // 检查 Collider
            HasNotNullComponent<Collider>();
            LogInfo("Collider 检查通过");
        }
        catch (MissingComponentException e)
        {
            LogError($"必需组件缺失：{e.Message}");
            // 在编辑器中会抛出异常，便于快速定位问题
        }
    }
}
```

#### 实际应用：玩家角色验证
```csharp
using AsakiFramework;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerCharacter : AsakiMono
{
    [Header("玩家核心组件")]
    [NotNullComponent]
    [SerializeField] private Rigidbody characterRigidbody;
    
    [NotNullComponent]
    [SerializeField] private CapsuleCollider characterCollider;
    
    [NotNullComponent]
    [SerializeField] private Animator characterAnimator;
    
    void Awake()
    {
        LogInfo("开始验证玩家核心组件");
        
        try
        {
            // 验证所有必需组件
            ValidateCoreComponents();
            LogInfo("玩家核心组件验证通过");
        }
        catch (MissingComponentException e)
        {
            LogError($"玩家角色配置错误：{e.Message}");
            LogError("请确保玩家对象包含所有必需的组件！");
            
#if UNITY_EDITOR
            // 编辑器下提供修复建议
            SuggestFixes();
#endif
        }
    }
    
    private void ValidateCoreComponents()
    {
        // 验证刚体组件
        HasNotNullComponent<Rigidbody>();
        LogInfo("刚体组件验证通过");
        
        // 验证碰撞器组件
        HasNotNullComponent<CapsuleCollider>();
        LogInfo("碰撞器组件验证通过");
        
        // 验证动画组件
        HasNotNullComponent<Animator>();
        LogInfo("动画组件验证通过");
    }
    
#if UNITY_EDITOR
    private void SuggestFixes()
    {
        LogInfo("=== 修复建议 ===");
        LogInfo("1. 确保玩家对象添加了 Rigidbody 组件");
        LogInfo("2. 确保玩家对象添加了 CapsuleCollider 组件");
        LogInfo("3. 确保玩家对象添加了 Animator 组件");
        LogInfo("4. 检查组件是否被意外移除或禁用");
    }
#endif
}
```

### ⚡ GetCachedComponent 缓存组件

#### 基础用法
```csharp
public class CachedComponentExample : AsakiMono
{
    // 缓存字段
    private Rigidbody cachedRigidbody;
    private Collider cachedCollider;
    private Renderer cachedRenderer;
    
    void Start()
    {
        // 首次调用会缓存组件
        Rigidbody rb = GetCachedComponent(ref cachedRigidbody);
        LogInfo($"首次获取 Rigidbody：{rb != null}");
        
        // 后续调用直接使用缓存
        Rigidbody rb2 = GetCachedComponent(ref cachedRigidbody);
        LogInfo($"使用缓存获取 Rigidbody：{rb2 != null}");
        LogAssert(rb == rb2, "缓存组件应该相同");
    }
    
    void Update()
    {
        // 使用缓存组件，避免每帧查找
        if (cachedRigidbody != null)
        {
            LogInfo($"刚体速度：{cachedRigidbody.velocity.magnitude}");
        }
    }
}
```

#### 实际应用：高性能角色控制器
```csharp
using AsakiFramework;

public class PerformanceCharacter : AsakiMono
{
    [Header("性能优化")]
    [SerializeField] private bool useCachedComponents = true;
    
    // 组件缓存
    private Rigidbody rb;
    private Collider col;
    private Transform trans;
    private Animator anim;
    private Renderer renderer;
    
    // 频繁访问的子组件
    private Transform headTransform;
    private Transform handTransform;
    private Transform footTransform;
    
    void Awake()
    {
        LogInfo("开始高性能组件缓存");
        
        if (useCachedComponents)
        {
            CacheAllComponents();
        }
    }
    
    private void CacheAllComponents()
    {
        // 缓存基础组件
        trans = transform; // Transform 可以直接缓存
        rb = GetCachedComponent(ref rb);
        col = GetCachedComponent(ref col);
        anim = GetCachedComponent(ref anim);
        renderer = GetCachedComponent(ref renderer);
        
        LogInfo("基础组件缓存完成");
        
        // 缓存子对象变换
        CacheChildTransforms();
        
        LogInfo("所有组件缓存完成");
    }
    
    private void CacheChildTransforms()
    {
        // 查找特定的子对象
        Transform head = transform.Find("Head");
        if (head != null)
        {
            headTransform = head;
            LogInfo("头部变换缓存完成");
        }
        else
        {
            LogWarning("未找到头部对象");
        }
        
        Transform hand = transform.Find("Hand");
        if (hand != null)
        {
            handTransform = hand;
            LogInfo("手部变换缓存完成");
        }
        else
        {
            LogWarning("未找到手部对象");
        }
        
        Transform foot = transform.Find("Foot");
        if (foot != null)
        {
            footTransform = foot;
            LogInfo("脚部变换缓存完成");
        }
        else
        {
            LogWarning("未找到脚部对象");
        }
    }
    
    void Update()
    {
        // 使用缓存组件进行高性能更新
        UpdateMovement();
        UpdateAnimation();
        UpdateRendering();
    }
    
    private void UpdateMovement()
    {
        if (rb != null)
        {
            // 直接使用缓存的刚体组件
            Vector3 velocity = rb.velocity;
            LogInfo($"当前速度：{velocity.magnitude:F2}");
        }
    }
    
    private void UpdateAnimation()
    {
        if (anim != null)
        {
            // 使用缓存的动画组件
            float speed = rb.velocity.magnitude;
            anim.SetFloat("Speed", speed);
            LogInfo($"动画速度参数：{speed:F2}");
        }
    }
    
    private void UpdateRendering()
    {
        if (renderer != null)
        {
            // 使用缓存的渲染器
            Material material = renderer.material;
            LogInfo($"材质信息：{material.name}");
        }
    }
    
    // 公共方法，供外部调用
    public Vector3 GetHeadPosition()
    {
        return headTransform != null ? headTransform.position : transform.position;
    }
    
    public Vector3 GetHandPosition()
    {
        return handTransform != null ? handTransform.position : transform.position;
    }
    
    public Vector3 GetFootPosition()
    {
        return footTransform != null ? footTransform.position : transform.position;
    }
}
```

---

## 高级使用技巧

### 🎯 组合使用所有功能

```csharp
using AsakiFramework;

public class AdvancedGameSystem : AsakiMono
{
    [Header("系统配置")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool useComponentCaching = true;
    
    [Header("必需组件")]
    [NotNullComponent]
    [SerializeField] private GameObject playerObject;
    
    // 缓存组件
    private PlayerController playerController;
    private GameObject cachedGameObject;
    private Transform cachedTransform;
    
    void Awake()
    {
        // 配置日志系统
        ConfigureLogging();
        
        // 验证和缓存组件
        ValidateAndCacheComponents();
        
        LogInfo("高级游戏系统初始化完成");
    }
    
    private void ConfigureLogging()
    {
        // 根据配置启用详细日志
        if (enableDebugLogging)
        {
            LogInfo("详细日志已启用");
        }
        else
        {
            LogInfo("详细日志已禁用");
        }
    }
    
    private void ValidateAndCacheComponents()
    {
        try
        {
            // 验证必需组件
            HasNotNullComponent<GameObject>();
            LogInfo("游戏对象验证通过");
            
            // 缓存常用组件
            if (useComponentCaching)
            {
                CacheComponents();
            }
        }
        catch (MissingComponentException e)
        {
            LogError($"组件验证失败：{e.Message}");
            HandleMissingComponents();
        }
    }
    
    private void CacheComponents()
    {
        // 缓存基础组件
        cachedGameObject = gameObject;
        cachedTransform = transform;
        
        // 缓存玩家控制器
        if (playerObject != null)
        {
            playerController = playerObject.GetComponent<PlayerController>();
            if (playerController != null)
            {
                LogInfo("玩家控制器缓存完成");
            }
            else
            {
                LogWarning("玩家对象上未找到 PlayerController 组件");
            }
        }
        else
        {
            LogError("玩家对象未赋值！");
        }
        
        LogInfo("组件缓存完成");
    }
    
    private void HandleMissingComponents()
    {
        LogInfo("尝试自动修复缺失组件...");
        
        // 自动添加必需组件
        GetOrAddComponent<Rigidbody>();
        GetOrAddComponent<Collider>();
        
        LogInfo("自动修复完成");
    }
    
    void Update()
    {
        if (enableDebugLogging)
        {
            LogInfo($"系统更新中... 时间：{Time.time}");
        }
        
        // 使用缓存组件进行更新
        UpdateGameLogic();
    }
    
    private void UpdateGameLogic()
    {
        if (cachedTransform != null)
        {
            LogInfo($"当前位置：{cachedTransform.position}");
        }
        
        if (playerController != null)
        {
            // 更新玩家相关逻辑
            LogInfo("玩家逻辑更新完成");
        }
    }
    
    void OnDestroy()
    {
        LogInfo("游戏系统销毁中...");
        
        // 清理资源
        CleanupResources();
        
        LogInfo("游戏系统销毁完成");
    }
    
    private void CleanupResources()
    {
        // 清理缓存引用
        playerController = null;
        cachedGameObject = null;
        cachedTransform = null;
        
        LogInfo("资源清理完成");
    }
}
```

---

## 性能优化与最佳实践

### 1. 日志系统性能考虑

```csharp
public class OptimizedLogging : AsakiMono
{
    [Header("性能配置")]
    [SerializeField] private bool enableVerboseLogging = false;
    [SerializeField] private float logInterval = 1f;
    
    private float lastLogTime;
    
    void Update()
    {
        // 控制日志频率，避免每帧输出
        if (Time.time - lastLogTime >= logInterval)
        {
            LogSystemInfo();
            lastLogTime = Time.time;
        }
    }
    
    private void LogSystemInfo()
    {
        if (enableVerboseLogging)
        {
            LogInfo($"系统信息 - 时间：{Time.time}, 帧率：{1f / Time.deltaTime:F1}");
        }
    }
    
    // 高频调用的方法中避免日志输出
    private void HighFrequencyMethod()
    {
        // 不推荐：在每帧调用的方法中输出日志
        // LogInfo("高频方法调用"); // ❌
        
        // 推荐：使用条件编译或频率控制
#if UNITY_EDITOR
        if (Time.frameCount % 60 == 0) // 每秒输出一次
        {
            LogInfo("高频方法状态更新");
        }
#endif
    }
}
```

### 2. 组件缓存策略

```csharp
public class ComponentCachingStrategy : AsakiMono
{
    [Header("缓存策略")]
    [SerializeField] private CacheStrategy cacheStrategy = CacheStrategy.OnAwake;
    
    // 不同优先级的缓存
    private Transform _transform; // 最高优先级，立即缓存
    private Rigidbody _rigidbody; // 高优先级，Awake时缓存
    private Collider _collider;   // 中优先级，首次使用时缓存
    private Renderer _renderer;   // 低优先级，按需缓存
    
    void Awake()
    {
        // 立即缓存 Transform（最常用）
        _transform = transform;
        
        // 根据策略缓存其他组件
        switch (cacheStrategy)
        {
            case CacheStrategy.OnAwake:
                CacheComponentsOnAwake();
                break;
            case CacheStrategy.OnFirstUse:
                // 延迟缓存，在首次使用时进行
                break;
            case CacheStrategy.OnDemand:
                // 完全按需缓存
                break;
        }
    }
    
    private void CacheComponentsOnAwake()
    {
        // 缓存核心组件
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        
        LogInfo("组件缓存完成");
    }
    
    // 按需缓存的属性
    private Renderer Renderer
    {
        get
        {
            if (_renderer == null)
            {
                _renderer = GetComponent<Renderer>();
                LogInfo("Renderer 组件已缓存");
            }
            return _renderer;
        }
    }
    
    private enum CacheStrategy
    {
        OnAwake,      // Awake 时缓存
        OnFirstUse,   // 首次使用时缓存
        OnDemand      // 完全按需缓存
    }
}
```

### 3. 错误处理与恢复

```csharp
public class RobustGameSystem : AsakiMono
{
    [Header("系统配置")]
    [SerializeField] private bool autoRecoverFromErrors = true;
    [SerializeField] private int maxRetryAttempts = 3;
    
    private int currentAttempts = 0;
    
    void Start()
    {
        TryInitializeSystem();
    }
    
    private void TryInitializeSystem()
    {
        try
        {
            InitializeWithValidation();
            LogInfo("系统初始化成功");
        }
        catch (System.Exception e)
        {
            LogError($"系统初始化失败：{e.Message}");
            
            if (autoRecoverFromErrors && currentAttempts < maxRetryAttempts)
            {
                currentAttempts++;
                LogInfo($"尝试恢复系统... (尝试 {currentAttempts}/{maxRetryAttempts})");
                
                // 延迟后重试
                Invoke(nameof(TryInitializeSystem), 1f);
            }
            else
            {
                LogError("系统初始化失败，无法恢复");
                HandleFatalError();
            }
        }
    }
    
    private void InitializeWithValidation()
    {
        // 验证必需组件
        ValidateRequiredComponents();
        
        // 初始化子系统
        InitializeSubsystems();
        
        // 验证初始化结果
        ValidateInitialization();
    }
    
    private void HandleFatalError()
    {
        LogError("发生致命错误，系统无法继续运行");
        
        // 禁用系统
        enabled = false;
        
        // 通知用户
        ShowErrorToUser();
    }
    
    private void ShowErrorToUser()
    {
        LogError("请检查控制台日志以获取详细信息");
    }
    
    private void ValidateRequiredComponents()
    {
        // 验证逻辑...
    }
    
    private void InitializeSubsystems()
    {
        // 初始化逻辑...
    }
    
    private void ValidateInitialization()
    {
        // 验证逻辑...
    }
}
```

---

## 常见问题解答

### Q: 为什么要使用 AsakiMono 而不是直接继承 MonoBehaviour？

A: AsakiMono 提供了以下优势：
- **标准化日志系统**：统一的日志格式和行为控制
- **性能优化**：发布版零开销的日志系统
- **开发效率**：丰富的组件管理工具
- **错误预防**：编辑器下的严格检查机制
- **代码规范**：统一的项目代码标准

### Q: Is Verbose 勾选与否有什么区别？

A: 
- **勾选 Is Verbose**：所有 LogInfo 和 LogWarning 都会显示，适用于开发和调试阶段
- **不勾选 Is Verbose**：只有 LogError 和 LogAssert 会显示，适用于发布版本
- **发布版性能**：无论是否勾选，发布版的日志调用都会被编译器优化掉，零性能开销

### Q: 如何处理缺失的必需组件？

A: 有几种处理方式：
```csharp
// 方式1：使用 GetOrAddComponent 自动添加
Rigidbody rb = GetOrAddComponent<Rigidbody>();

// 方式2：使用 try-catch 处理异常
try
{
    HasNotNullComponent<Rigidbody>();
}
catch (MissingComponentException e)
{
    LogError($"组件缺失：{e.Message}");
    // 自动添加或优雅降级
    gameObject.AddComponent<Rigidbody>();
}

// 方式3：条件编译，只在编辑器下严格检查
#if UNITY_EDITOR
    HasNotNullComponent<Rigidbody>();
#endif
```

### Q: 组件缓存什么时候使用最合适？

A: 建议在以下场景使用组件缓存：
- **频繁访问的组件**：如 Update 中每帧使用的组件
- **昂贵的查找操作**：如 GetComponentInChildren 在复杂层级中
- **稳定的组件引用**：生命周期内不会变化的组件
- **性能敏感的场景**：移动设备或大量对象的情况

### Q: 发布版的日志真的完全没有性能影响吗？

A: 是的，因为：
1. **条件编译**：使用 `[Conditional("UNITY_EDITOR")]` 特性
2. **JIT 内联**：编译器会内联空方法调用
3. **零GC分配**：没有对象创建或内存分配
4. **完全移除**：在 IL 代码层面完全移除调用

---

## 总结

AsakiMono 作为 AsakiFramework 的基础类，提供了：

- ✅ **智能日志系统**：开发期详细日志，发布版零开销
- ✅ **组件管理工具**：GetOrAddComponent、缓存、非空检查
- ✅ **编辑器增强**：严格的组件验证和错误提示
- ✅ **性能优化**：JIT 内联、零GC分配、高效缓存
- ✅ **标准化开发**：统一的项目代码规范和最佳实践

使用 AsakiMono 可以：
- 提高开发效率和代码质量
- 减少运行时错误和调试时间
- 优化发布版本性能
- 保持代码风格的一致性

---

*文档更新时间：2025年9月19日*  
*AsakiMono 版本：2.0.0*