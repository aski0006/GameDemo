using Gameplay.Interfaces;
using Gameplay.MVC.Controller;
using Gameplay.MVC.Interfaces;
using Gameplay.UI;

namespace Gameplay.GA
{
    public class AttackTargetHeroGA : GameAction,IHasCaster
    {
        public EnemyCharacterController Attacker { get; set; }
        public CombatantBaseController Caster { get; set; }
        
        public AttackTargetHeroGA(EnemyCharacterController attacker)
        {
            Attacker = attacker;
            Caster = attacker;
        }


    }
}
