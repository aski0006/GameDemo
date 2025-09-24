# AsakiRPG 项目概述

AsakiRPG 是一个基于 Unity 引擎开发的 2D 回合制 RPG 游戏项目。该项目使用了自定义的 AsakiFramework 框架，该框架提供了一系列核心功能，如事件总线、服务定位器、动作系统和对象池等。

## 项目结构

项目代码位于 `Assets/Scripts` 目录下，主要分为以下几个部分：

1.  **AsakiFramework**: 核心框架代码，提供了游戏开发的基础组件和系统。
2.  **Editor**: Unity 编辑器扩展代码，用于增强编辑器功能。
3.  **Extensions**: C# 扩展方法。
4.  **Gameplay**: 游戏核心逻辑代码，包括数据定义、系统实现、视图和控制器等。

## 核心框架 (AsakiFramework)

AsakiFramework 是一个为 Unity 游戏开发设计的轻量级框架，包含以下关键组件：

*   **AsakiMono**: 所有 MonoBehaviour 的基类，提供了日志、组件获取、协程快捷方法、线程安全锁和分帧创建等功能。
*   **AsakiMonoServiceLocator**: 基于单例的服务定位器，用于管理全局的 AsakiMono 服务。
*   **EventBus**: 全局事件总线，支持类型安全的事件订阅和发布，具有调试和统计功能。
*   **ActionSystem**: 动作系统，用于定义和执行游戏中的各种动作 (GameAction)，支持前置、后置处理和异步执行。
*   **ObjectPool**: 对象池系统，用于高效地管理和复用游戏对象和组件。
*   **CoroutineUtility**: 协程工具类，提供了常用的协程操作，如延迟执行、条件等待和循环执行等。

## 游戏逻辑 (Gameplay)

Gameplay 目录包含了游戏的核心逻辑实现，采用了 MVC 架构模式：

*   **Data**: 游戏数据定义，使用 ScriptableObject 来存储角色、卡牌等数据。
    *   `CombatantBaseData`: 战斗单位基础数据。
    *   `HeroCharacterData`: 英雄角色数据。
    *   `EnemyCharacterData`: 敌人角色数据。
    *   `CardData`: 卡牌数据。
*   **Effects**: 游戏效果定义，定义了卡牌使用时产生的各种效果。
*   **GA (Game Actions)**: 游戏动作定义，定义了游戏中可以执行的各种原子操作，如抽卡、出牌、回合结束等。
*   **System**: 游戏系统实现，负责处理游戏逻辑和状态更新。
    *   `CardSystem`: 卡牌系统，管理卡牌的抽牌、弃牌、出牌等逻辑。
*   **MVC**: Model-View-Controller 架构的实现。
*   **View**: 视图组件，负责游戏界面的显示和交互。
*   **Controller**: 控制器组件，负责处理用户输入和游戏逻辑的交互。

## 开发约定

*   **命名空间**: 使用 `AsakiFramework` 和 `Gameplay` 等命名空间来组织代码。
*   **基类**: 所有 MonoBehaviour 都应该继承自 `AsakiMono`。
*   **事件**: 使用 `EventBus` 进行组件间的解耦通信。
*   **动作**: 使用 `ActionSystem` 来定义和执行游戏逻辑。
*   **数据**: 使用 ScriptableObject 来定义和存储游戏数据。

## 构建和运行

由于这是一个 Unity 项目，构建和运行需要使用 Unity 编辑器。

1.  打开 Unity Hub，添加项目文件夹。
2.  选择合适的 Unity 版本打开项目。
3.  在 Unity 编辑器中打开场景文件。
4.  点击 Play 按钮运行游戏。

## 未来发展

该项目目前处于开发阶段，未来可能会添加更多的游戏功能、优化现有系统、完善美术资源等。