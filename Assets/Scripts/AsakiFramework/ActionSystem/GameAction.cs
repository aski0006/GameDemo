using System.Collections.Generic;

public abstract class GameAction
{
    public List<GameAction> PreReactions  { get; } = new();
    public List<GameAction> CurPerformReactions { get; } = new();
    public List<GameAction> PostReactions { get; } = new();

    /* ---------- 类型安全的一次性添加 ---------- */
    public TReaction AddPreReaction<TReaction>(TReaction reaction) where TReaction : GameAction
    {
        PreReactions.Add(reaction);
        return reaction;   // 支持继续链式
    }

    public TReaction AddPostReaction<TReaction>(TReaction reaction) where TReaction : GameAction
    {
        PostReactions.Add(reaction);
        return reaction;
    }

    public TReaction AddPerformReaction<TReaction>(TReaction reaction) where TReaction : GameAction
    {
        CurPerformReactions.Add(reaction);
        return reaction;
    }

    /* ---------- 极简无参版本（可选） ---------- */
    public TReaction AddPreReaction<TReaction>() where TReaction : GameAction, new()
        => AddPreReaction(new TReaction());

    public TReaction AddPostReaction<TReaction>() where TReaction : GameAction, new()
        => AddPostReaction(new TReaction());
}
