using System.Linq;
using UnityEditor;
using UnityEngine;

public class HierarchyExpandCollapse : EditorWindow
{
    bool expand = true;

    [MenuItem("Tools/Hierarchy/展开全部 &F1")]
    static void ExpandAll() => ToggleAll(true);

    [MenuItem("Tools/Hierarchy/收起全部 &F2")]
    static void CollapseAll() => ToggleAll(false);

    [MenuItem("Tools/Hierarchy/切换展开状态 %&E")]
    static void OpenWindow()
    {
        var w = GetWindow<HierarchyExpandCollapse>();
        w.titleContent = new GUIContent("Expand/Collapse");
    }

    void OnGUI()
    {
        expand = GUILayout.Toggle(expand, "展开");
        if (GUILayout.Button("应用到选中或全部"))
            ToggleAll(expand);
    }

    static void ToggleAll(bool expand)
    {
        GameObject[] roots;
        if (Selection.gameObjects.Length > 0)
        {
            // 只处理选中的根节点
            roots = Selection.gameObjects.Where(go => go.transform.parent == null).ToArray();
        }
        else
        {
            // 整个场景
            roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene()
                .GetRootGameObjects().ToArray();
        }

        foreach (var go in roots)
            SetExpandedRecursive(go, expand);
    }

    static void SetExpandedRecursive(GameObject go, bool expand)
    {
        var id = go.GetInstanceID();
        var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
        var window = EditorWindow.GetWindow(type);

        // 反射调用 SetExpandedRecursive
        var method = type.GetMethod("SetExpandedRecursive",
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.Instance,
            null, new[] { typeof(int), typeof(bool) }, null);

        method?.Invoke(window, new object[] { id, expand });
    }
}
