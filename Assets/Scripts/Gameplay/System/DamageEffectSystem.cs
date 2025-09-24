// DamageEffectSystem.cs

using AsakiFramework;
using Gameplay.Creator;
using Gameplay.Events;
using Gameplay.MVC.View;
using UnityEngine;

namespace Gameplay.System
{
    public class DamageEffectSystem : AsakiMono
    {
        [Header("特效创建器")]
        [SerializeField] private AttackVfxEffect2DAnimCreator attackVfxEffect2DAnimCreator;
        [SerializeField] private DamageTextCreator damageTextCreator;

        [Header("动画时序设置")]
        [SerializeField] private float vfxDelay = 0.1f;
        [SerializeField] private float damageTextDelay = 0.2f;

        private void OnEnable()
        {
            // 订阅伤害相关事件
            EventBus.Instance.Subscribe<DamageVfxEvent>(OnDamageVfxEvent);
            EventBus.Instance.Subscribe<DamageTextEvent>(OnDamageTextEvent);
        }

        private void OnDisable()
        {
            if (EventBus.Instance == null) return;
            EventBus.Instance.Unsubscribe<DamageVfxEvent>(OnDamageVfxEvent);
            EventBus.Instance.Unsubscribe<DamageTextEvent>(OnDamageTextEvent);
        }

        private void OnDamageVfxEvent(DamageVfxEvent e)
        {
            // 播放命中特效
            if (attackVfxEffect2DAnimCreator != null && e.TargetView != null)
            {
                PlayVfxEffect(e.TargetView, e.IsCritical);
            }
        }

        private void OnDamageTextEvent(DamageTextEvent e)
        {
            // 显示伤害数字
            if (damageTextCreator != null)
            {
                ShowDamageText(e.WorldPosition, e.DamageAmount, e.IsCritical);
            }
        }

        private void PlayVfxEffect(CombatantViewBase targetView, bool isCritical)
        {
            var attackVfxEffect = attackVfxEffect2DAnimCreator.CreateAttackVfxEffect(
                targetView.transform.position,
                targetView.transform.rotation
            );

            if (!attackVfxEffect) return;

            // 暴击特效调整
            if (isCritical)
            {
                attackVfxEffect.transform.localScale = Vector3.one * 1.5f;
            }

            attackVfxEffect.PlayEffectAnim(() =>
            {
                attackVfxEffect2DAnimCreator.ReturnAttackVfxEffect(attackVfxEffect);
            });
        }

        // DamageEffectSystem.cs 中的修改
        private void ShowDamageText(Vector3 worldPosition, int damage, bool isCritical)
        {
            var damageTextUI = damageTextCreator.CreateDamageText(
                damage,
                worldPosition,
                Quaternion.identity
            );

            if (damageTextUI == null) return;

            damageTextUI.SetDamageText(damage, isCritical);
            damageTextUI.PlayShowAnimation(worldPosition, () =>
            {
                damageTextCreator.ReturnDamageText(damageTextUI);
            });
        }
    }
}
