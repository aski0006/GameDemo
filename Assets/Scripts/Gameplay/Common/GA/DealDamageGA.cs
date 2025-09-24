using Gameplay.MVC.Controller;
using Gameplay.MVC.Interfaces;
using System.Collections.Generic;

namespace Gameplay.GA
{
    public class DealDamageGA : GameAction
    {
        public float DamageAmount { get; set; }
        public List<CombatantBaseController> Targets { get; private set; }

        public DealDamageGA(float damageAmount, List<CombatantBaseController> targets)
        {
            DamageAmount = damageAmount;
            Targets = new(targets);
        }
    }
}
