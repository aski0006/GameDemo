using AsakiFramework;
using Gameplay.MVC.Controller;
using Gameplay.System;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace Gameplay.Common.Target
{
    [Serializable]
    public class RandomNHeroTarget : TargetMode
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
            var indices = new List<int>(list.Count);
            for (int i = 0; i < list.Count; i++) indices.Add(i);

            for (int i = 0; i < take; i++)
            {
                int idx = UnityEngine.Random.Range(0, indices.Count);
                int sel = indices[idx];
                indices.RemoveAt(idx);
                res.Add(list[sel]);
            }
            return res;
        }
    }
}
