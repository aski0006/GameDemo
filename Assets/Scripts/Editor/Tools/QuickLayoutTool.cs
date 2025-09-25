using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class QuickLayoutTool : EditorWindow
{
    enum LayoutMode { Horizontal, Vertical, Grid }

    [Header("目标对象"), Tooltip("将要排列的对象")]
    [SerializeField] private List<Transform> targets = new List<Transform>();
    private Vector3 spacing = Vector3.one;
    private Vector2Int gridSize = new Vector2Int(3, 3);
    private LayoutMode mode = LayoutMode.Horizontal;
    private bool showGizmo = true;

    private Vector3[] previewPositions;
    private Bounds previewBounds;

    [MenuItem("Tools/快速布局工具")]
    static void Open() => GetWindow<QuickLayoutTool>("Quick Layout");

    // 只创建一次 SerializedObject
    private SerializedObject so;
    private SerializedProperty propTargets;

    void OnEnable()
    {
        so = new SerializedObject(this);
        propTargets = so.FindProperty("targets");
        SceneView.duringSceneGui += OnSceneHandles;
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneHandles;
        so?.Dispose();
    }

    void OnGUI()
    {
        so.Update();

        mode = (LayoutMode)EditorGUILayout.EnumPopup("Layout", mode);
        spacing = EditorGUILayout.Vector3Field("Spacing", spacing);

        if (mode == LayoutMode.Grid)
            gridSize = EditorGUILayout.Vector2IntField("Grid (列×行)", gridSize);

        showGizmo = GUILayout.Toggle(showGizmo, "Show Preview");

        // 自定义拖拽区域
        GUILayout.Space(10);
        GUILayout.Label("拖拽场景中的物体到下方：");
        Rect dropRect = GUILayoutUtility.GetRect(0, 50, GUILayout.ExpandWidth(true));
        GUI.Box(dropRect, "拖放 Hierarchy 中的 Transform 到这里");
        HandleDragAndDrop(dropRect);

        // 显示当前列表
        for (int i = 0; i < targets.Count; i++)
        {
            GUILayout.BeginHorizontal();
            targets[i] = (Transform)EditorGUILayout.ObjectField(targets[i], typeof(Transform), true);
            if (GUILayout.Button("×", GUILayout.Width(20)))
            {
                targets.RemoveAt(i);
                RecalculatePreview();
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(10);
        GUI.enabled = targets.Count > 0;
        if (GUILayout.Button("Apply Layout"))
            ApplyNow();
        GUI.enabled = true;

        GUILayout.Space(10);
        if (GUILayout.Button("Clear"))
        {
            targets.Clear();
            RecalculatePreview();
        }

        so.ApplyModifiedProperties();
    }
    
    void HandleDragAndDrop(Rect rect)
    {
        Event evt = Event.current;

        if (!rect.Contains(evt.mousePosition)) return;

        switch (evt.type)
        {
            case EventType.DragUpdated:
                bool valid = DragAndDrop.objectReferences.All(o => o is Transform);
                DragAndDrop.visualMode = valid ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;
                evt.Use();
                break;

            case EventType.DragPerform:
                DragAndDrop.AcceptDrag();
                foreach (var obj in DragAndDrop.objectReferences)
                {
                    if (obj is Transform t && !targets.Contains(t))
                        targets.Add(t);
                }
                RecalculatePreview();
                evt.Use();
                break;
        }
    }

    void RecalculatePreview()
    {
        if (targets.Count == 0) return;

        previewPositions = new Vector3[targets.Count];
        var cur = Vector3.zero;

        switch (mode)
        {
            case LayoutMode.Horizontal:
                for (int i = 0; i < targets.Count; i++)
                {
                    previewPositions[i] = cur;
                    cur.x -= spacing.x;
                }
                break;

            case LayoutMode.Vertical:
                for (int i = 0; i < targets.Count; i++)
                {
                    previewPositions[i] = cur;
                    cur.y += spacing.y;
                }
                break;

            case LayoutMode.Grid:
                int cols = gridSize.x;
                for (int i = 0; i < targets.Count; i++)
                {
                    int c = i % cols;
                    int r = i / cols;
                    previewPositions[i] = new Vector3(c * spacing.x, -r * spacing.y, 0);
                }
                break;
        }

        Vector3 min = previewPositions[0], max = previewPositions[0];
        foreach (var p in previewPositions)
        {
            min = Vector3.Min(min, p);
            max = Vector3.Max(max, p);
        }
        previewBounds = new Bounds((min + max) * 0.5f, max - min);
    }

    void ApplyNow()
    {
        if (targets.Count == 0) return;

        Undo.RecordObjects(targets.Select(t => t.transform).ToArray(), "Quick Layout");
        RecalculatePreview();
        for (int i = 0; i < targets.Count; i++)
        {
            var t = targets[i];
            if (t != null)
            {
                t.localPosition = previewPositions[i];
                EditorUtility.SetDirty(t);
            }
        }
    }

    // 用 Handles 画预览，避免 Gizmos 限制
    void OnSceneHandles(SceneView sv)
    {
        if (!showGizmo || previewPositions == null || previewPositions.Length == 0) return;

        Handles.color = Color.green;
        foreach (var p in previewPositions)
            Handles.DrawWireCube(p, Vector3.one * 0.5f);

        Handles.color = Color.cyan;
        Handles.DrawWireCube(previewBounds.center, previewBounds.size);
    }
}