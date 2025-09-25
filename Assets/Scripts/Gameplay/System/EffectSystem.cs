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
            // 生成实际的 effect GameAction（例如 DealDamageGA）
            GameAction gameAction = performEffectGA.Effect.GetGameAction(performEffectGA.Targets, performEffectGA.Caster);
            if (gameAction == null) yield break;

            // 把来源链从 performEffectGA 传给子 action（以便被其他藏品检测）
            gameAction.CopyOriginFrom(performEffectGA);

            // 如果 PerformEffectGA 自带 OriginCollectionId（表示由某个藏品创建），把它追加到子 action 的 origin path
            if (performEffectGA.OriginCollectionId != default)
            {
                gameAction.PushOrigin(performEffectGA.OriginCollectionId);
            }

            // 标记此动作为生成（可用于某些条件判定），但不要把它自动阻断链式传播
            gameAction.IsGenerated = true;

            // 把子 action 加到 PerformEffectGA 的 perform reactions 中
            performEffectGA.AddPerformReaction(gameAction);
            yield return null;
        }
    }
}
