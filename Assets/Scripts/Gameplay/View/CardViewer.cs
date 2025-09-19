using AsakiFramework;
using AsakiFramework.ObjectPool;
using Model;
using System;
using TMPro;
using UnityEngine;

namespace Gameplay.View
{
    public class CardViewer : AsakiMono
    {
        [Header("卡牌精灵渲染器"), SerializeField] private SpriteRenderer cardRenderer;
        [Space]
        [Header("卡牌文本组件列表")]
        [Header("卡牌名称文本组件"), SerializeField] TMP_Text cardNameText;
        [Header("卡牌描述文本组件"), SerializeField] TMP_Text cardDescriptionText;
        [Header("卡牌费用文本组件"), SerializeField] TMP_Text cardCostText;

        private void Awake()
        {
            HasNotNullComponent(cardRenderer);
            HasNotNullComponent(cardNameText);
            HasNotNullComponent(cardDescriptionText);
            HasNotNullComponent(cardCostText);
        }

        // 视图初始化
        public void Setup(CardModel model)
        {
            cardRenderer.sprite = model.cardSprite;
            cardNameText.text = model.cardName;
            cardDescriptionText.text = model.cardDescription;
            cardCostText.text = model.cardCost.ToString();
        }
        

    }
}
