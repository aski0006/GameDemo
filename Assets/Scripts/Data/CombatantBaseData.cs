using UnityEngine;

namespace Data
{
    public abstract class CombatantBaseData : ScriptableObject
    {
        [Header("战斗单位图像"), SerializeField]
        protected Sprite combatantSprite;
        [Header("战斗单位名称"), SerializeField]
        protected string combatantName;
        [Header("战斗单位最大血量"), SerializeField]
        protected float combatantMaxHp;

        public abstract Sprite CombatantSprite { get; }
        public abstract string CombatantName { get; }
        public abstract float CombatantMaxHp { get; }
    }
}
