using AsakiFramework;
using Gameplay.GA;
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
            // 将 caster 传给 Effect，以便返回的 GameAction（若实现了 IHasCaster）能携带施法者信息
            GameAction gameAction = performEffectGA.Effect.GetGameAction(performEffectGA.Targets, performEffectGA.Caster);
            if (gameAction == null) yield break;
            performEffectGA.AddPerformReaction(gameAction);
            yield return null;
        }
    }
}
