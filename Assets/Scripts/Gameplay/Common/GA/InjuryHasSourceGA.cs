using Gameplay.Interfaces;
using Gameplay.MVC.Controller;
using System.Collections.Generic;

namespace Gameplay.GA
{
    public class InjuryHasSourceGA : GameAction, IHasCaster
    {
        public float DamageAmount { get; set; }
        public CombatantBaseController Caster { get; set; }
        public List<CombatantBaseController> Targets { get; set; }

        public InjuryHasSourceGA(float damageAmount, CombatantBaseController caster, List<CombatantBaseController> targets)
        {
            DamageAmount = damageAmount;
            Caster = caster;
            Targets = targets;
        }
    }
}
