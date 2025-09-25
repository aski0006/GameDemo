using Gameplay.MVC.Controller;
using System.Collections.Generic;

namespace Gameplay.GA
{
    public class InjuryWithoutSourceGA : GameAction
    {
        public float DamageAmount { get; set; }
        public List<CombatantBaseController> Targets { get; private set; }

        public InjuryWithoutSourceGA(float damageAmount, List<CombatantBaseController> targets)
        {
            DamageAmount = damageAmount;
            Targets = new(targets);
        }
        
        
        
    }
}
