using AsakiFramework;
using AsakiFramework.Data;
using Gameplay.Common.Target;
using SerializeReferenceEditor;
using System;
using UnityEngine;

namespace Gameplay.MVC.Data
{
    [CustomData(category: "藏品", description: "游戏中的肉鸽藏品数据")]
    public class CollectionData : ScriptableObject
    {
        [Header("藏品数据")]
        [Tooltip("藏品在游戏中显示的图标精灵")]
        [field: SerializeField] public Sprite CollectionSprite { get; private set; }
        [Tooltip("藏品触发的条件，决定藏品在什么情况下会生效")]
        [field: SerializeReference, SR] public CollectionTriggerCondition TriggerCondition { get; private set; }
        [Tooltip("藏品生效时产生的自动目标效果，会对目标产生相应的影响")]
        [field: SerializeField] public AutoTargetEffect CollectionEffect { get; private set; }
        [Tooltip("是否使用自动目标机制，若为 true 则自动选择目标来应用藏品效果")]
        [field: SerializeField] public bool UseAutoTarget { get; private set; }
        [Tooltip("是否将动作的施法者作为目标，若为 true 则藏品效果会作用于施法者")]
        [field: SerializeField] public bool UseActionCasterAsTarget { get; private set; }
        [Tooltip("是否允许此藏品产生的 PerformEffectGA 继续触发其他藏品，默认为 true")]
        [field: SerializeField] public bool AllowPropagation { get; private set; } = true;
        [Tooltip("在连锁中允许的最大链长（含本藏品），防止无限递归，0 表示无限制，默认值为 4")]
        [field: SerializeField, Min(0)] public int MaxChainDepth { get; private set; } = 4;
    }


    [Serializable]
    public abstract class CollectionTriggerCondition
    {
        [Tooltip("藏品触发的时机，由 ActionSystem 的 ReactionTiming 枚举指定")]
        [SerializeField] public ActionSystem.ReactionTiming TriggerTiming;

        [Tooltip("是否允许响应由系统/效果生成（IsGenerated==true）的动作，默认 false，即由 Effect/系统生成的动作不触发该条件，可以在具体条件中覆写")]
        [SerializeField] protected bool allowGeneratedActions = false;

        [Tooltip("最多允许的链深（<=0 意味无链深限制），这里只作为条件层面的默认检查，具体 CollectionData 中的 MaxChainDepth 更具优先性")]
        [SerializeField, Min(0)] protected int maxAllowedChainDepth = 0;

        /// <summary>
        /// 订阅接口：将由调用方传入具体 actionType（若为 null 则回退到原有泛型订阅）
        /// </summary>
        public abstract void SubGACondition(Action<GameAction> subAction, Type actionType = null);
        public abstract void UnSubGACondition(Action<GameAction> subAction, Type actionType = null);

        /// <summary>
        /// 默认实现的子条件判断（可被覆写）：
        /// - 空动作返回 false
        /// - 如果动作被标记为 IsGenerated 且 allowGeneratedActions == false 则返回 false
        /// - 如果 maxAllowedChainDepth > 0 且 action.ChainDepth >= maxAllowedChainDepth 则返回 false
        /// - 其它情况返回 true（具体条件逻辑可在派生类中继续叠加判断）
        /// 
        /// 注意：CollectionModel / CollectionData 的 MaxChainDepth 优先于此处的 maxAllowedChainDepth，
        /// 如果你在 CollectionModel 中也做了链深检查（建议这样），这里作为保底判断即可。
        /// </summary>
        public virtual bool SubConditionIsMet(GameAction action)
        {
            if (action == null) return false;

            // 如果不是允许响应由系统/效果生成的动作，则直接忽略
            if (!allowGeneratedActions && action.IsGenerated) return false;

            // 如果在条件层面对链深有限制，则判定动作当前链深是否已达到（或超过）上限
            if (maxAllowedChainDepth > 0)
            {
                // 当 action.ChainDepth 表示已经经过的来源节点数时，
                // 若当前深度 >= 限制，说明不应再被响应（避免进一步传播）
                if (action.ChainDepth >= maxAllowedChainDepth) return false;
            }

            // 通过默认基础检查；派生类可在此基础上追加更细的判断（例如判断 IHasCaster 来源类型等）
            return true;
        }
    }
}