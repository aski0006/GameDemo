using AsakiFramework;
using Gameplay.Data;

namespace Gameplay.Model
{
    public class HeroCharacter : CombatantModel
    {

        public HeroCharacter(HeroCharacterData data) : base(data, CombatantType.Hero)
        { }

    }
}
