using Gameplay.Interfaces;
using Gameplay.MVC.Controller;
using System.Collections.Generic;

namespace Gameplay.GA
{
    public class DealDamageGA : GameAction, IHasCaster
    {
        public float DamageAmount { get; set; }
        public List<CombatantBaseController> Targets { get; private set; }

        public DealDamageGA(float damageAmount, List<CombatantBaseController> targets, CombatantBaseController caster)
        {
            DamageAmount = damageAmount;
            Targets = new(targets);
            Caster = caster;
        }
        public CombatantBaseController Caster { get; set; }
    }
}
