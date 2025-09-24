using AsakiFramework;
using Gameplay.MVC.Controller;
using Gameplay.System;
using System.Collections.Generic;

namespace Gameplay.Common.Target
{
    public class AllEnemiesTarget : TargetMode
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
            return new List<CombatantBaseController>(enemySystem.GetAllEnemyControllers());

        }
    }
}
