using AsakiFramework;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using UnityEditor;

/// <summary>
/// 基础 GameAction：ActionGuid 现在基于 类型名 + 可覆写的上下文字符串 计算，
/// 使得相同类型但不同上下文（例如 generated / origin path 不同）的动作拥有不同 GUID，
/// 从而避免类型交替造成的误报死循环，同时仍可按 GUID 做循环检测与限制深度。
/// </summary>
public abstract class GameAction
{
    // 标识是否由系统/Effect 生成（可用于特殊判断，向后兼容）
    public bool IsGenerated { get; set; } = false;

    // 来源链（按顺序记录触发链上的藏品/来源 id）
    public List<GUID> OriginPath { get; } = new List<GUID>();

    // 快捷：当前链深度（等于 OriginPath.Count）
    public int ChainDepth => OriginPath?.Count ?? 0;

    public List<GameAction> PreReactions  { get; } = new();
    public List<GameAction> CurPerformReactions { get; } = new();
    public List<GameAction> PostReactions { get; } = new();

    // ---------------- 新版：用于死循环检测的确定性 Action GUID ----------------
    // 基于： 类型全名 + '|' + GetActionContextString()
    // 默认上下文包含 IsGenerated 和 OriginPath 摘要；子类可以覆写 GetActionContextString() 增加更多上下文（如 Caster ID）
    public virtual Guid ActionGuid
    {
        get
        {
            var typeName = GetType().FullName ?? GetType().Name;
            var context = GetActionContextString() ?? "";
            // 合并 typeName 与 context，再做确定性 GUID（MD5）
            return DeterministicGuidFromString(typeName + "|" + context);
        }
    }

    /// <summary>
    /// 子类可覆写此方法来在 GUID 计算时注入更多上下文信息（例如：Caster Id、SkillId）
    /// 默认实现返回 IsGenerated 与 OriginPath 的可读摘要。
    /// </summary>
    protected virtual string GetActionContextString()
    {
        // 以可读字符串表示 OriginPath（GUID 列表）与 IsGenerated 标志
        var origin = (OriginPath != null && OriginPath.Count > 0)
            ? string.Join(",", OriginPath.Select(g => g.ToString()))
            : "";
        return $"gen:{IsGenerated};origin:{origin}";
    }

    private static Guid DeterministicGuidFromString(string s)
    {
        // MD5 取 16 字节 -> Guid
        using (var md5 = MD5.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(s ?? "");
            var hash = md5.ComputeHash(bytes);
            return new Guid(hash);
        }
    }
    // ---------------------------------------------------------------------

    // 便捷方法：复制 origin path
    public void CopyOriginFrom(GameAction other)
    {
        OriginPath.Clear();
        if (other?.OriginPath != null && other.OriginPath.Count > 0)
            OriginPath.AddRange(other.OriginPath);
    }

    // 便捷方法：push origin id（返回新的深度）
    public int PushOrigin(GUID originId)
    {
        if (originId != default) OriginPath.Add(originId);
        return ChainDepth;
    }

    /* ---------- 其它封装方法不变 ---------- */
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
    
    public TReaction AddReaction<TReaction>(TReaction reaction, ActionSystem.ReactionTiming timing) where TReaction : GameAction
    {
        switch (timing)
        {
            case ActionSystem.ReactionTiming.Pre:
                return AddPreReaction(reaction);
            case ActionSystem.ReactionTiming.Post:
                return AddPostReaction(reaction);
            default:
                throw new System.NotImplementedException();
        }
    }

    public TReaction AddPerformReaction<TReaction>(TReaction reaction) where TReaction : GameAction
    {
        CurPerformReactions.Add(reaction);
        return reaction;
    }

    public TReaction AddPreReaction<TReaction>() where TReaction : GameAction, new()
        => AddPreReaction(new TReaction());

    public TReaction AddPostReaction<TReaction>() where TReaction : GameAction, new()
        => AddPostReaction(new TReaction());
}