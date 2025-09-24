using Gameplay.MVC.Controller;
using System;
using System.Collections.Generic;

namespace Gameplay.Common.Target
{
    [Serializable]
    public abstract class TargetMode
    {
        public abstract List<CombatantBaseController> GetTargets();
    }
}
