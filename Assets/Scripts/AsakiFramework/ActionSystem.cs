using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AsakiFramework
{
    public class ActionSystem : Singleton<ActionSystem>
    {
        public enum ReactionTiming { Pre, Post }

        /* ========== 订阅缓存 ========== */
        private readonly Dictionary<Type, List<Action<GameAction>>> _preSubs = new();
        private readonly Dictionary<Type, List<Action<GameAction>>> _postSubs = new();
        private readonly Dictionary<Type, Func<GameAction, IEnumerator>> _perfSubs = new();

        /* ========== 重复订阅去重 ========== */
        private readonly Dictionary<Type, HashSet<Action<GameAction>>> _preSet = new();
        private readonly Dictionary<Type, HashSet<Action<GameAction>>> _postSet = new();

        /* ========== 调用栈防死循环：按 ActionGuid 检测 ========== */
        // 用 Guid 集合加有序列表保存进入 Flow 的动作标识（稳定于动作类型）
        private readonly HashSet<Guid> _callStackGuid = new();
        private readonly List<(Guid guid, Type type)> _callStackList = new();

        /* ========== 最大链式深度（超过则中断该 Flow） ========== */
        // 可按需暴露为属性或 inspector 可调值，这里默认 200
        private int _maxFlowDepth = 200;
        public int MaxFlowDepth
        {
            get => _maxFlowDepth;
            set => _maxFlowDepth = Math.Max(1, value);
        }

        /* ========== 调试 & 性能 ========== */
        private int _runningCount;
        public bool IsRunning => _runningCount > 0;

        public void PerformGameAction(GameAction action, Action onFinish = null)
        {
            CoroutineUtility.StartCoroutine(Flow(action, onFinish));
        }

        /* ------------------------------------------------------------------ */
        #region 订阅管理（带去重 + 缓存包装）

        public void SubscribePre<T>(Action<T> callback) where T : GameAction => AddUnique<T>(callback, _preSubs, _preSet);
        public void SubscribePost<T>(Action<T> callback) where T : GameAction => AddUnique<T>(callback, _postSubs, _postSet);
        public void SubscribeReaction<T>(Action<T> callback, ReactionTiming t) where T : GameAction
        {
            switch (t)
            {
                case ReactionTiming.Pre:
                    SubscribePre(callback);
                    break;
                case ReactionTiming.Post:
                    SubscribePost(callback);
                    break;
                default:
                    throw new System.NotImplementedException();
            }
        }
        public void SubscribeReaction(Type actionType, Action<GameAction> callback, ReactionTiming t)
        {
            if (actionType == null || callback == null) return;
            if (t == ReactionTiming.Pre) AddUniqueByType(actionType, callback, _preSubs, _preSet);
            else AddUniqueByType(actionType, callback, _postSubs, _postSet);
        }
        
        public void UnsubscribePre<T>(Action<T> callback) where T : GameAction => RemoveUnique<T>(callback, _preSubs, _preSet);
        public void UnsubscribePost<T>(Action<T> callback) where T : GameAction => RemoveUnique<T>(callback, _postSubs, _postSet);
        public void UnsubscribeReaction<T>(Action<T> callback, ReactionTiming t) where T : GameAction
        {
            switch (t)
            {
                case ReactionTiming.Pre:
                    UnsubscribePre(callback);
                    break;
                case ReactionTiming.Post:
                    UnsubscribePost(callback);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        public void UnsubscribeReaction(Type actionType, Action<GameAction> callback, ReactionTiming t)
        {
            if (actionType == null || callback == null) return;
            if (t == ReactionTiming.Pre) RemoveUniqueByType(actionType, callback, _preSubs, _preSet);
            else RemoveUniqueByType(actionType, callback, _postSubs, _postSet);
        }
        public void AttachPerformer<T>(Func<T, IEnumerator> performer) where T : GameAction
        {
            Func<GameAction, IEnumerator> wrapped = a => performer((T)a);
            _perfSubs[typeof(T)] = wrapped;
            Debug.Log($"[ActionSystem] 添加执行器：{typeof(T).Name}");
        }
        public void DetachPerformer<T>() where T : GameAction
        {
            _perfSubs.Remove(typeof(T));
        }
        public void ClearAll()
        {
            _preSubs.Clear();
            _preSet.Clear();
            _postSubs.Clear();
            _postSet.Clear();
            _perfSubs.Clear();
            _callStackGuid.Clear();
            _callStackList.Clear();
        }
        private static void AddUnique<T>(Action<T> cb,
            Dictionary<Type, List<Action<GameAction>>> dic,
            Dictionary<Type, HashSet<Action<GameAction>>> set) where T : GameAction
        {
            var t = typeof(T);
            if (!dic.TryGetValue(t, out var list))
            {
                list = new List<Action<GameAction>>();
                dic[t] = list;
                set[t] = new HashSet<Action<GameAction>>();
            }
            Action<GameAction> wrapped = a => cb((T)a);
            if (set[t].Add(wrapped)) list.Add(wrapped);
        }

        private void RemoveUnique<T>(Action<T> cb,
            Dictionary<Type, List<Action<GameAction>>> dic,
            Dictionary<Type, HashSet<Action<GameAction>>> set) where T : GameAction
        {
            var t = typeof(T);
            if (!dic.TryGetValue(t, out var list)) return;

            // 创建与 OnAdd 时完全相同的包装委托
            Action<GameAction> wrapped = a => cb((T)a);

            if (set[t].Remove(wrapped)) list.Remove(wrapped);
        }

        private static void AddUniqueByType(Type t,
            Action<GameAction> cb,
            Dictionary<Type, List<Action<GameAction>>> dic,
            Dictionary<Type, HashSet<Action<GameAction>>> set)
        {
            if (!dic.TryGetValue(t, out var list))
            {
                list = new List<Action<GameAction>>();
                dic[t] = list;
                set[t] = new HashSet<Action<GameAction>>();
            }
            if (set[t].Add(cb)) list.Add(cb);
        }

        private static void RemoveUniqueByType(Type t,
            Action<GameAction> cb,
            Dictionary<Type, List<Action<GameAction>>> dic,
            Dictionary<Type, HashSet<Action<GameAction>>> set)
        {
            if (!dic.TryGetValue(t, out var list)) return;
            if (set[t].Remove(cb)) list.Remove(cb);
        }

        
        #endregion

        /* ------------------------------------------------------------------ */
        #region 核心流程

        private IEnumerator Flow(GameAction action, Action onFinish)
        {
            if (action == null)
            {
                yield break;
            }

            var guid = action.ActionGuid;
            var type = action.GetType();

            // 检查最大链深（guard）：如果当前已达到限制则中断并丢弃后续反应
            if (_callStackList.Count >= _maxFlowDepth)
            {
                string msg = $"[ActionSystem] 超过最大链式深度 {_maxFlowDepth}，中断动作：{type.Name}";
                Debug.LogError(msg);
                yield break;
            }

            // try push to call stack guided by GUID (稳定于动作类型)
            if (!_callStackGuid.Add(guid))
            {
                // 已存在，构建并打印出完整的调用链与环路信息，便于调试
                int loopStart = _callStackList.FindIndex(p => p.guid == guid);
                string fullStack = _callStackList.Count > 0 ? string.Join(" -> ", _callStackList.ConvertAll(p => p.type.Name)) : "<empty>";
                string cycle;
                if (loopStart >= 0)
                {
                    var cycleParts = _callStackList.GetRange(loopStart, _callStackList.Count - loopStart)
                        .Select(p => p.type.Name).ToList();
                    cycleParts.Add(type.Name); // 闭环回到当前类型实例的类型名
                    cycle = string.Join(" -> ", cycleParts);
                }
                else
                {
                    cycle = string.Join(" -> ", _callStackList.ConvertAll(p => p.type.Name)) + " -> " + type.Name;
                }

                string msg = $"[ActionSystem] 死循环反应 (GUID)：{type.Name} ({guid})\nFull call chain: {fullStack}\nDetected cycle: {cycle}\nStackTrace:\n{Environment.StackTrace}";
                Debug.LogError(msg);
                yield break;
            }

            // push ordered tuple
            _callStackList.Add((guid, type));

            _runningCount++;
            /* ------ Pre ------ */
            InvokeList(action, _preSubs);
            yield return DoReactions(action.PreReactions);
            /* ------ Perform ------ */
            yield return DoPerformer(action);
            yield return DoReactions(action.CurPerformReactions);
            /* ------ Post ------ */
            InvokeList(action, _postSubs);
            yield return DoReactions(action.PostReactions);

            // pop from call stack (ordered & set)
            _callStackGuid.Remove(guid);
            if (_callStackList.Count > 0 && _callStackList[_callStackList.Count - 1].guid == guid)
                _callStackList.RemoveAt(_callStackList.Count - 1);
            else
                _callStackList.RemoveAll(x => x.guid == guid); // 保底移除

            _runningCount--;
            onFinish?.Invoke();
        }
        private IEnumerator DoPerformer(GameAction action)
        {
            if (_perfSubs.TryGetValue(action.GetType(), out var p))
                yield return p(action);
        }
        private IEnumerator DoReactions(List<GameAction> reactions)
        {
            if (reactions == null || reactions.Count == 0) yield break;
            foreach (var r in reactions)
                yield return Flow(r, null);
        }
        private static void InvokeList(GameAction action, Dictionary<Type, List<Action<GameAction>>> dic)
        {
            if (dic.TryGetValue(action.GetType(), out var list))
            {
                var arr = list.ToArray(); // 快照，防止订阅者修改列表
                foreach (var l in arr) l.Invoke(action);
            }
        }

        #endregion
    }
}