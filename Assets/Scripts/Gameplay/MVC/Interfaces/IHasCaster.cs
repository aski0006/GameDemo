using Gameplay.MVC.Controller;

namespace Gameplay.Interfaces
{
    /// <summary>
    /// 如果某些 GameAction 带有“施放者”，实现此接口以便藏品目标策略能访问它
    /// （可由你已有的 DamageGA / HealGA 等实现）
    /// </summary>
    public interface IHasCaster
    {
        CombatantBaseController Caster { get; }
    }
}
