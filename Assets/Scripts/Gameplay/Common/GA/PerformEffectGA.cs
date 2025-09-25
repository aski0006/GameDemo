using Gameplay.MVC.Controller;
using Gameplay.Effects;
using Gameplay.Interfaces;
using System.Collections.Generic;
using UnityEditor;

namespace Gameplay.GA
{
    public class PerformEffectGA : GameAction, IHasCaster
    {
        public Effect Effect { get; private set; }
        public List<CombatantBaseController> Targets { get; private set; }
        public CombatantBaseController Caster { get; private set; }

        // 新增：可携带创建此 PerformEffectGA 的藏品 GUID（由 CollectionModel 填）
        public GUID OriginCollectionId { get; private set; } = default;

        public PerformEffectGA(Effect effect, List<CombatantBaseController> targets, CombatantBaseController caster = null, GUID originCollectionId = default)
        {
            Effect = effect;
            Targets = targets != null ? new List<CombatantBaseController>(targets) : new List<CombatantBaseController>();
            Caster = caster;
            OriginCollectionId = originCollectionId;
        }
    }
}
