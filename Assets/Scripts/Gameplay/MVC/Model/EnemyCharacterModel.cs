using AsakiFramework;
using Gameplay.Data;
using Gameplay.MVC.Interfaces;
using UnityEngine;

namespace Gameplay.MVC.Model
{
    public class EnemyCharacterModel : CombatantModel, IEnemyCombatant
    {
        private EnemyCharacterData enemyData;
        public int CurrentAtk { get; set; }
        public int CurrentDef { get; set; }
        
        public EnemyCharacterModel() { } // 允许创建空的模型，后续再绑定数据
        
        public EnemyCharacterModel(EnemyCharacterData data) 
        {
            BindData(data);
        }

        public void BindData(EnemyCharacterData data)
        {
            enemyData = data;
            base.BindData(data, CombatantType.Enemy);
            CurrentAtk = data != null ? data.EnemyAttack : 0;
            CurrentDef = data != null ? data.EnemyDefense : 0;
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
