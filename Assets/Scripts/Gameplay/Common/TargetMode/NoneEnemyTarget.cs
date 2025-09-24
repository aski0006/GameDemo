using Gameplay.MVC.Controller;
using System.Collections.Generic;

namespace Gameplay.Common.Target
{
    public class NoneEnemyTarget : TargetMode
    {

        public override List<CombatantBaseController> GetTargets()
        {
            return new List<CombatantBaseController>(); // 返回空列表，表示没有目标
        }
    }
}
