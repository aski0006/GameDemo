using AsakiFramework;
using Data;
using UnityEngine;

namespace Gameplay.Model
{
    public class CombatantModel
    {
        private CombatantBaseData combatantBaseData;
        public Sprite Sprite => combatantBaseData.CombatantSprite;
        public string Name => combatantBaseData.CombatantName ?? "UnKnown";
        public float MaxHp => combatantBaseData.CombatantMaxHp;
        public float CurrentHp { get; set; }
        public bool IsDead => CurrentHp <= 0;

        private static ulong _nextID = 1; // 静态字段用于生成唯一ID
        public ulong CombatantInstanceID { get; } = _nextID++; // 每个实例的唯一ID
        [RuntimeInitializeOnLoadMethod]
        static void ResetID() => _nextID = 1; // 重置ID
        public struct CombatantDeathEvent
        {
            public ulong InstanceID; // 谁死亡
        }

        public virtual void TakeDamage(float damage)
        {
            if (IsDead) return;
            if (damage <= 0) return;
            CurrentHp = Mathf.Max(0, CurrentHp - damage);
            if (IsDead) EventBus.Instance.Trigger(new CombatantDeathEvent { InstanceID = CombatantInstanceID });
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
