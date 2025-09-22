using Gameplay.View;
using Model;

namespace Gameplay.Controller
{
    public class EnemyCharacterController : CombatantBaseController
    {
        public EnemyCharacterController(EnemyCharacter model, CombatantViewBase view) : base(model, view)
        { }

        public void ModifyAttack(int change) => GetModel<EnemyCharacter>().ModifyAttack(change);
        public void ModifyDefense(int change) => GetModel<EnemyCharacter>().ModifyDefense(change);
    }
}
