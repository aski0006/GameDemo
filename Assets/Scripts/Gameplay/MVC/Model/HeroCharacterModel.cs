using Gameplay.Data;
using Gameplay.MVC.Interfaces;

namespace Gameplay.MVC.Model
{
    public class HeroCharacterModel : CombatantModel, IHeroCombatant
    {

        public HeroCharacterModel(HeroCharacterData data) : base(data, CombatantType.Hero)
        { }

    }
}
