using AsakiFramework.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Data
{
    [CustomData(category:"角色数据/英雄角色数据", description:"英雄角色的基础数据")]
    public class HeroCharacterData : CombatantBaseData
    {
        public override Sprite CombatantSprite => combatantSprite;
        public override string CombatantName => combatantName;
        public override float CombatantMaxHp => combatantMaxHp;
        public override List<CardData> CardDataList => cardDataList;
    }
}
