using Gameplay.Common.Target;
using System;
using UnityEditor;
using Gameplay.Data;
using Gameplay.MVC.Controller;
using Gameplay.MVC.Interfaces;
using System.Collections.Generic;

namespace Gameplay.MVC.Model
{
    /// <summary>
    /// 简单的藏品数据模型（MVC Model）
    /// 职责：
    /// - 持有 CollectionData（只读引用）
    /// - 跟踪可用使用次数（-1 表示无限）
    /// - 暴露基础操作：CanUse / TryUse / Reset / Activate / Deactivate
    /// - 提供事件通知（OnUsed / OnExpired）
    /// 
    /// 说明：
    /// - 本类不直接处理动作系统的订阅逻辑（CollectionCondition），把那部分留给更高一层（系统/控制器）来进行订阅和回调，
    ///   这样避免与具体的 GameAction 类型在此处产生强耦合。
    /// - ModelInstanceID 使用 UnityEditor.GUID 的默认值（默认 GUID），在需要时上层可以替换或扩展以生成真实实例 ID。
    /// </summary>
    public class CollectionModel : IModel
    {
        // 实例 ID（实现 IModel）
        private readonly GUID modelInstanceID = default;
        public GUID ModelInstanceID => modelInstanceID;

        // 持有的数据（只读）
        public CollectionData Data { get; private set; }

        // 剩余使用次数，-1 表示无限
        public int RemainingUses { get; private set; } = 0;

        // 是否激活（可被使用/生效）
        public bool IsActive { get; private set; } = true;

        // 已失效（用于标记已耗尽且不可再用）
        public bool IsExpired => (Data != null && Data.UseTimes >= 0 && RemainingUses == 0) || !IsActive;

        // 事件：每次使用后和耗尽时触发
        public event Action<CollectionModel> OnUsed;
        public event Action<CollectionModel> OnExpired;

        private CollectionCondition condition;
        private AutoTargetEffect autoTargetEffect;

        public CollectionModel()
        {
            // 保留无参构造，允许延迟初始化
        }

        public CollectionModel(CollectionData data)
        {
            Initialize(data);
        }

        /// <summary>
        /// 初始化模型，设置数据与使用次数
        /// </summary>
        public void Initialize(CollectionData data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            RemainingUses = data.UseTimes;
            IsActive = true;
            condition = data.Conditions;
            autoTargetEffect = data.TargetEffect;
        }
        
        /// <summary>
        /// 是否可以使用（激活且剩余次数非0，-1 表示无限）
        /// </summary>
        public bool CanUse()
        {
            if (!IsActive) return false;
            if (Data == null) return false;
            return Data.UseTimes == -1 || RemainingUses > 0;
        }

        /// <summary>
        /// 尝试使用一次藏品，返回是否成功使用。
        /// 成功时触发 OnUsed，若使用后耗尽则触发 OnExpired 并将 IsActive 设为 false。
        /// </summary>
        public bool TryUse()
        {
            if (!CanUse()) return false;

            // 如果是有限次数，递减
            if (RemainingUses > 0)
            {
                RemainingUses--;
            }

            OnUsed?.Invoke(this);

            // 如果耗尽（且原本不是无限），标记过期并触发事件
            if (Data.UseTimes >= 0 && RemainingUses == 0)
            {
                IsActive = false;
                OnExpired?.Invoke(this);
            }

            return true;
        }

        /// <summary>
        /// 重置为初始使用次数（来自 CollectionData）
        /// </summary>
        public void ResetUses()
        {
            if (Data == null) return;
            RemainingUses = Data.UseTimes;
            IsActive = true;
        }

        /// <summary>
        /// 强制激活/禁用模型（不改变 RemainingUses）
        /// </summary>
        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;
    }
}