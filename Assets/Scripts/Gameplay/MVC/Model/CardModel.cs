using Gameplay.Data;
using Gameplay.Common.Target;
using Gameplay.Effects;
using Gameplay.MVC.Interfaces;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gameplay.MVC.Model
{
    public class CardModel : IModel
    {
        public GUID ModelInstanceID { get; } = GUID.Generate();
        private CardData cardData;
        
        public string cardName => cardData != null ? cardData.CardName ?? "UnKnown" : "UnKnown";
        public string cardDescription => cardData != null ? cardData.CardDescription ?? "UnKnown" : "UnKnown";
        public int cardCost { get; set; }
        public Sprite cardSprite => cardData != null ? cardData.CardSprite : null;
        public Effect manualTargetEffect => cardData != null ? cardData.ManualTargetEffect : null;
        public List<AutoTargetEffect> autoTargetEffects => cardData != null ? cardData.AutoTargetEffects : null;
        // TODO : 后续可拓展其他卡牌属性

        public CardModel() { } // 允许创建空的模型，后续再绑定数据
        
        public CardModel(CardData data) 
        {
            BindData(data);
        }
        
        public void BindData(CardData data)
        {
            cardData = data;
            cardCost = data != null ? data.CardCost : 0;
        }
    }
}
