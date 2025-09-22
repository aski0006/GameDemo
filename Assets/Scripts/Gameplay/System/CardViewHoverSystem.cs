using AsakiFramework;
using Gameplay.View;
using Gameplay.Model;
using UnityEngine;

namespace Gameplay.System
{
    public class CardViewHoverSystem : AsakiMono
    {
        [Header("悬停卡牌视图"), SerializeField] private CardViewer hoverCardView;
        [Header("悬浮卡牌位置偏移"), SerializeField] private Vector3 hoverCardOffset = new Vector3(0, 2, 0);
        [Header("悬停卡牌缩放比例"), SerializeField, Min(0.1f)] private float hoverCardScale = 1.5f;
        public void ShowHoverCardView(Card card, Vector3 position)
        {
            hoverCardView.gameObject.SetActive(true);
            hoverCardView.Setup(card);
            hoverCardView.transform.position = position + hoverCardOffset;
            hoverCardView.transform.localScale = Vector3.one * hoverCardScale;
        }

        public void HideHoverCardView()
        {
            hoverCardView.gameObject.SetActive(false);
        }
    }
}
