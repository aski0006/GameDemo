using AsakiFramework.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Data
{
    [CustomData(category:"角色数据/敌人角色数据", description: "敌人角色的基础数据")]
    public class EnemyCharacterData : CombatantBaseData
    {

        public override Sprite CombatantSprite => combatantSprite;
        public override string CombatantName => combatantName;
        public override float CombatantMaxHp => combatantMaxHp;
        public override List<CardData> CardDataList => cardDataList;

        [Header("敌人攻击力"), SerializeField] private int enemyAttack;
        
        [Header("敌人防御力"), SerializeField] private int enemyDefense;
        
        public int EnemyAttack => enemyAttack;
        
        public int EnemyDefense => enemyDefense;
    }
}
