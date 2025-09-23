using AsakiFramework;
using Gameplay.Anim;
using Gameplay.Controller;
using Gameplay.Creator;
using Gameplay.GA;
using Gameplay.Model;
using Gameplay.View;
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
            foreach (CombatantBaseController target in action.Targets)
            {
                target.TakeDamage(action.DamageAmount);
                var targetView = target.GetView<CombatantViewBase>();
                var go = attackVfxEffect2DAnimCreator.Get(
                    attackVfxEffect2DAnimCreator.prefab,
                    targetView.transform.position,
                    targetView.transform.rotation
                );
                var attackVfxEffect2DAnim = go.GetComponent<AttackVfxEffect2DAnim>();
                attackVfxEffect2DAnim.PlayEffectAnim(()=>{attackVfxEffect2DAnimCreator.Return(go);});
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}
