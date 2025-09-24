using AsakiFramework;
using Gameplay.MVC.Controller;
using Gameplay.System;
using System.Collections.Generic;

namespace Gameplay.Common.Target
{
    public class AllCombatantTarget : TargetMode
    {

        public override List<CombatantBaseController> GetTargets()
        {
            List<CombatantBaseController> targets = new List<CombatantBaseController>();
            
            var asakiServiceLocator = AsakiMonoServiceLocator.Instance;
            if (asakiServiceLocator == null)
            {
                return targets;
            }
            var enemySystem = asakiServiceLocator.GetService<EnemySystem>();
            if (enemySystem != null)
            {
                targets.AddRange(enemySystem.GetAllEnemyControllers());
            }
            var heroSystem = asakiServiceLocator.GetService<HeroSystem>();
            if (heroSystem != null)
            {
                targets.AddRange(heroSystem.GetAllHeroControllers());
            }
            return targets;
        }
    }
}
