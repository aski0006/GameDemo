using AsakiFramework;
using Gameplay.MVC.Controller;
using Gameplay.System;
using System.Collections.Generic;

namespace Gameplay.Common.Target
{
    public class AllHeriesTarget : TargetMode
    {

        public override List<CombatantBaseController> GetTargets()
        {
            if (AsakiMonoServiceLocator.Instance == null)
            {
                return new List<CombatantBaseController>();
            }
            var heroSystem = AsakiMonoServiceLocator.Instance.GetService<HeroSystem>();
            if (heroSystem == null)
            {
                return new List<CombatantBaseController>();
            }
            return new List<CombatantBaseController>(heroSystem.GetAllHeroControllers());
        }
    }
}
