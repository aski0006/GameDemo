using AsakiFramework;
using Gameplay.Data;
using Gameplay.MVC.Interfaces;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gameplay.MVC.Model
{


    public class CombatantModel : ICombatantReadOnly
    {
        private CombatantBaseData combatantBaseData;
        public Sprite Sprite => combatantBaseData.CombatantSprite;
        public string Name => combatantBaseData.CombatantName ?? "UnKnown";
        public float MaxHp => combatantBaseData.CombatantMaxHp;
        public List<CardData> HoldCard => combatantBaseData.CardDataList;
        public float CurrentHp { get; set; }

        public bool IsDead => CurrentHp <= 0;

        public CombatantType combatantType { get; set; }
        public GUID ModelInstanceID { get; } = GUID.Generate();

        public struct CombatantModelDeathEvent
        {
            public CombatantType Type;
            public GUID InstanceID; // 谁死亡
        }

        public virtual void TakeDamage(float damage)
        {
            if (IsDead) return;
            if (damage <= 0) return;
            CurrentHp = Mathf.Max(0, CurrentHp - damage);
            if (IsDead)
            {
                EventBus.Instance.Trigger(new CombatantModelDeathEvent { Type = combatantType, InstanceID = ModelInstanceID });
            }
        }

        public virtual void Heal(float amount)
        {
            if (IsDead) return; // 如果已经死亡，则不能再治疗
            if (amount <= 0) return; // 治疗量必须大于0
            CurrentHp = Mathf.Min(MaxHp, CurrentHp + amount);
        }
        
        // TODO : 简单的暴击判定，可以扩展
        public bool IsCritical() => Random.Range(0, 100) <= 10; // 10%的概率暴击 

        public CombatantModel(CombatantBaseData data, CombatantType type)
        {
            combatantBaseData = data;
            CurrentHp = MaxHp;
            combatantType = type;
        }

    }
}
