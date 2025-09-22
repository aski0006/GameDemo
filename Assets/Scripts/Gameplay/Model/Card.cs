using Data;
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
        
        public List<Effect> cardEffects => cardData.cardEffects ?? new List<Effect>();
        // TODO : 后续可拓展其他卡牌属性

        public Card(CardData data)
        {
            cardData = data;
            cardCost = data.CardCost;
        }
    }
}
