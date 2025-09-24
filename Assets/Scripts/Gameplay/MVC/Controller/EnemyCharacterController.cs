using Gameplay.MVC.Model;
using Gameplay.MVC.View;

namespace Gameplay.MVC.Controller
{
    public class EnemyCharacterController : CombatantBaseController
    {
        public EnemyCharacterController(EnemyCharacterModel model, CombatantViewBase view) : base(model, view)
        { }

        public void ModifyAttack(int change) => GetModel<EnemyCharacterModel>().ModifyAttack(change);
        public void ModifyDefense(int change) => GetModel<EnemyCharacterModel>().ModifyDefense(change);
    }
}
