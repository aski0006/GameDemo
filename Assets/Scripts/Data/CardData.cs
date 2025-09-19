using AsakiFramework.Data;
using UnityEngine;

namespace Data
{
    [CustomData(category:"卡牌", description:"游戏中的卡牌数据")]
    public class CardData : ScriptableObject
    {
        [Header("卡牌名称"), SerializeField] private string cardName;
        [Header("卡牌描述"), SerializeField, TextArea(3, 10)] private string cardDescription;
        [Header("卡牌费用"), SerializeField] private int cardCost;
        [Header("卡牌精灵"), SerializeField] private Sprite cardSprite;
        // TODO : 后续可拓展其他卡牌属性
        
        public string CardName => cardName;
        public string CardDescription => cardDescription;
        public int CardCost => cardCost;
        public Sprite CardSprite => cardSprite;
    }
}
