using AsakiFramework;
using AsakiFramework.Data;
using Gameplay.Common.Target;
using SerializeReferenceEditor;
using System;
using UnityEngine;

namespace Gameplay.Data
{
    [CustomData(category: "藏品", description: "游戏中的藏品数据")]
    public class CollectionData : ScriptableObject
    {
        [Header("藏品名称")][field: SerializeField] public string CollectionName { get; private set; }
        [Header("藏品图标")][field: SerializeField] public Sprite CollectionIcon { get; private set; }
        [Header("藏品描述"), TextArea][field: SerializeField] public string CollectionDescription { get; private set; }
        [Header("藏品价格")][field: SerializeField] public int CollectionPrice { get; private set; }
        [Header("藏品将会触发的条件")][field: SerializeReference, SR] public CollectionCondition Conditions { get; private set; }
        [Header("藏品作用对象目标")][field: SerializeReference, SR] public AutoTargetEffect TargetEffect { get; private set; }
        [Header("使用自动目标")][field: SerializeField] public bool UseAutoTarget { get; private set; } = true;
        [Header("是否使用动作施放者作为目标")][field: SerializeField] public bool UseActionCasterAsTarget { get; private set; } = false; // 是否使用动作施放者作为目标
        [Header("藏品使用次数, -1为无限")][field: SerializeField] public int UseTimes { get; private set; } = -1;
    }

    /// <summary>
    /// 抽象藏品条件
    /// </summary>
    [Serializable]
    public abstract class CollectionCondition
    {
        [SerializeField] protected ActionSystem.ReactionTiming reactionTiming;
        public abstract void SubscribeCondition(Action<GameAction> reaction); // 订阅动作系统反应
        public abstract void UnsubscribeCondition(Action<GameAction> reaction); // 取消订阅动作系统反应
        public abstract bool SubConditionMet(); // 检查子条件是否满足
    }


}
