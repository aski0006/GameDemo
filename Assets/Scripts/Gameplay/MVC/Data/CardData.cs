using AsakiFramework.Data;
using Gameplay.Common.Target;
using Gameplay.Effects;
using SerializeReferenceEditor;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Data
{
    [CustomData(category: "卡牌", description: "游戏中的卡牌数据")]
    public class CardData : ScriptableObject
    {
        [Header("卡牌名称")]
        [field: SerializeField] public string CardName { get; private set; }
        [Header("卡牌描述")]
        [field: SerializeField, TextArea(3, 10)] public string CardDescription { get; private set; }
        [Header("卡牌费用")]
        [field: SerializeField] public int CardCost { get; private set; }
        [Header("卡牌精灵")]
        [field: SerializeField] public Sprite CardSprite { get; private set; }
        [Header("卡牌拖拽效果")]
        [field: SerializeReference, SR] public Effect ManualTargetEffect { get; private set; } = null;
        [Header("卡牌使用效果")]
        [field:SerializeReference, SR] public List<AutoTargetEffect> AutoTargetEffects { get; private set; } = new List<AutoTargetEffect>();


    }
}
