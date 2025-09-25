using Gameplay.MVC.Controller;
using Gameplay.GA;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Effects
{
    public class AttackEnemyEffect : Effect
    {
        [Header("伤害值"), SerializeField] private float damage = 10;

        // 接受并把 caster 传入 InjuryHasSourceGA
        public override GameAction GetGameAction(List<CombatantBaseController> targets, CombatantBaseController caster = null)
        {
            return new InjuryHasSourceGA(damage, caster, targets);
        }
    }
}
