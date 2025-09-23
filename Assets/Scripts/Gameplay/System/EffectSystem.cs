using AsakiFramework;
using Gameplay.GA;
using System;
using System.Collections;

namespace Gameplay.System
{
    public class EffectSystem : AsakiMono
    {
        private void OnEnable()
        {
            ActionSystem.Instance.AttachPerformer<PerformEffectGA>(PerformEffectPerformer);
        }

        private void OnDisable()
        {
            ActionSystem.Instance?.DetachPerformer<PerformEffectGA>();
        }

        private IEnumerator PerformEffectPerformer(PerformEffectGA performEffectGA)
        {
            // 执行效果
            GameAction gameAction = performEffectGA.Effect.GetGameAction(performEffectGA.Targets);
            if (gameAction == null) yield break;
            performEffectGA.AddPerformReaction(gameAction);
            yield return null;
        }
    }
}
