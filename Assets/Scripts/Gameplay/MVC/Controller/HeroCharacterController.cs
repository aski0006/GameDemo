using Gameplay.MVC.View;
using Gameplay.MVC.Model;

namespace Gameplay.MVC.Controller
{
    public class HeroCharacterController : CombatantBaseController
    {

        public HeroCharacterController(HeroCharacter model, CombatantViewBase view) : base(model, view)
        { }
    }
}
