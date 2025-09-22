using AsakiFramework.Data;
using UnityEngine;

namespace Data
{
    [CustomData]
    public class HeroCharacterData : CombatantBaseData
    {
        public override Sprite CombatantSprite => combatantSprite;
        public override string CombatantName => combatantName;
        public override float CombatantMaxHp => combatantMaxHp;
    }
}
