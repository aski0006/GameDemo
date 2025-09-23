using AsakiFramework;
using Gameplay.Controller;
using Gameplay.GA;
using Gameplay.System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Effects
{
    public class AttackAllEnemyEffect : Effect
    {
        [Header("伤害值"), SerializeField] private float demage = 10;

        public override GameAction GetGameAction()
        {
            var enemySystem = AsakiMonoServiceLocator.Instance?.GetService<EnemySystem>();
            if (enemySystem == null) return null;
            var enemies = enemySystem.GetAllEnemyControllers();
            List<CombatantBaseController> enemiesList = new List<CombatantBaseController>(enemies);
            return new DealDamageGA(demage, enemiesList);
        }
    }
}
