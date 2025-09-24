using Gameplay.MVC.Controller;
using Gameplay.MVC.Model;

namespace Gameplay.GA
{
    public class PlayCardGA : GameAction
    {
        public CardModel CardModel { get; set; }
        public EnemyCharacterController ManualTargetEnemy { get; set; }
        public PlayCardGA(CardModel cardModel)
        {
            // 牌不取目标
            CardModel = cardModel;
            ManualTargetEnemy = null;
        }
        public PlayCardGA(CardModel cardModel, EnemyCharacterController manualTargetEnemy)
        {
            // 技能牌有目标
            CardModel = cardModel;
            ManualTargetEnemy = manualTargetEnemy;
        }
    }
}
