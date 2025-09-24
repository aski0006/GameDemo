using Gameplay.Data;
using Gameplay.MVC.Interfaces;

namespace Gameplay.MVC.Model
{
    public class HeroCharacter : CombatantModel, IHeroCombatant
    {

        public HeroCharacter(HeroCharacterData data) : base(data, CombatantType.Hero)
        { }

    }
}
