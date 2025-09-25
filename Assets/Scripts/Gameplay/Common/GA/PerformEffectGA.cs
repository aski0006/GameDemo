using Gameplay.Effects;
using Gameplay.MVC.Controller;
using System.Collections.Generic;

namespace Gameplay.GA
{
    // 如果仓库已有 PerformEffectGA，请把下面的字段合并进去（保证包含 Caster）
    public class PerformEffectGA : GameAction
    {
        public Effect Effect { get; }
        public List<CombatantBaseController> Targets { get; }
        public CombatantBaseController Caster { get; }

        public PerformEffectGA(Effect effect, List<CombatantBaseController> targets, CombatantBaseController caster = null)
        {
            Effect = effect;
            Targets = targets ?? new List<CombatantBaseController>();
            Caster = caster;
        }
    }
}
