using AsakiFramework;
using Gameplay.Data;
using System;

namespace Gameplay.Collections
{
    public class OnEnemyAttackCollectionCondition : CollectionCondition
    {

        public override void SubscribeCondition(Action<GameAction> reaction)
        {
            ActionSystem.Instance.SubscribeReaction(reaction, reactionTiming);
        }
        public override void UnsubscribeCondition(Action<GameAction> reaction)
        {
            ActionSystem.Instance.UnsubscribeReaction(reaction, reactionTiming);
        }
        public override bool SubConditionMet()
        {
            return true; // 无条件反击
        }
        
    }
}
