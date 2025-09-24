using Gameplay.MVC.Model;
using Gameplay.MVC.View;

namespace Gameplay.MVC.Controller
{
    public class HeroCharacterController : CombatantBaseController
    {

        public HeroCharacterController(HeroCharacter model, CombatantViewBase view) : base(model, view)
        { }
    }
}
