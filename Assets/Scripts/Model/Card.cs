using Data;
using UnityEngine;

namespace Model
{
    public class Card
    {
        private CardData cardData;
        public string cardName => cardData.CardName ?? "UnKnown";
        public string cardDescription => cardData.CardDescription ?? "UnKnown";
        public int cardCost => cardData.CardCost;
        public Sprite cardSprite => cardData.CardSprite;
        // TODO : 后续可拓展其他卡牌属性

        public Card(CardData data)
        {
            cardData = data;
        }
    }
}
