using AsakiFramework;
using Gameplay.MVC.Controller;
using Gameplay.MVC.Model;
using Gameplay.System;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gameplay.Common.Target
{
    [Serializable]
    public class TopNEnemiesByHpTarget : TargetMode
    {
        [SerializeField, Min(1)] private int count = 1;

        public override List<CombatantBaseController> GetTargets()
        {
            var res = new List<CombatantBaseController>();
            if (AsakiMonoServiceLocator.Instance == null) return res;
            var enemySystem = AsakiMonoServiceLocator.Instance.GetService<EnemySystem>();
            if (enemySystem == null) return res;

            var list = enemySystem.GetAllEnemyControllers();
            if (list == null || list.Count == 0) return res;

            var ordered = list
                .Where(c => c != null)
                .OrderByDescending(c =>
                {
                    var m = c.GetModel<CombatantModel>();
                    return m != null ? m.CurrentHp : float.MinValue;
                })
                .Take(Mathf.Clamp(count, 1, list.Count))
                .ToList();

            res.AddRange(ordered);
            return res;
        }
    }
}
