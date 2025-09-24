using Gameplay.MVC.Controller;
using Gameplay.MVC.Model;

namespace Gameplay.GA
{
    public class PlayCardGA : GameAction
    {
        public Card Card { get; set; }
        public EnemyCharacterController ManualTargetEnemy { get; set; }
        public PlayCardGA(Card card)
        {
            // 牌不取目标
            Card = card;
            ManualTargetEnemy = null;
        }
        public PlayCardGA(Card card, EnemyCharacterController manualTargetEnemy)
        {
            // 技能牌有目标
            Card = card;
            ManualTargetEnemy = manualTargetEnemy;
        }
    }
}
