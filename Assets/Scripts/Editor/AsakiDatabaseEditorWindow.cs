using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using AsakiFramework.Data;

namespace AsakiFramework.Editor
{
    /// <summary>
    /// Asaki数据库编辑器窗口
    /// 提供游戏数据的可视化编辑和管理功能
    /// </summary>
    public class AsakiDatabaseEditorWindow : EditorWindow
    {
        #region 窗口配置
        
        private const string WINDOW_TITLE = "Asaki数据库编辑器";
        private const string WINDOW_MENU_PATH = "Asaki 框架/数据库编辑器";
        private const string DATABASE_FOLDER = "Assets/Resources/Database";
        private const string DATABASE_FILE_EXTENSION = ".asset";
        
        #endregion

        #region 窗口状态
        
        private Vector2 _scrollPosition;
        private int _selectedTab;
        private string[] _tabNames = { "数据管理", "类型浏览器", "设置" };
        
        // 数据列表
        private List<ScriptableObject> _databaseObjects = new List<ScriptableObject>();
        private List<Type> _availableTypes = new List<Type>();
        
        // 搜索和筛选
        private string _searchFilter = "";
        private string _typeFilter = "";
        private bool _showOnlyCustomData = false;
        
        // 选中项
        private int _selectedIndex = -1;
        private ScriptableObject _selectedObject;
        private UnityEditor.Editor _selectedEditor;
        
        // 创建新数据相关
        private string _newDataName = "";
        private int _selectedTypeIndex = 0;
        
        #endregion

        #region 窗口入口

        [MenuItem(WINDOW_MENU_PATH)]
        public static void ShowWindow()
        {
            var window = GetWindow<AsakiDatabaseEditorWindow>();
            window.titleContent = new GUIContent(WINDOW_TITLE, EditorGUIUtility.FindTexture("d_ScriptableObject Icon"));
            window.minSize = new Vector2(800, 600);
            window.Show();
        }

        #endregion

        #region 窗口生命周期

        private void OnEnable()
        {
            RefreshDatabaseList();
            RefreshAvailableTypes();
        }

        private void OnDisable()
        {
            if (_selectedEditor != null)
            {
                DestroyImmediate(_selectedEditor);
                _selectedEditor = null;
            }
        }

        private void OnFocus()
        {
            RefreshDatabaseList();
        }

        private void OnGUI()
        {
            DrawToolbar();
            DrawMainContent();
        }

        #endregion

        #region 工具栏

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            // 刷新按钮
            if (GUILayout.Button("刷新", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                RefreshDatabaseList();
                RefreshAvailableTypes();
            }
            
            GUILayout.Space(10);
            
            // 搜索框
            EditorGUI.BeginChangeCheck();
            _searchFilter = EditorGUILayout.TextField(_searchFilter, EditorStyles.toolbarSearchField, GUILayout.Width(200));
            if (EditorGUI.EndChangeCheck())
            {
                FilterDatabaseObjects();
            }
            
            GUILayout.Space(10);
            
            // 快速创建按钮
            if (GUILayout.Button("创建新数据", EditorStyles.toolbarButton, GUILayout.Width(100)))
            {
                ShowCreateDataDialog();
            }
            
            GUILayout.FlexibleSpace();
            
            // 设置按钮
            if (GUILayout.Button(EditorGUIUtility.FindTexture("d__Menu"), EditorStyles.toolbarButton, GUILayout.Width(25)))
            {
                ShowSettingsMenu();
            }
            
            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region 主内容区域

        private void DrawMainContent()
        {
            EditorGUILayout.BeginHorizontal();
            
            // 左侧数据列表
            DrawDataList();
            
            // 右侧内容区域
            DrawContentArea();
            
            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region 数据列表

        private void DrawDataList()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(300));
            
            // 标题
            EditorGUILayout.LabelField("数据库对象", EditorStyles.boldLabel);
            
            // 筛选选项
            DrawFilterOptions();
            
            // 滚动视图
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUI.skin.box);
            
            // 数据列表
            for (int i = 0; i < _databaseObjects.Count; i++)
            {
                DrawDatabaseObjectItem(i);
            }
            
            if (_databaseObjects.Count == 0)
            {
                EditorGUILayout.HelpBox("没有找到数据库对象", MessageType.Info);
            }
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.EndVertical();
        }

        private void DrawFilterOptions()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            
            // 类型筛选
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("类型筛选:", GUILayout.Width(60));
            _typeFilter = EditorGUILayout.TextField(_typeFilter, GUILayout.Width(150));
            EditorGUILayout.EndHorizontal();
            
            // 选项 - 当状态改变时刷新创建对话框
            EditorGUI.BeginChangeCheck();
            _showOnlyCustomData = EditorGUILayout.Toggle("只显示自定义数据", _showOnlyCustomData);
            if (EditorGUI.EndChangeCheck())
            {
                // 刷新数据列表
                FilterDatabaseObjects();
                
                // 如果创建对话框是打开的，刷新其类型列表
                var createDialog = Resources.FindObjectsOfTypeAll<CreateDataDialog>().FirstOrDefault();
                if (createDialog != null)
                {
                    createDialog.ShowOnlyCustomData = _showOnlyCustomData;
                }
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawDatabaseObjectItem(int index)
        {
            var dataObject = _databaseObjects[index];
            if (dataObject == null) return;
            
            bool isSelected = index == _selectedIndex;
            GUIStyle style = isSelected ? "TV Selection" : "Label";
            
            EditorGUILayout.BeginHorizontal();
            
            // 图标
            GUIContent iconContent = new GUIContent();
            iconContent.image = EditorGUIUtility.FindTexture("ScriptableObject Icon");
            iconContent.text = dataObject.name;
            
            // 按钮
            if (GUILayout.Button(iconContent, style, GUILayout.Height(20)))
            {
                SelectObject(index);
            }
            
            // 右键菜单
            if (Event.current.type == EventType.ContextClick && 
                GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
            {
                ShowObjectContextMenu(dataObject);
                Event.current.Use();
            }
            
            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region 内容区域

        private void DrawContentArea()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            
            if (_selectedObject != null)
            {
                DrawSelectedObjectInspector();
            }
            else
            {
                DrawWelcomeScreen();
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawSelectedObjectInspector()
        {
            // 标题栏
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField(_selectedObject.name, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("保存", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                SaveSelectedObject();
            }
            
            if (GUILayout.Button("另存为...", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                SaveSelectedObjectAs();
            }
            
            if (GUILayout.Button("删除", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                DeleteSelectedObject();
            }
            
            EditorGUILayout.EndHorizontal();
            
            // 类型信息
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField($"类型: {_selectedObject.GetType().FullName}", EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField($"GUID: {AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_selectedObject))}", EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndVertical();
            
            // 自定义数据属性
            DrawCustomDataProperties();
            
            // 标准 Inspector
            EditorGUILayout.Space();
            
            if (_selectedEditor == null)
            {
                _selectedEditor = UnityEditor.Editor.CreateEditor(_selectedObject);
            }
            
            if (_selectedEditor != null)
            {
                _selectedEditor.OnInspectorGUI();
            }
        }

        private void DrawCustomDataProperties()
        {
            var customDataAttributes = _selectedObject.GetType()
                .GetCustomAttributes(typeof(CustomDataAttribute), false);
            
            if (customDataAttributes.Length > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.LabelField("自定义数据属性", EditorStyles.boldLabel);
                
                foreach (CustomDataAttribute attr in customDataAttributes)
                {
                    EditorGUILayout.HelpBox("此对象包含自定义数据属性", MessageType.Info);
                }
                
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawWelcomeScreen()
        {
            EditorGUILayout.Space(50);
            
            // 标题
            var titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            
            EditorGUILayout.LabelField("Asaki数据库编辑器", titleStyle, GUILayout.Height(40));
            
            EditorGUILayout.Space(20);
            
            // 说明文字
            var descriptionStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                wordWrap = true,
                alignment = TextAnchor.MiddleCenter
            };
            
            EditorGUILayout.LabelField(
                "欢迎使用 Asaki 数据库编辑器！\n\n" +
                "• 左侧列表显示所有数据库对象\n" +
                "• 点击对象查看和编辑详细信息\n" +
                "• 支持搜索、筛选和快速创建\n" +
                "• 提供右键菜单进行更多操作",
                descriptionStyle);
            
            EditorGUILayout.Space(30);
            
            // 快速操作按钮
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("创建新数据对象", GUILayout.Width(150), GUILayout.Height(30)))
            {
                ShowCreateDataDialog();
            }
            
            GUILayout.Space(20);
            
            if (GUILayout.Button("刷新数据库列表", GUILayout.Width(150), GUILayout.Height(30)))
            {
                RefreshDatabaseList();
            }
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(50);
            
            // 快捷提示
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("快捷操作提示", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("• 双击列表项：快速选择对象");
            EditorGUILayout.LabelField("• 右键点击：显示上下文菜单");
            EditorGUILayout.LabelField("• Ctrl+S：保存当前对象");
            EditorGUILayout.LabelField("• Delete：删除选中对象");
            EditorGUILayout.EndVertical();
        }

        #endregion

        #region 数据操作

        private void SelectObject(int index)
        {
            _selectedIndex = index;
            _selectedObject = _databaseObjects[index];
            
            // 清理旧的编辑器
            if (_selectedEditor != null)
            {
                DestroyImmediate(_selectedEditor);
                _selectedEditor = null;
            }
            
            // 创建新的编辑器
            if (_selectedObject != null)
            {
                _selectedEditor = UnityEditor.Editor.CreateEditor(_selectedObject);
            }
            
            Repaint();
        }

        private void SaveSelectedObject()
        {
            if (_selectedObject == null) return;
            
            EditorUtility.SetDirty(_selectedObject);
            AssetDatabase.SaveAssets();
            
            ShowNotification(new GUIContent($"已保存: {_selectedObject.name}"));
        }

        private void SaveSelectedObjectAs()
        {
            if (_selectedObject == null) return;
            
            string path = EditorUtility.SaveFilePanelInProject(
                "保存数据库对象",
                _selectedObject.name + "_副本",
                "asset",
                "选择保存位置");
            
            if (!string.IsNullOrEmpty(path))
            {
                var copy = Instantiate(_selectedObject);
                AssetDatabase.CreateAsset(copy, path);
                AssetDatabase.SaveAssets();
                
                RefreshDatabaseList();
                ShowNotification(new GUIContent($"已另存为: {copy.name}"));
            }
        }

        private void DeleteSelectedObject()
        {
            if (_selectedObject == null) return;
            
            if (EditorUtility.DisplayDialog("确认删除", 
                $"确定要删除 {_selectedObject.name} 吗？", "删除", "取消"))
            {
                string path = AssetDatabase.GetAssetPath(_selectedObject);
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.SaveAssets();
                
                _selectedObject = null;
                _selectedIndex = -1;
                
                RefreshDatabaseList();
                ShowNotification(new GUIContent("对象已删除"));
            }
        }

        #endregion

        #region 数据创建

        private void ShowCreateDataDialog()
        {
            var window = CreateInstance<CreateDataDialog>();
            window.titleContent = new GUIContent("创建新数据对象");
            window.OnDataCreated += OnDataCreated;
            window.ShowOnlyCustomData = _showOnlyCustomData; // 传递当前筛选状态
            window.ShowUtility();
        }

        private void OnDataCreated(ScriptableObject dataObject)
        {
            if (dataObject != null)
            {
                RefreshDatabaseList();
                
                // 选中新创建的对象
                int index = _databaseObjects.IndexOf(dataObject);
                if (index >= 0)
                {
                    SelectObject(index);
                }
                
                ShowNotification(new GUIContent($"已创建: {dataObject.name}"));
            }
        }

        #endregion

        #region 数据刷新与筛选

        private void RefreshDatabaseList()
        {
            _databaseObjects.Clear();
            
            // 确保数据库文件夹存在
            if (!Directory.Exists(DATABASE_FOLDER))
            {
                Directory.CreateDirectory(DATABASE_FOLDER);
                AssetDatabase.Refresh();
            }
            
            // 查找所有 ScriptableObject
            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { DATABASE_FOLDER });
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (obj != null)
                {
                    _databaseObjects.Add(obj);
                }
            }
            
            // 应用筛选
            FilterDatabaseObjects();
        }

        private void FilterDatabaseObjects()
        {
            if (string.IsNullOrEmpty(_searchFilter) && string.IsNullOrEmpty(_typeFilter) && !_showOnlyCustomData)
                return;
            
            var filteredList = new List<ScriptableObject>();
            
            foreach (var obj in _databaseObjects)
            {
                if (obj == null) continue;
                
                bool matchesSearch = string.IsNullOrEmpty(_searchFilter) || 
                                   obj.name.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0;
                
                bool matchesType = string.IsNullOrEmpty(_typeFilter) || 
                                 obj.GetType().Name.IndexOf(_typeFilter, StringComparison.OrdinalIgnoreCase) >= 0;
                
                bool matchesCustomData = !_showOnlyCustomData || 
                                       obj.GetType().GetCustomAttributes(typeof(CustomDataAttribute), false).Length > 0;
                
                if (matchesSearch && matchesType && matchesCustomData)
                {
                    filteredList.Add(obj);
                }
            }
            
            _databaseObjects = filteredList;
        }

        private void RefreshAvailableTypes()
        {
            _availableTypes.Clear();
            
            // 获取所有继承自 ScriptableObject 的类型
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => t.IsSubclassOf(typeof(ScriptableObject)) && !t.IsAbstract)
                        .OrderBy(t => t.Name);
                    
                    _availableTypes.AddRange(types);
                }
                catch (ReflectionTypeLoadException)
                {
                    // 忽略无法加载的程序集
                }
            }
        }

        #endregion

        #region 上下文菜单

        private void ShowObjectContextMenu(ScriptableObject obj)
        {
            var menu = new GenericMenu();
            
            menu.AddItem(new GUIContent("复制"), false, () => CopyObject(obj));
            menu.AddItem(new GUIContent("删除"), false, () => DeleteObject(obj));
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("在项目中显示"), false, () => ShowInProject(obj));
            menu.AddItem(new GUIContent("重命名"), false, () => RenameObject(obj));
            
            menu.ShowAsContext();
        }

        private void ShowSettingsMenu()
        {
            var menu = new GenericMenu();
            
            menu.AddItem(new GUIContent("刷新列表"), false, () => {
                RefreshDatabaseList();
                RefreshAvailableTypes();
            });
            
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("创建数据库文件夹"), false, () => {
                CreateDatabaseFolder();
            });
            
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("导出列表..."), false, () => {
                ExportDatabaseList();
            });
            
            menu.ShowAsContext();
        }

        #endregion

        #region 辅助方法

        private void CopyObject(ScriptableObject obj)
        {
            var copy = Instantiate(obj);
            copy.name = obj.name + "_副本";
            
            string path = AssetDatabase.GetAssetPath(obj);
            string directory = Path.GetDirectoryName(path);
            string newPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(directory, copy.name + DATABASE_FILE_EXTENSION));
            
            AssetDatabase.CreateAsset(copy, newPath);
            AssetDatabase.SaveAssets();
            
            RefreshDatabaseList();
            ShowNotification(new GUIContent($"已复制: {copy.name}"));
        }

        private void DeleteObject(ScriptableObject obj)
        {
            if (EditorUtility.DisplayDialog("确认删除", 
                $"确定要删除 {obj.name} 吗？", "删除", "取消"))
            {
                string path = AssetDatabase.GetAssetPath(obj);
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.SaveAssets();
                
                RefreshDatabaseList();
                ShowNotification(new GUIContent("对象已删除"));
            }
        }

        private void ShowInProject(ScriptableObject obj)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = obj;
        }

        private void RenameObject(ScriptableObject obj)
        {
            // 创建一个简单的重命名对话框
            var renameWindow = CreateInstance<RenameDialog>();
            renameWindow.titleContent = new GUIContent("重命名对象");
            renameWindow.OldName = obj.name;
            renameWindow.OnRenamed += (newName) =>
            {
                if (!string.IsNullOrEmpty(newName) && newName != obj.name)
                {
                    obj.name = newName;
                    EditorUtility.SetDirty(obj);
                    AssetDatabase.SaveAssets();
                    
                    RefreshDatabaseList();
                    ShowNotification(new GUIContent($"已重命名为: {newName}"));
                }
            };
            renameWindow.ShowUtility();
        }

        private void CreateDatabaseFolder()
        {
            if (!Directory.Exists(DATABASE_FOLDER))
            {
                Directory.CreateDirectory(DATABASE_FOLDER);
                AssetDatabase.Refresh();
                ShowNotification(new GUIContent("数据库文件夹已创建"));
            }
            else
            {
                ShowNotification(new GUIContent("数据库文件夹已存在"));
            }
        }

        private void ExportDatabaseList()
        {
            string path = EditorUtility.SaveFilePanel("导出数据库列表", "", "DatabaseList", "txt");
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(path))
                    {
                        writer.WriteLine($"Asaki数据库列表 - 导出时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                        writer.WriteLine($"总计: {_databaseObjects.Count} 个对象");
                        writer.WriteLine(new string('-', 80));
                        
                        foreach (var obj in _databaseObjects)
                        {
                            if (obj != null)
                            {
                                writer.WriteLine($"名称: {obj.name}");
                                writer.WriteLine($"类型: {obj.GetType().FullName}");
                                writer.WriteLine($"路径: {AssetDatabase.GetAssetPath(obj)}");
                                writer.WriteLine($"GUID: {AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj))}");
                                writer.WriteLine(new string('-', 40));
                            }
                        }
                    }
                    
                    ShowNotification(new GUIContent("数据库列表已导出"));
                    EditorUtility.RevealInFinder(path);
                }
                catch (Exception e)
                {
                    ShowNotification(new GUIContent($"导出失败: {e.Message}"));
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// 创建数据对象对话框
    /// </summary>
    public class CreateDataDialog : EditorWindow
    {
        public event Action<ScriptableObject> OnDataCreated;
        
        private string _dataName = "NewData";
        private int _selectedTypeIndex = 0;
        private List<Type> _availableTypes;
        private string[] _typeNames;
        private bool _showOnlyCustomData = false;
        
        public void OnEnable()
        {
            RefreshAvailableTypes();
        }
        
        /// <summary>
        /// 设置是否只显示自定义数据类型
        /// </summary>
        public bool ShowOnlyCustomData
        {
            set
            {
                if (_showOnlyCustomData != value)
                {
                    _showOnlyCustomData = value;
                    RefreshAvailableTypes();
                }
            }
        }
        
        private void RefreshAvailableTypes()
        {
            _availableTypes = new List<Type>();
            
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => t.IsSubclassOf(typeof(ScriptableObject)) && !t.IsAbstract);
                    
                    // 如果只显示自定义数据，则过滤带有CustomDataAttribute的类型
                    if (_showOnlyCustomData)
                    {
                        types = types.Where(t => t.GetCustomAttributes(typeof(CustomDataAttribute), false).Length > 0);
                    }
                    
                    types = types.OrderBy(t => t.Name);
                    
                    _availableTypes.AddRange(types);
                }
                catch (ReflectionTypeLoadException)
                {
                    // 忽略无法加载的程序集
                }
            }
            
            _typeNames = _availableTypes.Select(t => t.Name).ToArray();
            
            // 重置选中索引，确保不超出范围
            if (_selectedTypeIndex >= _availableTypes.Count)
            {
                _selectedTypeIndex = 0;
            }
        }
        
        private void OnGUI()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(400));
            
            EditorGUILayout.LabelField("创建新数据对象", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // 名称输入
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("名称:", GUILayout.Width(50));
            _dataName = EditorGUILayout.TextField(_dataName);
            EditorGUILayout.EndHorizontal();
            
            // 类型选择
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("类型:", GUILayout.Width(50));
            if (_typeNames != null && _typeNames.Length > 0)
            {
                _selectedTypeIndex = EditorGUILayout.Popup(_selectedTypeIndex, _typeNames);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // 按钮
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("创建", GUILayout.Height(30)))
            {
                CreateDataObject();
            }
            
            if (GUILayout.Button("取消", GUILayout.Height(30)))
            {
                Close();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        private void CreateDataObject()
        {
            if (string.IsNullOrEmpty(_dataName))
            {
                EditorUtility.DisplayDialog("错误", "请输入有效的数据对象名称", "确定");
                return;
            }
            
            if (_availableTypes == null || _selectedTypeIndex >= _availableTypes.Count)
            {
                EditorUtility.DisplayDialog("错误", "请选择有效的数据类型", "确定");
                return;
            }
            
            try
            {
                var selectedType = _availableTypes[_selectedTypeIndex];
                var newObject = CreateInstance(selectedType);
                newObject.name = _dataName;
                
                // 确保数据库文件夹存在
                string databaseFolder = "Assets/Resources/Database";
                if (!Directory.Exists(databaseFolder))
                {
                    Directory.CreateDirectory(databaseFolder);
                }
                
                string path = AssetDatabase.GenerateUniqueAssetPath($"{databaseFolder}/{_dataName}.asset");
                AssetDatabase.CreateAsset(newObject, path);
                AssetDatabase.SaveAssets();
                
                OnDataCreated?.Invoke(newObject);
                Close();
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("错误", $"创建数据对象失败: {e.Message}", "确定");
            }
        }
    }

    #region 自定义特性

    /// <summary>
    /// 数据库配置特性 - 标记数据库配置类
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DatabaseConfigAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public Color Color { get; set; } = Color.white;
        
        public DatabaseConfigAttribute(string name)
        {
            Name = name;
        }
    }

    #endregion
}

/// <summary>
/// 重命名对话框
/// </summary>
public class RenameDialog : EditorWindow
{
    public event Action<string> OnRenamed;
    
    public string OldName { get; set; } = "";
    private string _newName = "";
    
    private void OnGUI()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(300));
        
        EditorGUILayout.LabelField("重命名对象", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.LabelField($"当前名称: {OldName}");
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("新名称:", GUILayout.Width(60));
        _newName = EditorGUILayout.TextField(_newName);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("确定", GUILayout.Height(25)))
        {
            if (!string.IsNullOrEmpty(_newName))
            {
                OnRenamed?.Invoke(_newName);
                Close();
            }
        }
        
        if (GUILayout.Button("取消", GUILayout.Height(25)))
        {
            Close();
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
    }

}
