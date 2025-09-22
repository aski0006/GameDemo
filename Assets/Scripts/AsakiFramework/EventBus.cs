using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace AsakiFramework
{
    /// <summary>
    /// 全局事件总线 - 基于单例的类型安全委托驱动事件系统
    /// 提供可调试、可清理、可扩展的事件管理功能
    /// </summary>
    [DefaultExecutionOrder(-1001)]
    public class EventBus : Singleton<EventBus>
    {
        #region 内部数据结构
        
        /// <summary>
        /// 事件处理器包装器接口
        /// </summary>
        private interface IEventHandler
        {
            void Remove(object handler);
            void Clear();
            int Count { get; }
        }
        
        /// <summary>
        /// 泛型事件处理器包装器
        /// </summary>
        private class EventHandler<T> : IEventHandler where T : struct
        {
            private readonly List<Action<T>> _handlers = new List<Action<T>>();
            private readonly List<Action<T>> _handlersToRemove = new List<Action<T>>();
            private bool _isInvoking;
            
            public void Add(Action<T> handler)
            {
                if (handler == null)
                {
                    Debug.LogError("[EventBus] Cannot add null handler");
                    return;
                }
                
                if (!_handlers.Contains(handler))
                {
                    _handlers.Add(handler);
                }
                else
                {
                    Debug.LogWarning($"[EventBus] Handler {handler.Method.Name} already registered for event {typeof(T).Name}");
                }
            }
            
            public void Remove(Action<T> handler)
            {
                if (_isInvoking)
                {
                    _handlersToRemove.Add(handler);
                }
                else
                {
                    _handlers.Remove(handler);
                }
            }
            
            public void Invoke(T eventData)
            {
                if (_handlers.Count == 0) return;
                
                _isInvoking = true;
                
                // 创建副本以避免在调用过程中修改集合
                var handlersCopy = new List<Action<T>>(_handlers);
                
                foreach (var handler in handlersCopy)
                {
                    try
                    {
                        handler?.Invoke(eventData);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[EventBus] Error invoking handler {handler.Method.Name} for event {typeof(T).Name}: {ex.Message}");
                    }
                }
                
                _isInvoking = false;
                
                // 处理在调用过程中请求移除的处理器
                foreach (var handler in _handlersToRemove)
                {
                    _handlers.Remove(handler);
                }
                _handlersToRemove.Clear();
            }
            
            void IEventHandler.Remove(object handler)
            {
                if (handler is Action<T> typedHandler)
                {
                    Remove(typedHandler);
                }
            }
            
            void IEventHandler.Clear()
            {
                _handlers.Clear();
                _handlersToRemove.Clear();
            }
            
            public int Count => _handlers.Count;
        }
        public delegate void RefAction<T>(ref T e) where T : struct; // 引用类型事件
        
        /// <summary>
        /// 支持 ref 的事件处理器包装器
        /// </summary>
        private class RefEventHandler<T> : IEventHandler where T : struct
        {
            private readonly List<RefAction<T>> _handlers = new();
            private readonly List<RefAction<T>> _toRemove = new();
            private bool _invoking;

            public void Add(RefAction<T> handler)
            {
                if (handler == null) return;
                if (!_handlers.Contains(handler)) _handlers.Add(handler);
            }

            public void Remove(RefAction<T> handler)
            {
                if (_invoking) _toRemove.Add(handler);
                else _handlers.Remove(handler);
            }

            public void Invoke(ref T eventData)
            {
                if (_handlers.Count == 0) return;
                _invoking = true;
                foreach (var h in _handlers) h?.Invoke(ref eventData);
                _invoking = false;
                foreach (var h in _toRemove) _handlers.Remove(h);
                _toRemove.Clear();
            }

            void IEventHandler.Remove(object handler) => Remove(handler as RefAction<T>);
            void IEventHandler.Clear() { _handlers.Clear(); _toRemove.Clear(); }
            public int Count => _handlers.Count;
        }
        
        /// <summary>
        /// 事件统计信息
        /// </summary>
        [Serializable]
        public class EventStatistics
        {
            public string EventType;
            public int HandlerCount;
            public int InvokeCount;
            public int TotalInvokeTimeMs;
            
            public float AverageInvokeTimeMs => InvokeCount > 0 ? (float)TotalInvokeTimeMs / InvokeCount : 0f;
        }
        
        #endregion
        
        #region 字段和属性
        
        private readonly Dictionary<Type, IEventHandler> _eventHandlers = new Dictionary<Type, IEventHandler>();
        private readonly Dictionary<Type, EventStatistics> _statistics = new Dictionary<Type, EventStatistics>();
        private readonly Dictionary<Type, IEventHandler> _refHandlers = new Dictionary<Type, IEventHandler>();
        
        [Header("调试设置")]
        [SerializeField] private bool _enableDebugLogging = false;
        [SerializeField] private bool _enableStatistics = true;
        [SerializeField] private int _slowEventThresholdMs = 5; // 慢事件阈值
        
        /// <summary>
        /// 是否启用调试日志
        /// </summary>
        public bool EnableDebugLogging
        {
            get => _enableDebugLogging;
            set => _enableDebugLogging = value;
        }
        
        /// <summary>
        /// 是否启用统计
        /// </summary>
        public bool EnableStatistics
        {
            get => _enableStatistics;
            set => _enableStatistics = value;
        }
        
        /// <summary>
        /// 获取所有注册的事件类型
        /// </summary>
        public Type[] RegisteredEventTypes
        {
            get
            {
                var types = new Type[_eventHandlers.Count];
                _eventHandlers.Keys.CopyTo(types, 0);
                return types;
            }
        }
        
        /// <summary>
        /// 获取事件统计信息
        /// </summary>
        public EventStatistics[] GetStatistics()
        {
            var stats = new EventStatistics[_statistics.Count];
            _statistics.Values.CopyTo(stats, 0);
            return stats;
        }
        
        #endregion
        
        #region 公共API
        
        /// <summary>
        /// 订阅事件
        /// </summary>
        /// <typeparam name="T">事件类型</typeparam>
        /// <param name="handler">事件处理器</param>
        public void Subscribe<T>(Action<T> handler) where T : struct
        {
            if (handler == null)
            {
                Debug.LogError("[EventBus] Cannot subscribe null handler");
                return;
            }
            
            var eventType = typeof(T);
            
            if (!_eventHandlers.TryGetValue(eventType, out var handlerWrapper))
            {
                handlerWrapper = new EventHandler<T>();
                _eventHandlers[eventType] = handlerWrapper;
                
                if (_enableStatistics && !_statistics.ContainsKey(eventType))
                {
                    _statistics[eventType] = new EventStatistics { EventType = eventType.Name };
                }
            }
            
            ((EventHandler<T>)handlerWrapper).Add(handler);
            
            if (_enableDebugLogging)
            {
                Debug.Log($"[EventBus] Subscribed {handler.Method.Name} to event {eventType.Name}");
            }
        }
        /// <summary>订阅 ref 事件（零 GC）</summary>
        public void SubscribeRef<T>(RefAction<T> handler) where T : struct
        {
            var t = typeof(T);
            if (!_refHandlers.TryGetValue(t, out var h))
            {
                h = new RefEventHandler<T>();
                _refHandlers[t] = h;
            }
            (h as RefEventHandler<T>)?.Add(handler);
        }
        /// <summary>
        /// 取消订阅事件
        /// </summary>
        /// <typeparam name="T">事件类型</typeparam>
        /// <param name="handler">事件处理器</param>
        public void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            if (handler == null)
            {
                Debug.LogError("[EventBus] Cannot unsubscribe null handler");
                return;
            }
            
            var eventType = typeof(T);
            
            if (_eventHandlers.TryGetValue(eventType, out var handlerWrapper))
            {
                ((EventHandler<T>)handlerWrapper).Remove(handler);
                
                if (_enableDebugLogging)
                {
                    Debug.Log($"[EventBus] Unsubscribed {handler.Method.Name} from event {eventType.Name}");
                }
                
                // 如果没有处理器了，清理资源
                if (handlerWrapper.Count == 0)
                {
                    _eventHandlers.Remove(eventType);
                    if (_enableStatistics)
                    {
                        _statistics.Remove(eventType);
                    }
                    
                    if (_enableDebugLogging)
                    {
                        Debug.Log($"[EventBus] Removed empty event type {eventType.Name}");
                    }
                }
            }
            else if (_enableDebugLogging)
            {
                Debug.LogWarning($"[EventBus] Trying to unsubscribe from non-existent event {eventType.Name}");
            }
        }
        
        /// <summary>取消订阅 ref 事件</summary>
        public void UnsubscribeRef<T>(RefAction<T> handler) where T : struct
        {
            if (_refHandlers.TryGetValue(typeof(T), out var h))
                (h as RefEventHandler<T>)?.Remove(handler);
        }
        /// <summary>
        /// 触发事件
        /// </summary>
        /// <typeparam name="T">事件类型</typeparam>
        /// <param name="eventData">事件数据</param>
        public void Trigger<T>(T eventData) where T : struct
        {
            var eventType = typeof(T);
            
            if (_eventHandlers.TryGetValue(eventType, out var handlerWrapper))
            {
                var stopwatch = _enableStatistics ? Stopwatch.StartNew() : null;
                
                try
                {
                    ((EventHandler<T>)handlerWrapper).Invoke(eventData);
                }
                finally
                {
                    if (_enableStatistics && stopwatch != null)
                    {
                        stopwatch.Stop();
                        var elapsedMs = (int)stopwatch.ElapsedMilliseconds;
                        
                        if (_statistics.TryGetValue(eventType, out var stats))
                        {
                            stats.InvokeCount++;
                            stats.TotalInvokeTimeMs += elapsedMs;
                            
                            if (elapsedMs > _slowEventThresholdMs)
                            {
                                Debug.LogWarning($"[EventBus] Slow event detected: {eventType.Name} took {elapsedMs}ms");
                            }
                        }
                    }
                }
                
                if (_enableDebugLogging)
                {
                    Debug.Log($"[EventBus] Triggered event {eventType.Name}");
                }
            }
            else if (_enableDebugLogging)
            {
                Debug.LogWarning($"[EventBus] No handlers registered for event {eventType.Name}");
            }
        }
        /// <summary>触发 ref 事件（零 GC）</summary>
        public void TriggerRef<T>(ref T eventData) where T : struct
        {
            var t = typeof(T);
            if (_refHandlers.TryGetValue(t, out var h))
                (h as RefEventHandler<T>)?.Invoke(ref eventData);
        }
        /// <summary>
        /// 清理特定事件的所有处理器
        /// </summary>
        /// <typeparam name="T">事件类型</typeparam>
        public void ClearEvent<T>() where T : struct
        {
            var eventType = typeof(T);
            
            if (_eventHandlers.TryGetValue(eventType, out var handlerWrapper))
            {
                handlerWrapper.Clear();
                _eventHandlers.Remove(eventType);
                
                if (_enableStatistics)
                {
                    _statistics.Remove(eventType);
                }
                
                if (_enableDebugLogging)
                {
                    Debug.Log($"[EventBus] Cleared all handlers for event {eventType.Name}");
                }
            }
        }
        
        /// <summary>
        /// 清理所有事件
        /// </summary>
        public void ClearAllEvents()
        {
            foreach (var handlerWrapper in _eventHandlers.Values)
            {
                handlerWrapper.Clear();
            }
            
            _eventHandlers.Clear();
            _statistics.Clear();
            
            foreach (var v in _refHandlers.Values) v.Clear();
            _refHandlers.Clear();
            
            if (_enableDebugLogging)
            {
                Debug.Log("[EventBus] Cleared all events and handlers");
            }
        }
        
        /// <summary>
        /// 获取事件的处理器数量
        /// </summary>
        /// <typeparam name="T">事件类型</typeparam>
        /// <returns>处理器数量</returns>
        public int GetHandlerCount<T>() where T : struct
        {
            var eventType = typeof(T);
            return _eventHandlers.TryGetValue(eventType, out var handlerWrapper) ? handlerWrapper.Count : 0;
        }
        
        /// <summary>
        /// 获取事件的处理器数量
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <returns>处理器数量</returns>
        public int GetHandlerCount(Type eventType)
        {
            if (eventType == null) throw new ArgumentNullException(nameof(eventType));
            return _eventHandlers.TryGetValue(eventType, out var handlerWrapper) ? handlerWrapper.Count : 0;
        }
        
        /// <summary>
        /// 检查是否有事件处理器注册
        /// </summary>
        /// <typeparam name="T">事件类型</typeparam>
        /// <returns>是否有处理器注册</returns>
        public bool HasHandler<T>() where T : struct
        {
            return _eventHandlers.ContainsKey(typeof(T));
        }
        
        #endregion
        
        #region 生命周期方法
        
        protected override void Awake()
        {
            base.Awake();
            
            if (_enableDebugLogging)
            {
                Debug.Log("[EventBus] EventBus initialized");
            }
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            ClearAllEvents();
            
            if (_enableDebugLogging)
            {
                Debug.Log("[EventBus] EventBus destroyed");
            }
        }
        
        #endregion
        
        #region 调试和工具方法
        
        /// <summary>
        /// 打印事件总线状态
        /// </summary>
        public void PrintStatus()
        {
            Debug.Log("=== EventBus Status ===");
            Debug.Log($"Total event types: {_eventHandlers.Count}");
            Debug.Log($"Statistics enabled: {_enableStatistics}");
            Debug.Log($"Debug logging enabled: {_enableDebugLogging}");
            
            foreach (var kvp in _eventHandlers)
            {
                var eventType = kvp.Key;
                var handlerCount = kvp.Value.Count;
                
                if (_statistics.TryGetValue(eventType, out var stats))
                {
                    Debug.Log($"Event: {eventType.Name}, Handlers: {handlerCount}, Invokes: {stats.InvokeCount}, AvgTime: {stats.AverageInvokeTimeMs:F2}ms");
                }
                else
                {
                    Debug.Log($"Event: {eventType.Name}, Handlers: {handlerCount}");
                }
            }
            
            Debug.Log("=======================");
        }
        #endregion
    }
    
    #region 便利的静态扩展
    
    /// <summary>
    /// EventBus的静态扩展方法
    /// </summary>
    public static class EventBusExtensions
    {
        /// <summary>
        /// 订阅事件（便利方法）
        /// </summary>
        public static void Subscribe<T>(this Action<T> handler) where T : struct
        {
            if (EventBus.Instance != null)
            {
                EventBus.Instance.Subscribe(handler);
            }
            else
            {
                Debug.LogError("[EventBus] EventBus instance is null");
            }
        }
        
        /// <summary>
        /// 取消订阅事件（便利方法）
        /// </summary>
        public static void Unsubscribe<T>(this Action<T> handler) where T : struct
        {
            if (EventBus.Instance != null)
            {
                EventBus.Instance.Unsubscribe(handler);
            }
        }
    }
    
    #endregion
}