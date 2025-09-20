using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AsakiFramework
{
    /// <summary>
    /// EventBus使用示例
    /// </summary>
    public class EventBusExamples : MonoBehaviour
    {
        #region 示例事件定义
        
        // 玩家相关事件
        public struct PlayerDamagedEvent
        {
            public int PlayerId;
            public float Damage;
            public Vector3 HitPosition;
        }
        
        public struct PlayerHealedEvent
        {
            public int PlayerId;
            public float HealAmount;
        }
        
        // 游戏状态事件
        public struct GameStartEvent
        {
            public string LevelName;
            public int PlayerCount;
        }
        
        public struct GameEndEvent
        {
            public bool IsVictory;
            public float Duration;
        }
        
        // UI事件
        public struct ScoreChangedEvent
        {
            public int NewScore;
            public int OldScore;
        }
        
        public struct UIOpenEvent
        {
            public string UIName;
        }
        
        // 卡牌游戏特定事件
        public struct CardPlayedEvent
        {
            public int CardId;
            public int PlayerId;
            public Vector3 PlayPosition;
        }
        
        public struct CardDrawnEvent
        {
            public int CardId;
            public int PlayerId;
        }
        
        #endregion
        
        #region 生命周期方法
        
        private void OnEnable()
        {
            // 订阅事件
            SubscribeToEvents();
        }
        
        private void OnDisable()
        {
            // 取消订阅事件
            UnsubscribeFromEvents();
        }
        
        private void Start()
        {
            // 触发一些测试事件
            TriggerTestEvents();
        }
        
        #endregion
        
        #region 事件订阅
        
        private void SubscribeToEvents()
        {
            // 方法1: 直接通过EventBus实例订阅
            if (EventBus.Instance != null)
            {
                EventBus.Instance.Subscribe<PlayerDamagedEvent>(OnPlayerDamaged);
                EventBus.Instance.Subscribe<GameStartEvent>(OnGameStart);
                EventBus.Instance.Subscribe<ScoreChangedEvent>(OnScoreChanged);
                EventBus.Instance.Subscribe<CardPlayedEvent>(OnCardPlayed);
            }
            
            // 方法2: 使用扩展方法订阅（更简洁）
            ((Action<PlayerHealedEvent>)OnPlayerHealed).Subscribe<PlayerHealedEvent>();
            ((Action<GameEndEvent>)OnGameEnd).Subscribe<GameEndEvent>();
            ((Action<UIOpenEvent>)OnUIOpen).Subscribe<UIOpenEvent>();
        }
        
        private void UnsubscribeFromEvents()
        {
            // 方法1: 直接通过EventBus实例取消订阅
            if (EventBus.Instance != null)
            {
                EventBus.Instance.Unsubscribe<PlayerDamagedEvent>(OnPlayerDamaged);
                EventBus.Instance.Unsubscribe<GameStartEvent>(OnGameStart);
                EventBus.Instance.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);
                EventBus.Instance.Unsubscribe<CardPlayedEvent>(OnCardPlayed);
            }
            
            // 方法2: 使用扩展方法取消订阅
            ((Action<PlayerHealedEvent>)OnPlayerHealed).Unsubscribe<PlayerHealedEvent>();
            ((Action<GameEndEvent>)OnGameEnd).Unsubscribe<GameEndEvent>();
            ((Action<UIOpenEvent>)OnUIOpen).Unsubscribe<UIOpenEvent>();
        }
        
        #endregion
        
        #region 事件处理器
        
        private void OnPlayerDamaged(PlayerDamagedEvent evt)
        {
            Debug.Log($"[EventBusExample] Player {evt.PlayerId} damaged for {evt.Damage} at position {evt.HitPosition}");
        }
        
        private void OnPlayerHealed(PlayerHealedEvent evt)
        {
            Debug.Log($"[EventBusExample] Player {evt.PlayerId} healed for {evt.HealAmount}");
        }
        
        private void OnGameStart(GameStartEvent evt)
        {
            Debug.Log($"[EventBusExample] Game started on level {evt.LevelName} with {evt.PlayerCount} players");
        }
        
        private void OnGameEnd(GameEndEvent evt)
        {
            Debug.Log($"[EventBusExample] Game ended. Victory: {evt.IsVictory}, Duration: {evt.Duration}s");
        }
        
        private void OnScoreChanged(ScoreChangedEvent evt)
        {
            Debug.Log($"[EventBusExample] Score changed from {evt.OldScore} to {evt.NewScore}");
        }
        
        private void OnUIOpen(UIOpenEvent evt)
        {
            Debug.Log($"[EventBusExample] UI opened: {evt.UIName}");
        }
        
        private void OnCardPlayed(CardPlayedEvent evt)
        {
            Debug.Log($"[EventBusExample] Card {evt.CardId} played by player {evt.PlayerId} at position {evt.PlayPosition}");
        }
        
        #endregion
        
        #region 测试事件触发
        
        private void TriggerTestEvents()
        {
            // 延迟触发一些事件以演示功能
            Invoke("TriggerDelayedEvents", 2f);
        }
        
        private void TriggerDelayedEvents()
        {
            if (EventBus.Instance != null)
            {
                // 触发游戏开始事件
                EventBus.Instance.Trigger(new GameStartEvent
                {
                    LevelName = "Forest Level",
                    PlayerCount = 2
                });
                
                // 触发玩家伤害事件
                EventBus.Instance.Trigger(new PlayerDamagedEvent
                {
                    PlayerId = 1,
                    Damage = 25.5f,
                    HitPosition = new Vector3(10, 0, 5)
                });
                
                // 触发分数变化事件
                EventBus.Instance.Trigger(new ScoreChangedEvent
                {
                    OldScore = 100,
                    NewScore = 150
                });
                
                // 触发卡牌游戏事件
                EventBus.Instance.Trigger(new CardPlayedEvent
                {
                    CardId = 42,
                    PlayerId = 1,
                    PlayPosition = new Vector3(0, 0, 0)
                });
                
                // 打印事件总线状态
                EventBus.Instance.PrintStatus();
            }
        }
        
        #endregion
        
        #region 更新循环中的事件触发
        
        private float _timer = 0f;
        private int _playerScore = 0;
        
        private void Update()
        {
            _timer += Time.deltaTime;
            
            // 每5秒触发一次分数增加事件
            if (_timer >= 5f)
            {
                _timer = 0f;
                
                if (EventBus.Instance != null)
                {
                    var oldScore = _playerScore;
                    _playerScore += Random.Range(10, 50);
                    
                    EventBus.Instance.Trigger(new ScoreChangedEvent
                    {
                        OldScore = oldScore,
                        NewScore = _playerScore
                    });
                }
            }
            
            // 按空格键触发UI打开事件
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (EventBus.Instance != null)
                {
                    EventBus.Instance.Trigger(new UIOpenEvent
                    {
                        UIName = "Settings Menu"
                    });
                }
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// EventBus最佳实践示例
    /// </summary>
    public class EventBusBestPractices : MonoBehaviour
    {
        #region 最佳实践建议
        
        // 1. 使用结构体定义事件数据（避免GC分配）
        public struct OptimizedEvent
        {
            public int Data;
            public float Time;
            // 避免使用引用类型字段
        }
        
        // 2. 事件命名约定：以Event结尾
        public struct PlayerMoveEvent
        {
            public Vector3 Position;
            public Vector3 Velocity;
        }
        
        // 3. 事件应该是不变的（只读属性）
        public struct ImmutableEvent
        {
            public readonly int Id;
            public readonly string Name;
            
            public ImmutableEvent(int id, string name)
            {
                Id = id;
                Name = name;
            }
        }
        
        #endregion
        
        private void OnEnable()
        {
            // 总是订阅事件
            EventBus.Instance?.Subscribe<OptimizedEvent>(OnOptimizedEvent);
        }
        
        private void OnDisable()
        {
            // 总是取消订阅事件
            EventBus.Instance?.Unsubscribe<OptimizedEvent>(OnOptimizedEvent);
        }
        
        private void OnOptimizedEvent(OptimizedEvent evt)
        {
            // 快速处理事件，避免长时间操作
            // 如果需要复杂处理，考虑使用协程或任务系统
        }
        
        /// <summary>
        /// 事件触发帮助类
        /// </summary>
        public static class EventHelper
        {
            public static void TriggerPlayerMove(Vector3 position, Vector3 velocity)
            {
                EventBus.Instance?.Trigger(new PlayerMoveEvent
                {
                    Position = position,
                    Velocity = velocity
                });
            }
            
            public static void TriggerOptimizedEvent(int data)
            {
                EventBus.Instance?.Trigger(new OptimizedEvent
                {
                    Data = data,
                    Time = Time.time
                });
            }
        }
    }
}