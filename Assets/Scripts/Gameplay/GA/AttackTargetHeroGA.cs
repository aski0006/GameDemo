using Gameplay.Controller;
using Gameplay.View;

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
