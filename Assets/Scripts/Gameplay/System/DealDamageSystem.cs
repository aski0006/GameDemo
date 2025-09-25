// DealDamageSystem.cs (简化版)

using AsakiFramework;
using Gameplay.Events;
using Gameplay.GA;
using Gameplay.MVC.Controller;
using Gameplay.MVC.View;
using Gameplay.UI;
using System;
using System.Collections;
using UnityEngine;

namespace Gameplay.System
{
    public class DealDamageSystem : AsakiMono
    {
        [Header("动画时序设置")]
        [SerializeField] private float betweenTargetDelay = 0.3f;

        private void OnEnable()
        {
            ActionSystem.Instance.AttachPerformer<InjuryWithoutSourceGA>(InjuryWithoutSourcePerformer);
            ActionSystem.Instance.AttachPerformer<InjuryHasSourceGA>(InjuryHasSourcePerformer);
        }

        private void OnDisable()
        {
            if (ActionSystem.Instance == null) return;
            ActionSystem.Instance.DetachPerformer<InjuryWithoutSourceGA>();
            ActionSystem.Instance.DetachPerformer<InjuryHasSourceGA>();
        }

        private IEnumerator InjuryWithoutSourcePerformer(InjuryWithoutSourceGA action)
        {
            if (action == null || action.Targets == null)
            {
                LogWarning("InjuryWithoutSourceGA 或 Targets 为 null，跳过造成伤害流程。");
                yield break;
            }

            foreach (CombatantBaseController target in action.Targets)
            {
                if (target == null) continue;

                target.TakeDamage(action.DamageAmount);
                yield return new WaitForSeconds(betweenTargetDelay);
            }
        }

        private IEnumerator InjuryHasSourcePerformer(InjuryHasSourceGA action)
        {
            if (action == null || action.Targets == null)
            {
                LogWarning("InjuryHasSourceGA 或 Targets 为 null，跳过造成伤害流程。");
                yield break;
            }
            foreach (CombatantBaseController target in action.Targets)
            {
                if (target == null) continue;
                target.TakeDamage(action.DamageAmount);
                yield return new WaitForSeconds(betweenTargetDelay);
            }
        }
    }
}
