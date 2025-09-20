#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using AsakiFramework;

namespace AsakiFramework.Editor
{
    /// <summary>
    /// EventBus调试器编辑器窗口
    /// </summary>
    public class EventBusEditorWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private bool _showStatistics = true;
        private bool _showHandlers = true;
        private string _searchFilter = "";
        private bool _autoRefresh = true;
        private float _lastRefreshTime;
        private const float REFRESH_INTERVAL = 1f;
        
        [MenuItem("AsakiFramework/事件总线调试器")]
        public static void ShowWindow()
        {
            GetWindow<EventBusEditorWindow>("EventBus Debugger");
        }
        
        private void OnEnable()
        {
            _lastRefreshTime = Time.realtimeSinceStartup;
        }
        
        private void OnGUI()
        {
            try
            {
                if (EventBus.Instance == null)
                {
                    DrawNoEventBusPanel();
                    return;
                }
                
                DrawHeader();
                DrawSettingsPanel();
                DrawControlButtons();
                DrawEventList();
                
                HandleAutoRefresh();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EventBusEditorWindow] GUI Error: {ex.Message}");
                EditorGUILayout.HelpBox($"GUI Error: {ex.Message}", MessageType.Error);
            }
        }
        
        private void DrawNoEventBusPanel()
        {
            EditorGUILayout.HelpBox("EventBus instance not found in the scene.", MessageType.Warning);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create EventBus"))
            {
                CreateEventBusInstance();
            }
            if (GUILayout.Button("Refresh"))
            {
                Repaint();
            }
            EditorGUILayout.EndHorizontal();
        }
        
        private void CreateEventBusInstance()
        {
            var go = new GameObject("EventBus");
            go.AddComponent<EventBus>();
            Selection.activeGameObject = go;
            EditorGUIUtility.PingObject(go);
        }
        
        private void DrawHeader()
        {
            GUILayout.Label("EventBus Debugger", EditorStyles.boldLabel);
            GUILayout.Label($"Total Events: {EventBus.Instance.RegisteredEventTypes.Length}", EditorStyles.miniLabel);
            EditorGUILayout.Space();
        }
        
        private void DrawSettingsPanel()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Settings", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            
            EventBus.Instance.EnableDebugLogging = EditorGUILayout.Toggle("Enable Debug Logging", EventBus.Instance.EnableDebugLogging);
            EventBus.Instance.EnableStatistics = EditorGUILayout.Toggle("Enable Statistics", EventBus.Instance.EnableStatistics);
            _showStatistics = EditorGUILayout.Toggle("Show Statistics", _showStatistics);
            _showHandlers = EditorGUILayout.Toggle("Show Handler Count", _showHandlers);
            _autoRefresh = EditorGUILayout.Toggle("Auto Refresh", _autoRefresh);
            
            if (EditorGUI.EndChangeCheck())
            {
                Repaint();
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
        
        private void DrawControlButtons()
        {
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Print Status"))
            {
                EventBus.Instance.PrintStatus();
            }
            
            if (GUILayout.Button("Clear All Events"))
            {
                if (EditorUtility.DisplayDialog("Confirm Clear", "Are you sure you want to clear all events and handlers?", "Yes", "No"))
                {
                    EventBus.Instance.ClearAllEvents();
                }
            }
            
            if (GUILayout.Button("Force Refresh"))
            {
                Repaint();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(2);
            
            // 搜索框
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Search:", GUILayout.Width(50));
            EditorGUI.BeginChangeCheck();
            _searchFilter = EditorGUILayout.TextField(_searchFilter, GUILayout.ExpandWidth(true));
            if (EditorGUI.EndChangeCheck())
            {
                Repaint();
            }
            if (GUILayout.Button("Clear", GUILayout.Width(50)))
            {
                _searchFilter = "";
                GUI.FocusControl(null);
                Repaint();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
        }
        
        private void DrawEventList()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            var eventTypes = GetFilteredEventTypes();
            
            if (eventTypes.Length > 0)
            {
                GUILayout.Label($"Registered Events ({eventTypes.Length})", EditorStyles.boldLabel);
                
                foreach (var eventType in eventTypes)
                {
                    DrawEventItem(eventType);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(_searchFilter))
                {
                    EditorGUILayout.HelpBox("No events registered.", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox("No events match the search filter.", MessageType.Info);
                }
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        private Type[] GetFilteredEventTypes()
        {
            var allTypes = EventBus.Instance.RegisteredEventTypes;
            
            if (string.IsNullOrEmpty(_searchFilter))
            {
                return allTypes;
            }
            
            return allTypes.Where(t => t.Name.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0).ToArray();
        }
        
        private void DrawEventItem(Type eventType)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // 事件标题行
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(eventType.Name, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            
            if (_showHandlers)
            {
                GUILayout.Label($"Handlers: {EventBus.Instance.GetHandlerCount(eventType)}", EditorStyles.miniLabel);
            }
            EditorGUILayout.EndHorizontal();
            
            // 统计信息
            if (_showStatistics && EventBus.Instance.EnableStatistics)
            {
                DrawStatistics(eventType);
            }
            
            // 操作按钮
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Trigger Test", GUILayout.Width(80)))
            {
                TriggerTestEvent(eventType);
            }
            
            if (GUILayout.Button("Clear Event", GUILayout.Width(80)))
            {
                ClearEvent(eventType);
            }
            
            if (GUILayout.Button("Copy Name", GUILayout.Width(80)))
            {
                EditorGUIUtility.systemCopyBuffer = eventType.Name;
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }
        
        private void DrawStatistics(Type eventType)
        {
            var stats = EventBus.Instance.GetStatistics();
            var eventStats = stats.FirstOrDefault(s => s.EventType == eventType.Name);
            
            if (eventStats != null)
            {
                EditorGUILayout.BeginVertical(EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"Invokes: {eventStats.InvokeCount}");
                EditorGUILayout.LabelField($"Avg Time: {eventStats.AverageInvokeTimeMs:F2}ms");
                
                if (eventStats.AverageInvokeTimeMs > 5f)
                {
                    EditorGUILayout.HelpBox("Slow event detected!", MessageType.Warning);
                }
                
                EditorGUILayout.EndVertical();
            }
        }
        
        private void TriggerTestEvent(Type eventType)
        {
            try
            {
                // 创建默认事件实例
                var instance = Activator.CreateInstance(eventType);
                
                // 使用反射调用泛型方法
                var triggerMethod = typeof(EventBus).GetMethod("Trigger").MakeGenericMethod(eventType);
                triggerMethod.Invoke(EventBus.Instance, new object[] { instance });
                
                Debug.Log($"[EventBusEditor] Triggered test event: {eventType.Name}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[EventBusEditor] Could not trigger test event {eventType.Name}: {ex.Message}");
            }
        }
        
        private void ClearEvent(Type eventType)
        {
            try
            {
                var clearMethod = typeof(EventBus).GetMethod("ClearEvent").MakeGenericMethod(eventType);
                clearMethod.Invoke(EventBus.Instance, null);
                
                Debug.Log($"[EventBusEditor] Cleared event: {eventType.Name}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EventBusEditor] Error clearing event {eventType.Name}: {ex.Message}");
            }
        }
        
        private void HandleAutoRefresh()
        {
            if (_autoRefresh && Time.realtimeSinceStartup - _lastRefreshTime > REFRESH_INTERVAL)
            {
                Repaint();
                _lastRefreshTime = Time.realtimeSinceStartup;
            }
        }
        
        // 移除Update方法，避免编辑器性能问题
        // 使用HandleAutoRefresh进行定时刷新
    }
    
    /// <summary>
    /// EventBus的快速检查器（在Inspector中显示）
    /// </summary>
    [CustomEditor(typeof(EventBus))]
    public class EventBusInspector : UnityEditor.Editor
    {
        private bool _showEvents = true;
        private Vector2 _scrollPosition;
        
        public override void OnInspectorGUI()
        {
            var eventBus = (EventBus)target;
            
            try
            {
                // 基本信息
                EditorGUILayout.HelpBox("Global Event Bus - Type-safe, delegate-driven event system", MessageType.Info);
                
                EditorGUILayout.Space();
                
                // 快速设置
                EditorGUILayout.LabelField("Quick Settings", EditorStyles.boldLabel);
                eventBus.EnableDebugLogging = EditorGUILayout.Toggle("Debug Logging", eventBus.EnableDebugLogging);
                eventBus.EnableStatistics = EditorGUILayout.Toggle("Statistics", eventBus.EnableStatistics);
                
                EditorGUILayout.Space();
                
                // 事件统计
                EditorGUILayout.LabelField("Event Statistics", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Total Events:", eventBus.RegisteredEventTypes.Length.ToString());
                
                // 快速操作
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Print Status"))
                {
                    eventBus.PrintStatus();
                }
                
                if (GUILayout.Button("Open Debugger"))
                {
                    EventBusEditorWindow.ShowWindow();
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space();
                
                // 事件列表
                _showEvents = EditorGUILayout.Foldout(_showEvents, "Registered Events", true);
                if (_showEvents)
                {
                    _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(150));
                    
                    var eventTypes = eventBus.RegisteredEventTypes;
                    if (eventTypes.Length > 0)
                    {
                        foreach (var eventType in eventTypes)
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label($"• {eventType.Name}", EditorStyles.miniLabel);
                            GUILayout.FlexibleSpace();
                            GUILayout.Label($"({eventBus.GetHandlerCount(eventType)} handlers)", EditorStyles.miniLabel);
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("No events registered", EditorStyles.miniLabel);
                    }
                    
                    EditorGUILayout.EndScrollView();
                }
                
                // 统计信息
                if (eventBus.EnableStatistics)
                {
                    var stats = eventBus.GetStatistics();
                    if (stats.Length > 0)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("Performance Statistics", EditorStyles.boldLabel);
                        
                        foreach (var stat in stats.Take(5)) // 只显示前5个
                        {
                            EditorGUILayout.LabelField($"{stat.EventType}:", $"{stat.InvokeCount} invokes, {stat.AverageInvokeTimeMs:F2}ms avg");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EventBusInspector] Inspector Error: {ex.Message}");
                EditorGUILayout.HelpBox($"Inspector Error: {ex.Message}", MessageType.Error);
            }
        }
    }
}
#endif