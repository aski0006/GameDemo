using Gameplay.Effects;
using SerializeReferenceEditor;
using System;
using UnityEngine;

namespace Gameplay.Common.Target
{
    [Serializable]
    public class AutoTargetEffect
    {
        [field: SerializeReference, SR] public TargetMode TargetMode { get; private set; }
        [field: SerializeReference, SR] public Effect Effect { get; private set; }
    }
}
