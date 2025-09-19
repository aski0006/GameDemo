using AsakiFramework;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

namespace Gameplay.View
{
    public class HandViewer : AsakiMono
    {
        [Header("曲线采样容器"), SerializeField] private SplineContainer splineContainer;
        [Header("手牌最大容量"), SerializeField, Range(1, 10)] private int handViewMaxCapacity = 10;
        [Space]
        [Header("卡牌移动动画参数"), SerializeField] private float cardMoveDuration = 0.25f;
        [Header("卡牌旋转动画参数"), SerializeField] private float cardRotateDuration = 0.25f;
        private readonly List<CardViewer> cards = new();

        public IEnumerator AddCardViewToHandView(CardViewer cardView)
        {
            cards.Add(cardView);
            yield return UpdateCardViewPosition(0.15f);
        }

        private IEnumerator UpdateCardViewPosition(float duration)
        {
            if (cards.Count == 0) yield break; // 没有卡牌，直接返回
            float cardSpacing = 1f / (handViewMaxCapacity); // 不可为 0 
            float firstCardPosition = 0.5f - (cards.Count - 1) * cardSpacing / 2f;
            Spline spline = splineContainer.Spline;
            for (int i = 0; i < cards.Count; i++)
            {
                float p = firstCardPosition + i * cardSpacing;
                Vector3 splinePosition = spline.EvaluatePosition(p);
                Vector3 forward = spline.EvaluateTangent(p);
                Vector3 up = spline.EvaluateUpVector(p);
                Quaternion rotation = Quaternion.LookRotation(
                    -up,
                    Vector3.Cross(-up, forward).normalized
                );
                cards[i].transform.DOMove(
                    splinePosition + transform.position + 0.01f * i * Vector3.back,
                    cardMoveDuration);
                cards[i].transform.DORotate(rotation.eulerAngles, cardRotateDuration);
                yield return new WaitForSeconds(duration);
            }
        }
    }
}
