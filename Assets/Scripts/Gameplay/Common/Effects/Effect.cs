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
        // 新增：告知这个 Effect 会产生哪种具体的 GameAction 类型，默认退化为 GameAction
        public virtual Type ActionType => typeof(GameAction);

        public abstract GameAction GetGameAction(List<CombatantBaseController> targets, CombatantBaseController caster = null);
    }
}
