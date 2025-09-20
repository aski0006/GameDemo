using System.Collections.Generic;

namespace AsakiFramework
{
    public abstract class GameAction
    {
        public List<GameAction> PreReactions { get; private set; } = new(); // 动作前置反应
        public List<GameAction> CurPerformReactions { get; private set; } = new(); // 动作当前执行反应
        public List<GameAction> PostReactions { get; private set; } = new(); // 动作后置反应
    }
}
