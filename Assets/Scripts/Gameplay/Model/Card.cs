using Gameplay.Data;
using Gameplay.Common.Target;
using Gameplay.Effects;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Model
{
    public class Card
    {
        private CardData cardData;
        public string cardName => cardData.CardName ?? "UnKnown";
        public string cardDescription => cardData.CardDescription ?? "UnKnown";
        public int cardCost { get; set; }
        public Sprite cardSprite => cardData.CardSprite;
        public Effect manualTargetEffect => cardData.ManualTargetEffect;
        public List<AutoTargetEffect> autoTargetEffects => cardData.AutoTargetEffects;
        // TODO : 后续可拓展其他卡牌属性

        public Card(CardData data)
        {
            cardData = data;
            cardCost = data.CardCost;
        }
    }
}
