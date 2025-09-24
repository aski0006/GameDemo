using Gameplay.Data;
using Gameplay.Common.Target;
using Gameplay.Effects;
using Gameplay.MVC.Interfaces;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gameplay.MVC.Model
{
    public class Card : IModel
    {
        private CardData cardData;
        
        public GUID ModelInstanceID { get; } = GUID.Generate();
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
