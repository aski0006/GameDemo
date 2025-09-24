using AsakiFramework;
using Gameplay.Data;
using Gameplay.MVC.Interfaces;
using UnityEngine;

namespace Gameplay.MVC.Model
{
    public class EnemyCharacterModel : CombatantModel, IEnemyCombatant
    {
        public int CurrentAtk { get; set; }
        public int CurrentDef { get; set; }
        
        public EnemyCharacterModel(EnemyCharacterData data) : base(data, CombatantType.Enemy)
        {
            CurrentAtk = data.EnemyAttack;
            CurrentDef = data.EnemyDefense;
        }

        public override void TakeDamage(float damage)
        {
            damage -= CurrentDef;
            base.TakeDamage(damage);
         
        }

        public void ModifyAttack(int change) => CurrentAtk = Mathf.Max(0, CurrentAtk + change);
        public void ModifyDefense(int change) => CurrentDef = Mathf.Max(0, CurrentDef + change);
    }
}
