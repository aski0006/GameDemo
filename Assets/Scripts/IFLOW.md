# AsakiRPG Unity项目 - 开发指南

## 项目概述

AsakiRPG 是一个基于Unity引擎的卡牌RPG游戏项目，采用自定义的AsakiFramework框架构建。项目采用MVC架构模式，包含完整的对象池系统、卡牌系统和UI框架。

### 核心技术栈
- **引擎**: Unity (C#)
- **框架**: 自定义AsakiFramework
- **动画**: DOTween
- **文本**: TextMeshPro
- **架构模式**: MVC (Model-View-Controller)

## 项目结构

```
E:\Projects\UnityGame\AsakiRPG\Assets\Scripts\
├───AsakiFramework\          # 自定义框架核心
├───Data\                     # 数据定义和配置
├───Editor\                   # 编辑器扩展工具
├───Gameplay\                 # 游戏逻辑实现
├───Model\                    # 数据模型层
└───Tests\                    # 测试代码
```

### 核心模块详解

#### 1. AsakiFramework - 自定义框架
**基础组件**:
- `AsakiMono.cs` - 框架基类，提供日志系统和组件管理
- `Singleton.cs` - 单例模式基类
- `CoroutineUtility.cs` - 协程工具类

**对象池系统**:
- `ObjectPoolManager.cs` - 统一管理多个对象池
- `GameObjectPool.cs` - GameObject对象池实现
- `ComponentPool.cs` - 组件对象池实现
- `GenericObjectPool.cs` - 通用对象池实现
- `IObjectPool.cs` - 对象池接口定义
- `IPoolable.cs` - 可回收对象接口

#### 2. Data - 数据层
- `CardData.cs` - 卡牌数据定义（ScriptableObject）
- `CustomDataAttribute.cs` - 自定义数据标记特性

#### 3. Gameplay - 游戏逻辑
**服务层**:
- `CardViewCreator.cs` - 卡牌视图创建服务，集成对象池

**视图层**:
- `CardViewer.cs` - 卡牌UI视图组件
- `HandViewer.cs` - 手牌区域视图
- `BackgroundViewer.cs` - 背景视图

**系统层**:
- `TestSystem.cs` - 测试系统

#### 4. Model - 数据模型
- `CardModel.cs` - 卡牌数据模型，封装CardData

#### 5. Editor - 编辑器工具
- `AsakiDatabaseEditorWindow.cs` - 自定义数据库编辑器窗口

## 开发规范

### 命名约定
- **命名空间**: 使用PascalCase，如 `AsakiFramework`, `Gameplay.View`
- **类名**: PascalCase，如 `CardViewer`, `ObjectPoolManager`
- **方法名**: PascalCase，如 `CreateCardView`, `GetComponent`
- **字段**: camelCase，私有字段使用下划线前缀，如 `_pools`, `cardName`
- **常量**: UPPER_CASE，如 `INITIAL_SIZE`

### 代码组织
- 每个类文件包含一个主要的public类
- 使用region块组织相关功能代码
- 添加XML文档注释说明public成员
- 使用Header特性标记Inspector显示字段

### 日志系统
框架提供分级日志系统：
```csharp
LogInfo("信息消息");      // 仅在编辑器中显示，需开启isVerbose
LogWarning("警告消息");   // 仅在编辑器中显示，需开启isVerbose  
LogError("错误消息");     // 总是显示，不受isVerbose限制
LogAssert(condition, "断言消息"); // 条件失败时显示错误
```

## 对象池使用指南

### 基本用法
```csharp
// 创建对象池
ObjectPool.Create(prefab, initialCapacity, maxCapacity, poolName);

// 获取对象
GameObject obj = ObjectPool.Get(prefab, position, rotation, parent);

// 归还对象
ObjectPool.Return(obj);
```

### 高级用法
```csharp
// 通过CardViewCreator创建卡牌视图
CardViewer cardView = cardViewCreator.CreateCardView(position, rotation, parent);
```

## 开发工作流程

### 1. 创建新卡牌
1. 在Unity编辑器中创建新的CardData资产：右键 -> Create -> Card Data
2. 配置卡牌属性：名称、描述、费用、精灵图
3. 在代码中创建CardModel实例：
   ```csharp
   CardModel model = new CardModel(cardData);
   ```

### 2. 创建卡牌视图
```csharp
// 获取CardViewCreator服务
CardViewCreator creator = FindObjectOfType<CardViewCreator>();

// 创建卡牌视图
CardViewer viewer = creator.CreateCardView(position, rotation, parent);
viewer.Setup(model); // 设置卡牌数据
```

### 3. 使用对象池
```csharp
// 框架自动管理对象池生命周期
// 只需调用CreateCardView即可，对象池会自动处理创建和回收
```

## 测试

### 运行测试
- 在Unity编辑器中打开Test Runner窗口
- 运行ObjectPool相关测试用例
- 检查控制台输出验证功能

### 测试文件
- `TestObjectPool.cs` - 对象池功能测试
- `ObjectPoolCompilationTest.cs` - 编译测试

## 性能优化建议

1. **对象池使用**: 频繁创建销毁的对象（如卡牌、特效）必须使用对象池
2. **缓存组件**: 使用`GetCachedComponent`方法缓存频繁访问的组件
3. **日志控制**: 发布版中日志会被自动剔除，确保性能
4. **资源管理**: 合理使用ScriptableObject存储配置数据

## 扩展建议

### 待实现功能
- 卡牌战斗系统
- 玩家手牌管理
- 游戏状态管理
- 音效系统
- 存档系统

### 框架增强
- 事件系统
- 资源加载管理器
- 网络同步支持
- 更完善的错误处理

## 注意事项

1. **对象池管理**: 确保正确归还对象到池中，避免内存泄漏
2. **组件依赖**: 使用`HasNotNullComponent`验证必需的组件依赖
3. **空引用检查**: 使用`LogAssert`进行关键条件断言
4. **编辑器扩展**: CustomDataAttribute用于标记可编辑的游戏数据

---

*最后更新: 2025年9月20日*
*框架版本: AsakiFramework v1.0*
*项目作者: Asaki*