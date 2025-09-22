using UnityEngine;

namespace Data
{
    public class EnemyCharacterData : CombatantBaseData
    {

        public override Sprite CombatantSprite => combatantSprite;
        public override string CombatantName => combatantName;
        public override float CombatantMaxHp => combatantMaxHp;

        [Header("敌人攻击力"), SerializeField] private int enemyAttack;
        
        [Header("敌人防御力"), SerializeField] private int enemyDefense;
        
        public int EnemyAttack => enemyAttack;
        
        public int EnemyDefense => enemyDefense;
    }
}
