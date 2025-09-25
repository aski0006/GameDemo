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
        // 将 caster 作为可选参数传入，子类可以决定是否使用它
        public abstract GameAction GetGameAction(List<CombatantBaseController> targets, CombatantBaseController caster = null);
    }
}
