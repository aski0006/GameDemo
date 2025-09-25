using AsakiFramework;
using Gameplay.MVC.Controller;
using Gameplay.System;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Common.Target
{
    [Serializable]
    public class FirstNHeroesTarget : TargetMode
    {
        [SerializeField, Min(1)] private int count = 1;

        public override List<CombatantBaseController> GetTargets()
        {
            var res = new List<CombatantBaseController>();
            if (AsakiMonoServiceLocator.Instance == null) return res;
            var heroSystem = AsakiMonoServiceLocator.Instance.GetService<HeroSystem>();
            if (heroSystem == null) return res;

            var list = heroSystem.GetAllHeroControllers();
            if (list == null || list.Count == 0) return res;

            int take = Mathf.Clamp(count, 1, list.Count);
            for (int i = 0; i < take; i++) res.Add(list[i]);
            return res;
        }
    }
}
