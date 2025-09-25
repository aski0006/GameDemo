using Gameplay.Common.Target;
using Gameplay.MVC.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.MVC.Interfaces
{
    public interface ICollectionReadOnly
    {
        Sprite CollectionSprite { get; }
        CollectionTriggerCondition TriggerCondition { get; }
        AutoTargetEffect CollectionEffect { get; }
        bool UseAutoTarget { get; }
        bool UseActionCasterAsTarget { get; }
    }
}
