using AsakiFramework;
using AsakiFramework.ObjectPool;
using Data;
using DG.Tweening;
using Extensions;
using Gameplay.Creator;
using Gameplay.GA;
using Gameplay.View;
using Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.System
{
    public class CardSystem : AsakiMono
    {
        [Header("卡牌视图创造器"), SerializeField] private CardViewCreator cardViewCreator;
        [Header("手牌视图"), SerializeField] private HandViewer handViewer;
        [Space]
        [Header("卡牌牌堆挂载点")]
        [SerializeField] private Transform drawPileMountPoint; // 抽牌堆挂
        [Header("卡牌手牌挂载点")]
        [SerializeField] private Transform handMountPoint; // 手牌挂载点
        [Header("卡牌弃牌堆挂载点")]
        [SerializeField] private Transform discardPileMountPoint; // 弃牌堆挂

        private readonly List<Card> drawPile = new(); // 抽牌堆
        private readonly List<Card> hand = new(); // 手牌
        private readonly List<Card> discardPile = new(); // 弃牌堆

        private void OnEnable()
        {
            ActionSystem.Instance.AttachPerformer<DrawCardGA>(DrawCardPerformer);
            ActionSystem.Instance.AttachPerformer<DiscardAllCardGA>(DiscardAllCardPerformer);

            ActionSystem.Instance.SubscribePre<EnemyTurnGA>(EnemyTurnPreReaction);
            ActionSystem.Instance.SubscribePost<EnemyTurnGA>(EnemyTurnPostReaction);
        }

        private void OnDisable()
        {
            if (ActionSystem.Instance == null) return;
            ActionSystem.Instance.DetachPerformer<DrawCardGA>();
            ActionSystem.Instance.DetachPerformer<DiscardAllCardGA>();

            ActionSystem.Instance.UnsubscribePre<EnemyTurnGA>(EnemyTurnPreReaction);
            ActionSystem.Instance.UnsubscribePost<EnemyTurnGA>(EnemyTurnPostReaction);
        }

        public void Setup(List<CardData> cardDatas)
        {
            foreach (var cardData in cardDatas)
            {
                drawPile.Add(new Card(cardData));
            }
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
            foreach (var card in hand)
            {
                discardPile.Add(card);
                CardViewer view = handViewer.GetCardViewByCard(card);
                yield return DiscardCard(view);
            }

        }

        #endregion

        #region 响应方法

        private void EnemyTurnPreReaction(EnemyTurnGA enemyTurnGa) =>
            enemyTurnGa.AddPreReaction(new DiscardAllCardGA());

        private void EnemyTurnPostReaction(EnemyTurnGA enemyTurnGa) =>
            enemyTurnGa.AddPostReaction(new DrawCardGA(5));

        #endregion

        private IEnumerator DrawCard()
        {
            var card = drawPile.DrawRandomElement();
            hand.Add(card);
            var view = cardViewCreator.CreateCardView(
                card,
                drawPileMountPoint.position,
                drawPileMountPoint.rotation);
           handViewer.AddCardViewToHandView(view);
            yield return null;
        }

        private IEnumerator DiscardCard(CardViewer view)
        {

            if (view == null) yield break;
            view.transform.DOScale(Vector3.zero, 0.15f);
            Tween tween = view.transform.DOMove(discardPileMountPoint.position, 0.15f);
            yield return tween.WaitForCompletion();
            ObjectPool.Return(view.gameObject);
            handViewer.RemoveCardViewFromHandView(view);
            yield return null;
        }

        private void RefillDeck()
        {
            drawPile.AddRange(discardPile);
            discardPile.Clear();
            drawPile.Shuffle();
        }

    }
}
