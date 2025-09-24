using AsakiFramework;
using Gameplay.Anim;
using Gameplay.MVC.Controller;
using Gameplay.Creator;
using Gameplay.GA;
using Gameplay.MVC.Model;
using Gameplay.MVC.View;
using System;
using System.Collections;
using UnityEngine;

namespace Gameplay.System
{
    public class DealDamageSystem : AsakiMono
    {
        [Header("造成伤害特效创建器"), SerializeField] private AttackVfxEffect2DAnimCreator attackVfxEffect2DAnimCreator;
        
        private void OnEnable()
        {
            ActionSystem.Instance.AttachPerformer<DealDamageGA>(DealDamagePerformer);
        }
        private void OnDisable()
        {
            if(ActionSystem.Instance == null) return;
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

                // 先执行逻辑伤害
                try
                {
                    target.TakeDamage(action.DamageAmount);
                }
                catch (Exception ex)
                {
                    LogError($"对目标造成伤害时出错：{ex}");
                }

                // 再做视图特效（若有视图）
                CombatantViewBase targetView = null;
                try
                {
                    targetView = target.GetView<CombatantViewBase>();
                }
                catch { /* 容错 */ }

                if (targetView == null)
                {
                    LogWarning("目标没有视图，无法播放命中特效，继续下一个目标。");
                    yield return new WaitForSeconds(0.1f);
                    continue;
                }

                if (attackVfxEffect2DAnimCreator == null)
                {
                    LogWarning("attackVfxEffect2DAnimCreator 未设置，跳过 VFX 播放。");
                    yield return new WaitForSeconds(0.1f);
                    continue;
                }

                var go = attackVfxEffect2DAnimCreator.Get(
                    attackVfxEffect2DAnimCreator.prefab,
                    targetView.transform.position,
                    targetView.transform.rotation
                );

                if (go == null)
                {
                    LogWarning("无法获取 VFX 实例，跳过该次特效播放。");
                    yield return new WaitForSeconds(0.1f);
                    continue;
                }

                var attackVfxEffect2DAnim = go.GetComponent<AttackVfxEffect2DAnim>();
                if (attackVfxEffect2DAnim != null)
                {
                    attackVfxEffect2DAnim.PlayEffectAnim(()=>{attackVfxEffect2DAnimCreator.Return(go);});
                }
                else
                {
                    // 若组件缺失，归还资源避免泄露
                    attackVfxEffect2DAnimCreator.Return(go);
                }

                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}