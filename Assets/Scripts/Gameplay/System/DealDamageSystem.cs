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
            ActionSystem.Instance.AttachPerformer<DealDamageGA>(DealDamagePerformer);
        }

        private void OnDisable()
        {
            if (ActionSystem.Instance == null) return;
            ActionSystem.Instance.DetachPerformer<DealDamageGA>();
        }

        private IEnumerator DealDamagePerformer(DealDamageGA action)
        {
            if (action == null || action.Targets == null)
            {
                LogWarning("DealDamageGA 或 Targets 为 null，跳过造成伤害流程。");
                yield break;
            }

            foreach (CombatantBaseController target in action.Targets)
            {
                if (target == null) continue;

                CombatantViewBase targetView = target.GetView<CombatantViewBase>();
                try
                {
                    target.TakeDamage(action.DamageAmount);
                }
                catch (Exception ex)
                {
                    LogError($"对目标造成伤害时出错：{ex}");
                }

                yield return new WaitForSeconds(betweenTargetDelay);
            }
        }
    }
}