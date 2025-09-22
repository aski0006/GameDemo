using System.Text;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

namespace Editor.Inspector
{

    public static class PrintSelectedGameObjectInfo
    {
        [MenuItem("Tools/打印选中 GameObject 信息 %#i")] // Ctrl/Cmd + Shift + I
        public static void PrintSelected()
        {
            // 收集选中的 GameObject（同时支持场景对象与项目窗口的 Prefab 资产）
            var selectedSet = new HashSet<GameObject>();
            foreach (var go in Selection.gameObjects)
                if (go != null)
                    selectedSet.Add(go);

            var assets = Selection.GetFiltered<GameObject>(SelectionMode.Assets);
            foreach (var go in assets)
                if (go != null)
                    selectedSet.Add(go);

            if (selectedSet.Count == 0)
            {
                Debug.Log("没有选中的 GameObject（场景或项目窗口）");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("=== Selected GameObject Info ===");

            foreach (var root in selectedSet)
            {
                PrintGameObjectRecursive(root, sb, "");
                sb.AppendLine(); // 每个选中的对象间空行分隔
            }

            // 输出到 Console
            Debug.Log(sb.ToString());
        }

        private static void PrintGameObjectRecursive(GameObject go, StringBuilder sb, string indent)
        {
            if (go == null)
            {
                sb.AppendLine(indent + "<null GameObject>");
                return;
            }

            // 基本信息：名字、激活、层、Tag、所在场景（若在场景中）
            string basic = $"{go.name}  (active={go.activeSelf}, layer={LayerMask.LayerToName(go.layer)}, tag={go.tag})";
            var scene = go.scene;
            if (scene.IsValid()) basic += $" scene=\"{scene.name}\"";

            // 预制体信息
            var assetType = PrefabUtility.GetPrefabAssetType(go);
            var instanceStatus = PrefabUtility.GetPrefabInstanceStatus(go);
            string prefabInfo = "Prefab: None";
            if (assetType != PrefabAssetType.NotAPrefab)
                prefabInfo = $"PrefabAssetType: {assetType}";
            else if (instanceStatus != PrefabInstanceStatus.NotAPrefab)
                prefabInfo = $"PrefabInstanceStatus: {instanceStatus}";

            // 对应的源 Prefab（若有）
            UnityEngine.Object source = null;
            try
            {
                source = PrefabUtility.GetCorrespondingObjectFromSource(go);
            }
            catch { source = null; }
            if (source != null)
                prefabInfo += $" (Source: {source.name})";

            sb.AppendLine($"{indent}{basic}    [{prefabInfo}]");

            // 列出该 GameObject 上挂载的组件（自身）
            var comps = go.GetComponents<Component>();
            if (comps == null || comps.Length == 0)
            {
                sb.AppendLine($"{indent}  Components: <none>");
            }
            else
            {
                sb.AppendLine($"{indent}  Components:");
                foreach (var c in comps)
                {
                    if (c == null)
                    {
                        sb.AppendLine($"{indent}    - <Missing Script>");
                        continue;
                    }

                    string compName = c.GetType().Name;
                    string compInfo = $"    - {compName}";

                    // 尝试读取常见的 enabled 属性（Behaviour/Renderer 等）
                    bool? enabledFlag = TryGetEnabledFlag(c);
                    if (enabledFlag.HasValue)
                        compInfo += $" (enabled={enabledFlag.Value})";

                    // 如果是 MonoBehaviour，输出是否是脚本实例（可以进一步扩展显示字段）
                    if (c is MonoBehaviour mb)
                    {
                        // 输出脚本启用状态已包含，不重复
                    }

                    sb.AppendLine(indent + compInfo);
                }
            }

            // 递归处理子对象（按 transform 顺序）
            var t = go.transform;
            int childCount = t.childCount;
            if (childCount > 0)
            {
                sb.AppendLine($"{indent}  Children ({childCount}):");
                for (int i = 0; i < childCount; i++)
                {
                    var child = t.GetChild(i).gameObject;
                    PrintGameObjectRecursive(child, sb, indent + "    ");
                }
            }
        }

        // 尝试获取对象的 enabled 属性（如果存在），否则返回 null
        private static bool? TryGetEnabledFlag(Component c)
        {
            // 常见接口
            if (c is Behaviour b) return b.enabled;
            if (c is Renderer r) return r.enabled;
            if (c is Collider col)
            {
                // Collider 没有 enabled 在某些版本中是有的，但 Collider 有 enabled 属性
                return col.enabled;
            }

            // 反射查找名为 "enabled" 的 public/非public 实例属性或字段
            var type = c.GetType();
            var prop = type.GetProperty("enabled", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop != null && prop.PropertyType == typeof(bool))
            {
                try { return (bool)prop.GetValue(c); }
                catch
                {
                    /* ignore */
                }
            }

            var field = type.GetField("enabled", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null && field.FieldType == typeof(bool))
            {
                try { return (bool)field.GetValue(c); }
                catch
                {
                    /* ignore */
                }
            }

            return null;
        }
    }
}
