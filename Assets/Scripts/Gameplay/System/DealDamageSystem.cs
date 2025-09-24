using AsakiFramework;
using Gameplay.Anim;
using Gameplay.Creator;
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
        [Header("造成伤害特效创建器")][SerializeField] private AttackVfxEffect2DAnimCreator attackVfxEffect2DAnimCreator;
        [Header("伤害数字创建器")][SerializeField] private DamageTextCreator damageTextCreator;
        [Header("动画时序设置")]
        [SerializeField] private float vfxDelay = 0.1f;
        [SerializeField] private float damageTextDelay = 0.2f;
        [SerializeField] private float betweenTargetDelay = 0.3f;
        private void Awake()
        {
            HasNotNullComponent(attackVfxEffect2DAnimCreator);
            HasNotNullComponent(damageTextCreator);
        }
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
                if (target == null)
                {
                    LogWarning("DealDamageGA 中包含 null target，已跳过该项。");
                    continue;
                }
                CombatantViewBase targetView = target.GetView<CombatantViewBase>();
                bool isCritical = false; // TODO : 添加暴击逻辑
                if (attackVfxEffect2DAnimCreator != null)
                {
                    yield return StartCoroutine(PlayVfxEffect(targetView, isCritical));
                }
                yield return new WaitForSeconds(vfxDelay);
                try
                {
                    target.TakeDamage(action.DamageAmount);
                }
                catch (Exception ex)
                {
                    LogError($"对目标造成伤害时出错：{ex}");
                }
                // 3. 显示伤害数字
                yield return new WaitForSeconds(damageTextDelay);
                yield return StartCoroutine(ShowDamageText(action, targetView, isCritical));
                // 目标间延迟
                yield return new WaitForSeconds(betweenTargetDelay);
            }
        }

        #region 动画效果

        private IEnumerator PlayVfxEffect(CombatantViewBase targetView, bool isCritical)
        {
            AttackVfxEffect2DAnim attackVfxEffect = attackVfxEffect2DAnimCreator.CreateAttackVfxEffect(
                targetView.transform.position,
                targetView.transform.rotation
            );

            if (!attackVfxEffect)
            {
                LogWarning("无法获取 VFX 实例，跳过该次特效播放。");
                yield break;
            }
            
            attackVfxEffect.PlayEffectAnim(() =>
            {
                attackVfxEffect2DAnimCreator.ReturnAttackVfxEffect(attackVfxEffect);
            });
        }

        private IEnumerator ShowDamageText(DealDamageGA action, CombatantViewBase targetView, bool isCritical)
        {
            DamageTextUI damageTextUI = damageTextCreator.CreateDamageText(
                Mathf.FloorToInt(action.DamageAmount),
                targetView.transform.position + Vector3.up * 1.5f, // 稍微高一点的位置
                targetView.transform.rotation
            );

            if (damageTextUI == null) yield break;

            // 设置伤害文本（包含暴击信息）
            damageTextUI.SetDamageText(Mathf.FloorToInt(action.DamageAmount), isCritical);

            // 重要修改：只传递目标位置，不传递transform引用
            damageTextUI.PlayShowAnimation(targetView.transform);
    
            yield return new WaitForSeconds(0.15f);
    
            // 播放隐藏动画并回收
            damageTextUI.PlayHideAnimation(() =>
            {
                damageTextCreator.ReturnDamageText(damageTextUI);
            });
        }

        #endregion
    }
}
