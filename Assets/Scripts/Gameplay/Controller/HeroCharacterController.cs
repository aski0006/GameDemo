using Gameplay.View;
using Gameplay.Model;

namespace Gameplay.Controller
{
    public class HeroCharacterController : CombatantBaseController
    {

        public HeroCharacterController(HeroCharacter model, CombatantViewBase view) : base(model, view)
        { }
    }
}
