using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Data
{
    public abstract class CombatantBaseData : ScriptableObject
    {
        [Header("战斗单位图像"), SerializeField]
        protected Sprite combatantSprite;
        [Header("战斗单位名称"), SerializeField]
        protected string combatantName;
        [Header("战斗单位最大血量"), SerializeField]
        protected float combatantMaxHp;
        [Header("战斗单位持有卡牌数据"), SerializeField]
        protected List<CardData> cardDataList;
        
        public abstract Sprite CombatantSprite { get; }
        public abstract string CombatantName { get; }
        public abstract float CombatantMaxHp { get; }
        
        public abstract List<CardData> CardDataList { get; }
    }
}
