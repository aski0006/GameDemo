using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using AsakiFramework.ObjectPool;

namespace AsakiFramework.Editor
{
    /// <summary>
    /// 对象池查看器窗口
    /// 用于运行时查看对象池管理器状态
    /// </summary>
    public class ObjectPoolViewer : EditorWindow
    {
        private Vector2 _scrollPosition;
        private bool _autoRefresh = true;
        private float _lastRefreshTime;
        private readonly float _refreshInterval = 1f; // 每秒刷新一次

        // 折叠状态记录
        private readonly Dictionary<string, bool> _poolFoldoutStates = new Dictionary<string, bool>();

        [MenuItem("Asaki 框架/对象池查看器")]
        public static void ShowWindow()
        {
            var window = GetWindow<ObjectPoolViewer>("对象池查看器");
            window.minSize = new Vector2(600, 400);
            window.Show();
        }

        private void OnGUI()
        {
            DrawToolbar();
            DrawPoolStats();
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
                    RefreshData();
                }

                GUILayout.FlexibleSpace();

                // 清空所有池按钮
                if (GUILayout.Button("清空所有池", EditorStyles.toolbarButton, GUILayout.Width(80)))
                {
                    if (EditorApplication.isPlaying)
                    {
                        ObjectPoolManager.Instance.ClearAllPools();
                        RefreshData();
                    }
                }

                // 归还所有对象按钮
                if (GUILayout.Button("归还所有对象", EditorStyles.toolbarButton, GUILayout.Width(100)))
                {
                    if (EditorApplication.isPlaying)
                    {
                        ObjectPoolManager.Instance.ReturnAllToPools();
                        RefreshData();
                    }
                }
            }
            GUILayout.EndHorizontal();

            // 分隔线
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        private void DrawPoolStats()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("请进入运行模式以查看对象池状态", MessageType.Info);
                return;
            }

            if (ObjectPoolManager.Instance == null)
            {
                EditorGUILayout.HelpBox("对象池管理器未初始化", MessageType.Warning);
                return;
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            {
                // 总体统计信息
                DrawSummaryStats();

                // 详细池信息
                DrawDetailedPoolInfo();
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawSummaryStats()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("总体统计", EditorStyles.boldLabel);

            var manager = ObjectPoolManager.Instance;
            var stats = manager.GetAllPoolStats();

            // 解析统计信息（这里简化处理，实际可以更精细地解析）
            var lines = stats.Split('\n');
            foreach (var line in lines)
            {
                if (!string.IsNullOrEmpty(line.Trim()))
                {
                    EditorGUILayout.LabelField(line.Trim());
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        private void DrawDetailedPoolInfo()
        {
            EditorGUILayout.LabelField("详细池信息", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            var manager = ObjectPoolManager.Instance;

            // 这里需要通过反射获取内部池信息，因为字典是私有的
            // 在实际使用中，你可能需要在ObjectPoolManager中添加公共的访问方法
            DrawPoolSection("GameObject 池", GetGameObjectPoolsInfo());
            DrawPoolSection("Component 池", GetComponentPoolsInfo());
            DrawPoolSection("通用对象池", GetGenericPoolsInfo());
        }

        private void DrawPoolSection(string sectionTitle, List<PoolInfo> poolInfos)
        {
            if (poolInfos.Count == 0) return;

            EditorGUILayout.LabelField(sectionTitle, EditorStyles.boldLabel);

            foreach (var poolInfo in poolInfos)
            {
                if (!_poolFoldoutStates.ContainsKey(poolInfo.Name))
                {
                    _poolFoldoutStates[poolInfo.Name] = false;
                }

                // 池信息折叠项
                _poolFoldoutStates[poolInfo.Name] = EditorGUILayout.Foldout(
                    _poolFoldoutStates[poolInfo.Name],
                    $"{poolInfo.Name} ({poolInfo.Type})",
                    true
                );

                if (_poolFoldoutStates[poolInfo.Name])
                {
                    EditorGUI.indentLevel++;

                    // 显示详细统计信息
                    EditorGUILayout.LabelField($"总对象数: {poolInfo.TotalCount}");
                    EditorGUILayout.LabelField($"活跃对象: {poolInfo.ActiveCount}");
                    EditorGUILayout.LabelField($"非活跃对象: {poolInfo.InactiveCount}");
                    EditorGUILayout.LabelField($"最大容量: {poolInfo.MaxSize}");

                    // 进度条显示使用率
                    float usageRatio = poolInfo.MaxSize > 0 ? (float)poolInfo.TotalCount / poolInfo.MaxSize : 0;
                    DrawProgressBar("池使用率", usageRatio, $"{poolInfo.TotalCount}/{poolInfo.MaxSize}");

                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space();
                }
            }

            EditorGUILayout.Space();
        }

        private void DrawProgressBar(string label, float value, string text)
        {
            Rect rect = GUILayoutUtility.GetRect(200, 20);
            EditorGUI.ProgressBar(rect, value, text);
            EditorGUILayout.LabelField(label, $"{value:P1}");
        }

        private List<PoolInfo> GetGameObjectPoolsInfo()
        {
            var pools = new List<PoolInfo>();

            // 这里需要通过反射访问私有字段 _gameObjectPools
            // 在实际使用中，建议在ObjectPoolManager中添加公共的访问方法
            var manager = ObjectPoolManager.Instance;

            var gameObjectPools = manager.GetGameObjectPools();
            if (gameObjectPools != null)
            {
                foreach (var kvp in gameObjectPools)
                {
                    var pool = kvp.Value;
                    pools.Add(new PoolInfo
                    {
                        Name = pool.PoolName,
                        Type = "GameObject",
                        TotalCount = pool.TotalCount,
                        ActiveCount = pool.ActiveCount,
                        InactiveCount = pool.InactiveCount,
                        MaxSize = pool.MaxPoolSize
                    });
                }
            }


            return pools;
        }

        private List<PoolInfo> GetComponentPoolsInfo()
        {
            var pools = new List<PoolInfo>();

            // 类似地，通过反射访问 _componentPools
            var manager = ObjectPoolManager.Instance;

            var componentPools = manager.GetComponentPools();
            if (componentPools != null)
            {
                foreach (var typeDict in componentPools)
                {
                    foreach (var poolDict in typeDict.Value)
                    {
                        if (poolDict.Value is IObjectPool pool)
                        {
                            pools.Add(new PoolInfo
                            {
                                Name = pool.PoolName,
                                Type = $"Component<{typeDict.Key.Name}>",
                                TotalCount = pool.TotalCount,
                                ActiveCount = pool.ActiveCount,
                                InactiveCount = pool.InactiveCount,
                                MaxSize = GetMaxSizeViaReflection(pool)
                            });
                        }
                    }
                }
            }

            return pools;
        }

        private List<PoolInfo> GetGenericPoolsInfo()
        {
            var pools = new List<PoolInfo>();

            // 访问 _pools 字典
            var manager = ObjectPoolManager.Instance;

            var allPools = manager.GetAllPools();
            if (allPools != null)
            {
                foreach (var kvp in allPools)
                {
                    var pool = kvp.Value;

                    // 过滤掉已经处理过的GameObject和Component池
                    if (!(pool is GameObjectPool) && !IsComponentPool(pool))
                    {
                        pools.Add(new PoolInfo
                        {
                            Name = pool.PoolName,
                            Type = "Generic",
                            TotalCount = pool.TotalCount,
                            ActiveCount = pool.ActiveCount,
                            InactiveCount = pool.InactiveCount,
                            MaxSize = GetMaxSizeViaReflection(pool)
                        });
                    }
                }
            }

            return pools;
        }

        private bool IsComponentPool(IObjectPool pool)
        {
            return pool.GetType().IsGenericType &&
                pool.GetType().GetGenericTypeDefinition() == typeof(ComponentPool<>);
        }

        private int GetMaxSizeViaReflection(IObjectPool pool)
        {
            var prop = pool.GetType().GetProperty("MaxPoolSize");
            return prop != null ? (int)prop.GetValue(pool) : 100;
        }

        private void Update()
        {
            if (_autoRefresh && EditorApplication.isPlaying && ObjectPoolManager.Instance != null)
            {
                if (Time.realtimeSinceStartup - _lastRefreshTime > _refreshInterval)
                {
                    RefreshData();
                    _lastRefreshTime = Time.realtimeSinceStartup;
                }
            }
        }

        private void RefreshData()
        {
            Repaint();
        }

        /// <summary>
        /// 池信息数据结构
        /// </summary>
        private class PoolInfo
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public int TotalCount { get; set; }
            public int ActiveCount { get; set; }
            public int InactiveCount { get; set; }
            public int MaxSize { get; set; }
        }
    }
}
