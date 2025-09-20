using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AsakiFramework
{
    public class ActionSystem : Singleton<ActionSystem>
    {
        /* ========== 订阅缓存 ========== */
        private readonly Dictionary<Type, List<Action<GameAction>>> _preSubs   = new();
        private readonly Dictionary<Type, List<Action<GameAction>>> _postSubs  = new();
        private readonly Dictionary<Type, Func<GameAction, IEnumerator>> _perfSubs = new();

        /* ========== 重复订阅去重 ========== */
        private readonly Dictionary<Type, HashSet<Action<GameAction>>> _preSet  = new();
        private readonly Dictionary<Type, HashSet<Action<GameAction>>> _postSet = new();

        /* ========== 调用栈防死循环 ========== */
        private readonly HashSet<Type> _callStack = new();

        /* ========== 调试 & 性能 ========== */
        private int _runningCount;
        public  bool IsRunning => _runningCount > 0;

        public void PerformGameAction(GameAction action, Action onFinish = null)
        {
            CoroutineUtility.StartCoroutine(Flow(action, onFinish));
        }

        /* ------------------------------------------------------------------ */
        #region 订阅管理（带去重 + 缓存包装）
        public void SubscribePre<T>(Action<T> callback) where T : GameAction => AddUnique<T>(callback, _preSubs, _preSet);
        public void SubscribePost<T>(Action<T> callback) where T : GameAction => AddUnique<T>(callback, _postSubs, _postSet);
        public void UnsubscribePre<T>(Action<T> callback) where T : GameAction => RemoveUnique<T>(callback, _preSubs, _preSet);
        public void UnsubscribePost<T>(Action<T> callback) where T : GameAction => RemoveUnique<T>(callback, _postSubs, _postSet);

        
        public void AttachPerformer<T>(Func<T, IEnumerator> performer) where T : GameAction
        {
            Func<GameAction, IEnumerator> wrapped = a => performer((T)a);
            _perfSubs[typeof(T)] = wrapped;
        }
        public void DetachPerformer<T>() where T : GameAction
        {
            _perfSubs.Remove(typeof(T));
        }
        public void ClearAll()
        {
            _preSubs.Clear();  _preSet.Clear();
            _postSubs.Clear(); _postSet.Clear();
            _perfSubs.Clear();
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

            // 创建与 Add 时完全相同的包装委托
            Action<GameAction> wrapped = a => cb((T)a);

            if (set[t].Remove(wrapped)) list.Remove(wrapped);
        }
        #endregion

        /* ------------------------------------------------------------------ */
        #region 核心流程
        private IEnumerator Flow(GameAction action, Action onFinish)
        {
            if (!_callStack.Add(action.GetType()))
            {
                Debug.LogError($"[ActionSystem] 死循环反应：{action.GetType().Name}");
                yield break;
            }
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

            _callStack.Remove(action.GetType());
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