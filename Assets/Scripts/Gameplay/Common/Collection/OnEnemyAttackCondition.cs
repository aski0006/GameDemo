using AsakiFramework;
using Gameplay.Interfaces;
using Gameplay.MVC.Controller;
using Gameplay.MVC.Data;
using System;
using UnityEngine;

namespace Gameplay.Common.Collection
{
    public class OnEnemyAttackCondition : CollectionTriggerCondition
    {
        public override void SubGACondition(Action<GameAction> subAction, Type actionType = null)
        {
            if (actionType != null)
            {
                ActionSystem.Instance.SubscribeReaction(actionType, subAction, TriggerTiming);
            }
            else
            {
                Debug.LogWarning("OnEnemyAttackCondition: 泛型订阅已弃用，请使用具体类型订阅");
            }
        }

        public override void UnSubGACondition(Action<GameAction> subAction, Type actionType = null)
        {
            if (actionType != null)
            {
                ActionSystem.Instance.UnsubscribeReaction(actionType, subAction, TriggerTiming);
            }
            else
            {
                Debug.LogWarning("OnEnemyAttackCondition: 泛型订阅已弃用，请使用具体类型订阅");
            }
        }

        public override bool SubConditionIsMet(GameAction action)
        {
            // 先让基类做通用过滤（IsGenerated / 链深等）
            if (!base.SubConditionIsMet(action))
            {
                return false;
            }

            if (action is IHasCaster hasCaster)
            {
                if (hasCaster.Caster is EnemyCharacterController) return true;
            }

            return false;
        }
    }
}
