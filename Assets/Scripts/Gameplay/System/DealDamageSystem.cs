using AsakiFramework;
using Gameplay.Controller;
using Gameplay.GA;
using Gameplay.Model;
using System;
using System.Collections;
using UnityEngine;

namespace Gameplay.System
{
    public class DealDamageSystem : AsakiMono
    {
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
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}
