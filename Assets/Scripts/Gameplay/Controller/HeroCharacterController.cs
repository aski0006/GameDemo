using Gameplay.View;
using Model;

namespace Gameplay.Controller
{
    public class HeroCharacterController : CombatantBaseController
    {

        public HeroCharacterController(HeroCharacter model, CombatantVIewBase view) : base(model, view)
        { }
    }
}
