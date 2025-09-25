using Gameplay.MVC.Controller;
using System;
using System.Collections.Generic;

namespace Gameplay.Effects
{
    /// <summary>
    /// 抽象效果类
    /// </summary>
    [Serializable]
    public abstract class Effect
    {
        public abstract GameAction GetGameAction(List<CombatantBaseController> targets);
    }
}
