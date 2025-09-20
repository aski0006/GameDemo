using AsakiFramework;
using AsakiFramework.ObjectPool;
using Gameplay.System;
using Model;
using System;
using TMPro;
using UnityEngine;

namespace Gameplay.View
{
    public class CardViewer : AsakiMono
    {
        [Header("卡牌包装器"), SerializeField] private GameObject wrapper;
        [Space]
        [Header("卡牌精灵渲染器"), SerializeField] private SpriteRenderer cardRenderer;
        [Space]
        [Header("卡牌文本组件列表")]
        [Header("卡牌名称文本组件"), SerializeField] TMP_Text cardNameText;
        [Header("卡牌描述文本组件"), SerializeField] TMP_Text cardDescriptionText;
        [Header("卡牌费用文本组件"), SerializeField] TMP_Text cardCostText;
        [Space]
        [Header("卡牌悬停交互组件"), SerializeField] private CardViewHoverSystem cardViewHoverSystem;
        private void Awake()
        {
            HasNotNullComponent(cardRenderer);
            HasNotNullComponent(cardNameText);
            HasNotNullComponent(cardDescriptionText);
            HasNotNullComponent(cardCostText);
            cardViewHoverSystem = GetOrAddComponent<CardViewHoverSystem>(FindComponentMode.Scene);
        }
        public Card Card { get; private set; }
        // 视图初始化
        public void Setup(Card model)
        {
            Card = model;
            cardRenderer.sprite = model.cardSprite;
            cardNameText.text = model.cardName;
            cardDescriptionText.text = model.cardDescription;
            cardCostText.text = model.cardCost.ToString();
        }

        #region 卡牌鼠标交互

        private void OnMouseEnter()
        {
            wrapper.SetActive(false);
            cardViewHoverSystem.ShowHoverCardView(Card, transform.position);
        }

        private void OnMouseExit()
        {
            wrapper.SetActive(true);
            cardViewHoverSystem.HideHoverCardView();
        }

        #endregion


    }
}
