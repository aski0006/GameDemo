using Gameplay.MVC.Controller;
using Gameplay.GA;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Effects
{
    public class AttackEnemyEffect : Effect
    {
        [Header("伤害值"), SerializeField] private float demage = 10;

        public override GameAction GetGameAction(List<CombatantBaseController> targets)
        {
            return new DealDamageGA(demage, targets);
        }
    }
}
