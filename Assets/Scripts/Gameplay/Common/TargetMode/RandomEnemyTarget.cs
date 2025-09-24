using AsakiFramework;
using Gameplay.MVC.Controller;
using Gameplay.System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Common.Target
{
    public class RandomEnemyTarget : TargetMode
    {

        public override List<CombatantBaseController> GetTargets()
        {
            if (AsakiMonoServiceLocator.Instance == null)
            {
                return new List<CombatantBaseController>();
            }
            var enemySystem = AsakiMonoServiceLocator.Instance.GetService<EnemySystem>();
            if (enemySystem == null)
            {
                return new List<CombatantBaseController>();
            }
            var list = enemySystem.GetAllEnemyControllers();
            if (list.Count == 0) return new List<CombatantBaseController>();
            return new List<CombatantBaseController>(new[] { list[Random.Range(0, list.Count)] });
        }
    }
}
