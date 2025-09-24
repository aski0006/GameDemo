using AsakiFramework;
using DG.Tweening;
using Extensions;
using Gameplay.Common.Target;
using Gameplay.Creator;
using Gameplay.Data;
using Gameplay.GA;
using Gameplay.MVC.Controller;
using Gameplay.MVC.Model;
using Gameplay.UI;
using Gameplay.View;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.System
{
    public class CardSystem : AsakiMono
    {
        [Header("卡牌视图创造器")][SerializeField] private CardViewCreator cardViewCreator;
        [Header("手牌视图")][SerializeField] private HandViewer handViewer;
        [Space]
        [Header("卡牌牌堆挂载点")]
        [SerializeField] private Transform drawPileMountPoint; // 抽牌堆挂
        [Header("卡牌手牌挂载点")]
        [SerializeField] private Transform handMountPoint; // 手牌挂载点
        [Header("卡牌弃牌堆挂载点")]
        [SerializeField] private Transform discardPileMountPoint; // 弃牌堆挂
        private readonly List<Card> discardPile = new List<Card>(); // 弃牌堆

        private readonly List<Card> drawPile = new List<Card>(); // 抽牌堆
        private readonly List<Card> hand = new List<Card>(); // 手牌
        private void OnEnable()
        {
            ActionSystem.Instance.AttachPerformer<DrawCardGA>(DrawCardPerformer);
            ActionSystem.Instance.AttachPerformer<DiscardAllCardGA>(DiscardAllCardPerformer);
            ActionSystem.Instance.AttachPerformer<PlayCardGA>(PlayCardPerformer);
            ActionSystem.Instance.SubscribePre<EnemyTurnGA>(EnemyTurnPreReaction);
            ActionSystem.Instance.SubscribePost<EnemyTurnGA>(EnemyTurnPostReaction);
        }
        private void OnDisable()
        {
            if (ActionSystem.Instance == null) return;
            ActionSystem.Instance.DetachPerformer<DrawCardGA>();
            ActionSystem.Instance.DetachPerformer<DiscardAllCardGA>();
            ActionSystem.Instance.DetachPerformer<PlayCardGA>();
            ActionSystem.Instance.UnsubscribePre<EnemyTurnGA>(EnemyTurnPreReaction);
            ActionSystem.Instance.UnsubscribePost<EnemyTurnGA>(EnemyTurnPostReaction);
        }

        public void Setup(List<CardData> cards)
        {
            foreach (CardData cardData in cards)
            {
                drawPile.Add(new Card(cardData));
            }
        }

        private IEnumerator DrawCard()
        {
            if (drawPile.Count == 0)
            {
                if (discardPile.Count == 0)
                {
                    yield break;
                }
                RefillDeck();
            }

            Card card = drawPile.DrawRandomElement();
            hand.Add(card);
            RunNextFrame(() =>
            {
                CardViewer view = cardViewCreator.CreateCardView(
                    card,
                    drawPileMountPoint.position,
                    drawPileMountPoint.rotation);
                handViewer.AddCardViewToHandView(view);
            });
            yield return null;
        }

        private IEnumerator DiscardCard(Card card)
        {
            CardViewer view = handViewer.GetCardViewByCard(card);
            if (view == null) yield break;

            view.transform.DOScale(Vector3.zero, 0.15f);
            Tween tween = view.transform.DOMove(discardPileMountPoint.position, 0.15f);
            yield return tween.WaitForCompletion();

            cardViewCreator.ReturnCardView(view);
            handViewer.RemoveCardViewFromHandView(view);
        }

        private void RefillDeck()
        {
            drawPile.AddRange(discardPile);
            discardPile.Clear();
            drawPile.Shuffle();
        }

        #region 执行方法

        private IEnumerator DrawCardPerformer(DrawCardGA drawCardGa)
        {
            int actualDrawAmount = Mathf.Min(drawCardGa.DrawAmount, drawPile.Count);
            int noneDrawAmount = drawCardGa.DrawAmount - actualDrawAmount;
            for (int i = 0; i < actualDrawAmount; i++) yield return DrawCard();
            if (noneDrawAmount > 0)
            {
                RefillDeck(); // 重置牌堆
                for (int i = 0; i < noneDrawAmount; i++) yield return DrawCard();
            }
            yield return null;
        }
        private IEnumerator DiscardAllCardPerformer(DiscardAllCardGA discardAllCardGa)
        {
            var handCopy = new List<Card>(hand);
            foreach (Card card in handCopy)
            {
                hand.Remove(card); // ✅ 先删数据
                discardPile.Add(card); // ✅ 再进弃牌堆
                yield return DiscardCard(card); // 只处理视图
            }
        }
        private IEnumerator PlayCardPerformer(PlayCardGA playCardGa)
        {
            if (playCardGa == null || playCardGa.Card == null) yield break;
            if (!hand.Contains(playCardGa.Card)) yield break;
            Card card = playCardGa.Card;
            hand.Remove(card); // ✅ 先移出手牌
            discardPile.Add(card); // ✅ 立即进弃牌堆
            yield return DiscardCard(card); // 视图动画
            UsageCostGA usageCostGa = new UsageCostGA(card.cardCost);
            ActionSystem.Instance.PerformGameAction(usageCostGa); // 扣除费用

            if (card.manualTargetEffect != null)
            {
                PerformEffectGA performEffectGA = new PerformEffectGA(card.manualTargetEffect, new List<CombatantBaseController> { playCardGa.ManualTargetEnemy });
                playCardGa.AddPerformReaction(performEffectGA);
            }
            foreach (AutoTargetEffect effectWrapper in card.autoTargetEffects)
            {
                var targetList = effectWrapper.TargetMode.GetTargets();
                PerformEffectGA performEffectGa = new PerformEffectGA(effectWrapper.Effect, targetList);
                playCardGa.AddPerformReaction(performEffectGa);
            }
        }

        #endregion

        #region 响应方法

        private void EnemyTurnPreReaction(EnemyTurnGA enemyTurnGa)
        {
            enemyTurnGa.AddPreReaction(new DiscardAllCardGA());
        }

        private void EnemyTurnPostReaction(EnemyTurnGA enemyTurnGa)
        {
            enemyTurnGa.AddPostReaction(new DrawCardGA(5));
        }

        #endregion
    }
}
