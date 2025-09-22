using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AsakiFramework.Editor
{
    public class SceneObjectFinder : EditorWindow
    {
        [MenuItem("Asaki Tools/场景对象查找器 &f")]
        public static void ShowWindow()
        {
            var window = GetWindow<SceneObjectFinder>();
            window.titleContent = new GUIContent("场景查找器");
            window.minSize = new Vector2(300, 400);
            window.Show();
        }

        private string searchText = "";
        private Vector2 scrollPosition;
        private List<GameObject> searchResults = new List<GameObject>();
        private SearchField searchField;
        private bool includeInactive = true;
        private bool searchInChildren = true;

        private void OnEnable()
        {
            searchField = new SearchField();
        }

        private void OnGUI()
        {
            DrawSearchBar();
            DrawOptions();
            DrawResults();
        }

        private void DrawSearchBar()
        {
            EditorGUILayout.Space(5);

            using (new GUILayout.HorizontalScope())
            {
                GUI.SetNextControlName("SearchTextField");
                string newSearchText = searchField.OnGUI(
                    EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true)),
                    searchText);

                if (newSearchText != searchText)
                {
                    searchText = newSearchText;
                    PerformSearch();
                }

                if (GUILayout.Button("刷新", GUILayout.Width(60)))
                {
                    PerformSearch();
                }
            }

            EditorGUILayout.Space(5);
        }

        private void DrawOptions()
        {
            using (new GUILayout.HorizontalScope())
            {
                includeInactive = EditorGUILayout.ToggleLeft("包含未激活", includeInactive, GUILayout.Width(100));
                searchInChildren = EditorGUILayout.ToggleLeft("搜索子对象", searchInChildren, GUILayout.Width(100));

                if (GUILayout.Button("清空", GUILayout.Width(60)))
                {
                    searchText = "";
                    searchResults.Clear();
                }
            }

            EditorGUILayout.Space(5);
        }

        private void DrawResults()
        {
            if (string.IsNullOrEmpty(searchText))
            {
                EditorGUILayout.HelpBox("输入关键词开始搜索...", MessageType.Info);
                return;
            }

            if (searchResults.Count == 0)
            {
                EditorGUILayout.HelpBox("未找到匹配的对象", MessageType.Warning);
                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.LabelField($"找到 {searchResults.Count} 个结果", EditorStyles.boldLabel);
            EditorGUILayout.Space(2);

            // 缓存 16×16 的默认图标
            Texture2D icon = EditorGUIUtility.FindTexture("GameObject Icon");

            foreach (var gameObject in searchResults)
            {
                if (!gameObject) continue;

                // 单行高度 18，整体更紧凑
                var lineRect = EditorGUILayout.GetControlRect(false, 18);

                // 图标区域 16×16
                var iconRect = new Rect(lineRect.x, lineRect.y, 16, 16);
                GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);

                // 名称按钮（可点击选中）
                var nameRect = new Rect(lineRect.x + 20, lineRect.y, lineRect.width - 70, 18);
                if (GUI.Button(nameRect, gameObject.name, EditorStyles.label))
                    SelectAndPingObject(gameObject);

                // 路径后缀
                var pathRect = new Rect(nameRect.xMax + 5, lineRect.y, 30, 18);
                EditorGUI.LabelField(pathRect, GetGameObjectPath(gameObject), EditorStyles.miniLabel);

                // 快速选择按钮
                var btnRect = new Rect(lineRect.xMax - 45, lineRect.y, 40, 18);
                if (GUI.Button(btnRect, "选择"))
                    SelectAndPingObject(gameObject);
            }

            EditorGUILayout.EndScrollView();
        }

        private void PerformSearch()
        {
            searchResults.Clear();

            if (string.IsNullOrEmpty(searchText))
                return;

            // 获取场景中的所有游戏对象
            var allObjects = Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(go => go.scene.isLoaded &&
                    (includeInactive || go.activeInHierarchy) &&
                    !IsEditorOnlyObject(go))
                .ToList();

            foreach (var obj in allObjects)
            {
                if (IsMatch(obj, searchText))
                {
                    searchResults.Add(obj);
                }
            }

            // 按名称长度排序（更精确的匹配排在前面）
            searchResults = searchResults
                .OrderBy(go => go.name.Length)
                .ThenBy(go => go.name)
                .ToList();
        }

        private bool IsMatch(GameObject gameObject, string searchTerm)
        {
            if (gameObject.name.IndexOf(searchTerm, System.StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            if (searchInChildren)
            {
                // 检查子对象名称
                foreach (Transform child in gameObject.transform)
                {
                    if (child.name.IndexOf(searchTerm, System.StringComparison.OrdinalIgnoreCase) >= 0)
                        return true;
                }
            }

            return false;
        }

        private bool IsEditorOnlyObject(GameObject gameObject)
        {
            // 排除编辑器专用对象
            return gameObject.hideFlags == HideFlags.HideAndDontSave ||
                gameObject.hideFlags == HideFlags.NotEditable;
        }

        private string GetGameObjectPath(GameObject gameObject)
        {
            var path = gameObject.name;
            var parent = gameObject.transform.parent;

            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }

        private void SelectAndPingObject(GameObject gameObject)
        {
            // 确保对象在层级视图中可见
            if (!SceneHierarchyUtil.IsExpanded(gameObject))
            {
                SceneHierarchyUtil.ExpandGameObject(gameObject);
            }

            // 选择并ping对象
            Selection.activeObject = gameObject;
            EditorGUIUtility.PingObject(gameObject);

            // 聚焦场景视图
            FocusSceneView();
        }

        private void FocusSceneView()
        {
            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                sceneView.Focus();
            }
        }

        // 键盘快捷键支持
        private void Update()
        {
            if (EditorWindow.focusedWindow == this)
            {
                if (Event.current != null && Event.current.type == EventType.KeyDown)
                {
                    switch (Event.current.keyCode)
                    {
                        case KeyCode.Return:
                            PerformSearch();
                            Event.current.Use();
                            break;
                        case KeyCode.Escape:
                            searchText = "";
                            searchResults.Clear();
                            Repaint();
                            Event.current.Use();
                            break;
                    }
                }
            }
        }
    }

    // 辅助类：用于操作场景层级视图
    public static class SceneHierarchyUtil
    {
        private static System.Reflection.MethodInfo expandMethod;
        private static System.Reflection.MethodInfo isExpandedMethod;

        static SceneHierarchyUtil()
        {
            var assembly = typeof(EditorWindow).Assembly;
            var sceneHierarchyType = assembly.GetType("UnityEditor.SceneHierarchy");
            var sceneHierarchyWindowType = assembly.GetType("UnityEditor.SceneHierarchyWindow");

            expandMethod = sceneHierarchyType.GetMethod("ExpandTreeViewItem",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            isExpandedMethod = sceneHierarchyType.GetMethod("IsExpanded",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        }

        public static void ExpandGameObject(GameObject gameObject)
        {
            var hierarchyWindow = EditorWindow.GetWindow(typeof(EditorWindow).Assembly
                .GetType("UnityEditor.SceneHierarchyWindow"));

            if (hierarchyWindow != null && expandMethod != null)
            {
                var sceneHierarchy = hierarchyWindow.GetType()
                    .GetProperty("sceneHierarchy")
                    .GetValue(hierarchyWindow);

                var instanceID = gameObject.GetInstanceID();
                expandMethod.Invoke(sceneHierarchy, new object[] { instanceID, true });
            }
        }

        public static bool IsExpanded(GameObject gameObject)
        {
            var hierarchyWindow = EditorWindow.GetWindow(typeof(EditorWindow).Assembly
                .GetType("UnityEditor.SceneHierarchyWindow"));

            if (hierarchyWindow != null && isExpandedMethod != null)
            {
                var sceneHierarchy = hierarchyWindow.GetType()
                    .GetProperty("sceneHierarchy")
                    .GetValue(hierarchyWindow);

                var instanceID = gameObject.GetInstanceID();
                return (bool)isExpandedMethod.Invoke(sceneHierarchy, new object[] { instanceID });
            }

            return false;
        }
    }
}
