using Data;
using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Model
{
    public class CombatantModel
    {
        private CombatantBaseData combatantBaseData;
        public Sprite Sprite => combatantBaseData.CombatantSprite;
        public string Name => combatantBaseData.CombatantName ?? "UnKnown";
        public float MaxHp => combatantBaseData.CombatantMaxHp;
        public float CurrentHp { get; set; }

        public Action OnCombatantDeath;
        public bool IsDead => CurrentHp <= 0;

        public virtual void TakeDamage(float damage)
        {
            if (IsDead) return;
            if (damage <= 0) return;
            CurrentHp = Mathf.Max(0, CurrentHp - damage);
            if (IsDead) OnCombatantDeath?.Invoke();
        }

        public virtual void Heal(float amount)
        {
            if (IsDead) return; // 如果已经死亡，则不能再治疗
            if (amount <= 0) return; // 治疗量必须大于0
            CurrentHp = Mathf.Min(MaxHp, CurrentHp + amount);
        }
        public CombatantModel(CombatantBaseData data)
        {
            combatantBaseData = data;
            CurrentHp = MaxHp;
        }


    }
}
