using AsakiFramework;
using Gameplay.GA;

namespace Gameplay.UI
{
    public class TurnEndButtonUI : AsakiMono
    {
        public void OnClick()
        {
            EnemyTurnGA enemyTurnGA = new EnemyTurnGA();
            ActionSystem.Instance.PerformGameAction(enemyTurnGA);
        }
        
    }
}
