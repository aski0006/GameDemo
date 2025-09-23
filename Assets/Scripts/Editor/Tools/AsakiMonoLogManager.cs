using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AsakiFramework.Editor
{
    /// <summary>
    /// AsakiMono 日志管理器
    /// 统一管理所有继承 AsakiMono 的脚本的日志开关
    /// </summary>
    public class AsakiMonoLogManager : EditorWindow
    {
        private Vector2 _scrollPosition;
        private bool _showEnabledOnly = false;
        private string _searchFilter = "";
        private Dictionary<AsakiMono, bool> _cachedMonoStates = new Dictionary<AsakiMono, bool>();
        private float _lastRefreshTime;
        private bool _autoRefresh = true;

        [MenuItem("Tools/AsakiMono 日志管理器")]
        public static void ShowWindow()
        {
            var window = GetWindow<AsakiMonoLogManager>("AsakiMono 日志管理");
            window.minSize = new Vector2(500, 400);
            window.Show();
        }

        private void OnGUI()
        {
            DrawToolbar();
            DrawLogControls();
            DrawMonoList();
        }

        private void DrawToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                // 自动刷新开关
                _autoRefresh = GUILayout.Toggle(_autoRefresh, "自动刷新", EditorStyles.toolbarButton, GUILayout.Width(80));

                GUILayout.Space(10);

                // 手动刷新按钮
                if (GUILayout.Button("刷新", EditorStyles.toolbarButton, GUILayout.Width(60)))
                {
                    RefreshMonoList();
                }

                GUILayout.FlexibleSpace();

                // 筛选选项
                _showEnabledOnly = GUILayout.Toggle(_showEnabledOnly, "仅显示启用", EditorStyles.toolbarButton, GUILayout.Width(80));

                // 搜索框
                GUILayout.Label("搜索:", GUILayout.Width(40));
                _searchFilter = GUILayout.TextField(_searchFilter, EditorStyles.toolbarSearchField, GUILayout.Width(150));
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();
        }

        private void DrawLogControls()
        {
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("全部开启日志", GUILayout.Height(25)))
                {
                    SetAllLogsEnabled(true);
                }

                if (GUILayout.Button("全部关闭日志", GUILayout.Height(25)))
                {
                    SetAllLogsEnabled(false);
                }

                if (GUILayout.Button("开启选中对象日志", GUILayout.Height(25)))
                {
                    SetSelectedLogsEnabled(true);
                }

                if (GUILayout.Button("关闭选中对象日志", GUILayout.Height(25)))
                {
                    SetSelectedLogsEnabled(false);
                }
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();
        }

        private void DrawMonoList()
        {
            // 统计信息
            var allMonos = GetAllAsakiMonos();
            var enabledCount = allMonos.Count(m => GetLogEnabledState(m));
            var disabledCount = allMonos.Count - enabledCount;

            EditorGUILayout.LabelField($"总计: {allMonos.Count} 个 AsakiMono | 开启: {enabledCount} | 关闭: {disabledCount}", EditorStyles.boldLabel);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            {
                bool foundAny = false;

                foreach (var mono in allMonos)
                {
                    if (mono == null) continue;

                    var gameObject = mono.gameObject;
                    var monoName = $"{gameObject.name} ({mono.GetType().Name})";
                    var sceneName = gameObject.scene.name;
                    var fullPath = GetGameObjectPath(gameObject);

                    // 应用筛选
                    if (_showEnabledOnly && !GetLogEnabledState(mono)) continue;
                    if (!string.IsNullOrEmpty(_searchFilter) &&
                        !monoName.ToLower().Contains(_searchFilter.ToLower()) &&
                        !sceneName.ToLower().Contains(_searchFilter.ToLower()) &&
                        !fullPath.ToLower().Contains(_searchFilter.ToLower()))
                        continue;

                    foundAny = true;
                    DrawMonoItem(mono, monoName, sceneName, fullPath);
                }

                if (!foundAny)
                {
                    EditorGUILayout.HelpBox("没有找到匹配的 AsakiMono 组件", MessageType.Info);
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawMonoItem(AsakiMono mono, string monoName, string sceneName, string fullPath)
        {
            GUILayout.BeginVertical("box");
            {
                // 第一行：开关和名称
                GUILayout.BeginHorizontal();
                {
                    bool isEnabled = GetLogEnabledState(mono);
                    bool newEnabled = EditorGUILayout.Toggle(isEnabled, GUILayout.Width(20));

                    if (newEnabled != isEnabled)
                    {
                        SetLogEnabledState(mono, newEnabled);
                    }

                    EditorGUILayout.LabelField(monoName, EditorStyles.boldLabel);

                    GUILayout.FlexibleSpace();

                    // 场景名称
                    EditorGUILayout.LabelField(sceneName, GUILayout.Width(100));

                    // 选择按钮
                    if (GUILayout.Button("选择", GUILayout.Width(50)))
                    {
                        Selection.activeGameObject = mono.gameObject;
                        EditorGUIUtility.PingObject(mono.gameObject);
                    }
                }
                GUILayout.EndHorizontal();

                // 第二行：路径信息
                EditorGUILayout.LabelField(fullPath, EditorStyles.miniLabel);

                // 第三行：组件信息
                GUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField($"Enabled: {mono.enabled}", GUILayout.Width(80));
                    EditorGUILayout.LabelField($"Active: {mono.gameObject.activeInHierarchy}", GUILayout.Width(80));

                    GUILayout.FlexibleSpace();

                    // 快速操作按钮
                    if (GUILayout.Button("开启", GUILayout.Width(40)))
                    {
                        SetLogEnabledState(mono, true);
                    }
                    if (GUILayout.Button("关闭", GUILayout.Width(40)))
                    {
                        SetLogEnabledState(mono, false);
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            GUILayout.Space(2);
        }

        private List<AsakiMono> GetAllAsakiMonos()
        {
            return FindObjectsOfType<AsakiMono>(true).ToList();
        }

        private bool GetLogEnabledState(AsakiMono mono)
        {
            if (_cachedMonoStates.TryGetValue(mono, out bool state))
            {
                return state;
            }


            state = mono.IsLogEnabled;
            _cachedMonoStates[mono] = state;
            return state;
            return false;
        }

        private void SetLogEnabledState(AsakiMono mono, bool enabled)
        {
            mono.IsLogEnabled = enabled;
            _cachedMonoStates[mono] = enabled;

            // 标记为脏，确保更改被保存
            EditorUtility.SetDirty(mono);

            // 如果是预制件实例，也标记预制件为脏
            if (PrefabUtility.IsPartOfAnyPrefab(mono))
            {
                var prefabRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(mono.gameObject);
                if (prefabRoot != null)
                {
                    EditorUtility.SetDirty(prefabRoot);
                }
            }

        }

        private void SetAllLogsEnabled(bool enabled)
        {
            var allMonos = GetAllAsakiMonos();
            foreach (var mono in allMonos)
            {
                SetLogEnabledState(mono, enabled);
            }

            Debug.Log($"{(enabled ? "开启" : "关闭")}了 {allMonos.Count} 个 AsakiMono 组件的日志");
        }

        private void SetSelectedLogsEnabled(bool enabled)
        {
            var selectedMonos = Selection.gameObjects
                .SelectMany(go => go.GetComponentsInChildren<AsakiMono>(true))
                .ToList();

            foreach (var mono in selectedMonos)
            {
                SetLogEnabledState(mono, enabled);
            }

            if (selectedMonos.Count > 0)
            {
                Debug.Log($"{(enabled ? "开启" : "关闭")}了 {selectedMonos.Count} 个选中对象的 AsakiMono 日志");
            }
        }

        private string GetGameObjectPath(GameObject obj)
        {
            string path = "/" + obj.name;
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = "/" + obj.name + path;
            }
            return path;
        }

        private void RefreshMonoList()
        {
            _cachedMonoStates.Clear();
            Repaint();
        }

        private void Update()
        {
            if (_autoRefresh && Time.realtimeSinceStartup - _lastRefreshTime > 1f)
            {
                RefreshMonoList();
                _lastRefreshTime = Time.realtimeSinceStartup;
            }
        }

        private void OnSelectionChange()
        {
            Repaint();
        }

        private void OnFocus()
        {
            RefreshMonoList();
        }

        private void OnHierarchyChange()
        {
            RefreshMonoList();
        }
    }

    /// <summary>
    /// 在 Inspector 中添加快捷控制按钮
    /// </summary>
    [CustomEditor(typeof(AsakiMono), true)]
    public class AsakiMonoInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var asakiMono = (AsakiMono)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("日志控制", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();
            {
                // 获取当前日志状态
                bool currentState = asakiMono.IsLogEnabled;

                if (GUILayout.Button(currentState ? "关闭日志" : "开启日志"))
                {
                    bool newState = !currentState;
                    asakiMono.IsLogEnabled = newState;
                    EditorUtility.SetDirty(asakiMono);
                    Debug.Log($"{asakiMono.name} 的日志已{(newState ? "开启" : "关闭")}");
                }

                if (GUILayout.Button("打开日志管理器"))
                {
                    AsakiMonoLogManager.ShowWindow();
                }
            }
            GUILayout.EndHorizontal();

            // 显示当前状态
            var style = new GUIStyle(EditorStyles.helpBox);
            style.normal.textColor = asakiMono.IsLogEnabled ? Color.green : Color.gray;
            EditorGUILayout.LabelField($"当前日志状态: {(asakiMono.IsLogEnabled ? "开启" : "关闭")}", style);
        }
    }
}
