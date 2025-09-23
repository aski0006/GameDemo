using Gameplay.Controller;
using Gameplay.Effects;
using System.Collections.Generic;

namespace Gameplay.GA
{
    public class PerformEffectGA : GameAction
    {
        public Effect Effect { get; private set; }
        public List<CombatantBaseController> Targets { get; private set; }
        public PerformEffectGA(Effect effect, List<CombatantBaseController> targets)
        {
            Effect = effect;
            Targets = new(targets);
        }
    }
}
