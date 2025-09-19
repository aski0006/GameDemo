# AsakiRPG - Unity项目开发指南

## 项目概述

AsakiRPG 是一个基于 Unity 6000.0.23f1 开发的 2D RPG 游戏项目。项目采用 Universal Render Pipeline (URP) 作为渲染管线，并集成了多个实用的 Unity 插件和自定义框架。

### 核心技术栈
- **Unity版本**: 6000.0.23f1
- **渲染管线**: Universal Render Pipeline (URP)
- **输入系统**: Unity Input System (1.11.1)
- **动画系统**: DOTween (Demigiant)
- **开发工具**: Serialize Reference Editor, Colourful Hierarchy

## 项目结构

### 主要目录
```
Assets/
├── Scripts/
│   └── AsakiFramework/          # 自定义框架代码
│       └── Singleton.cs         # 单例模式基类
├── Plugins/
│   └── Demigiant/               # DOTween 动画插件
├── M Studio/                    # 层级着色器工具
├── SREditor/                    # 序列化引用编辑器
├── Scenes/                      # 游戏场景
├── Settings/                    # URP设置和渲染配置
└── Resources/                   # 资源配置
```

### 关键文件说明

#### 框架代码
- `Assets/Scripts/AsakiFramework/Singleton.cs` - 通用单例模式实现，支持线程安全和应用退出处理

#### 插件集成
- **DOTween**: 强大的动画和缓动系统，位于 `Assets/Plugins/Demigiant/DOTween/`
- **Serialize Reference Editor**: 增强 Unity 序列化引用功能的编辑器工具
- **Colourful Hierarchy**: 层级窗口着色工具，便于场景管理

#### 项目配置
- `Packages/manifest.json` - Unity 包管理器依赖配置
- `ProjectSettings/ProjectSettings.asset` - 项目主设置文件
- `ProjectSettings/ProjectVersion.txt` - Unity 版本信息

## 开发环境设置

### Unity版本要求
- Unity 6000.0.23f1 或更高版本
- 支持平台：Windows, macOS, Linux

### 依赖包
项目已配置以下关键依赖：
- `com.unity.render-pipelines.universal`: URP 渲染管线
- `com.unity.inputsystem`: 新输入系统
- `com.unity.feature.2d`: 2D 开发功能集
- `com.unity.timeline`: 时间轴系统

## 开发规范

### 代码规范
1. **命名空间**: 使用 `AsakiFramework` 作为核心框架命名空间
2. **单例模式**: 继承 `Singleton<T>` 类实现单例，确保线程安全
3. **注释**: 使用中文注释说明关键逻辑
4. **代码结构**: 遵循 Unity 最佳实践，保持代码整洁

### 项目约定
1. **场景管理**: 场景文件保存在 `Assets/Scenes/` 目录
2. **资源配置**: 使用 `Assets/Resources/` 存放运行时加载资源
3. **插件管理**: 第三方插件统一放置在 `Assets/Plugins/` 目录
4. **版本控制**: 配置 `.gitignore` 忽略 Library、Temp 等临时文件

## 常用操作

### 项目打开
1. 使用 Unity Hub 打开项目根目录
2. 确保 Unity 版本为 6000.0.23f1
3. 打开后 Unity 会自动导入资源和配置

### 场景编辑
- 主场景位于 `Assets/Scenes/SampleScene.unity`
- 使用 Colourful Hierarchy 工具可以通过重命名 GameObject 并添加特定符号来着色

### 动画制作
- 使用 DOTween 创建流畅的动画效果
- 在代码中添加 `using DG.Tweening` 命名空间
- 参考 DOTween 文档：http://dotween.demigiant.com

## 构建和发布

### 构建设置
- 默认分辨率：1920x1080
- 支持横屏和竖屏模式
- 目标平台可在 Build Settings 中配置

### 构建步骤
1. 打开 File > Build Settings
2. 选择目标平台（PC, Mac, Linux 等）
3. 点击 Build 生成可执行文件

## 故障排除

### 常见问题
1. **版本不匹配**: 确保使用 Unity 6000.0.23f1 打开项目
2. **包导入失败**: 检查网络连接，重新导入 Packages
3. **DOTween 设置**: 首次导入后需要运行 Setup DOTween

### 调试工具
- 使用 Unity Console 查看日志输出
- 利用 Serialize Reference Editor 调试序列化问题
- Colourful Hierarchy 帮助快速识别场景对象

## 扩展功能

### 计划功能
- 角色系统开发
- 战斗系统实现
- 物品和装备系统
- 存档和读档功能

### 开发建议
1. 继续使用 AsakiFramework 开发核心系统
2. 利用现有插件提高开发效率
3. 保持代码模块化，便于维护和扩展

---

*最后更新：2025年9月19日*
*Unity版本：6000.0.23f1*