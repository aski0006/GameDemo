using AsakiFramework;
using Gameplay.MVC.Controller;
using Gameplay.System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Common.Target
{
    public class RandomHeroTarget : TargetMode
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
            var list = heroSystem.GetAllHeroControllers();
            if (list.Count == 0) return new List<CombatantBaseController>();
            return new List<CombatantBaseController>(new[] { list[Random.Range(0, list.Count)] });
        }
    }
}
