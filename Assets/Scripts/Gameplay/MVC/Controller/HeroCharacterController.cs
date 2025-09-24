using Gameplay.MVC.Model;
using Gameplay.MVC.View;

namespace Gameplay.MVC.Controller
{
    public class HeroCharacterController : CombatantBaseController
    {

        public HeroCharacterController(HeroCharacterModel model, CombatantViewBase view) : base(model, view)
        { }
    }
}
