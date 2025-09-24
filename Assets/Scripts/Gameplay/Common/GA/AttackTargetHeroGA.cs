using Gameplay.MVC.Controller;
using Gameplay.MVC.Interfaces;
using Gameplay.UI;

namespace Gameplay.GA
{
    public class AttackTargetHeroGA : GameAction
    {
        public EnemyCharacterController Attacker { get; set; }


        public AttackTargetHeroGA(EnemyCharacterController attacker)
        {
            Attacker = attacker;
        }


    }
}
