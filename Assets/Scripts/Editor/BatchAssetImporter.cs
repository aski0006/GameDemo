using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace AssetImportTool
{
    public class BatchAssetImporter : EditorWindow
    {
        // 配置数据类
        [System.Serializable]
        public class ImportSettings
        {
            public float modelScale = 1.0f;
            public bool generateColliders = false;
            public TextureImporterType textureType = TextureImporterType.Default;
            public TextureImporterCompression textureCompression = TextureImporterCompression.Compressed;
            public AudioClipLoadType audioLoadType = AudioClipLoadType.DecompressOnLoad;
            public bool audioForceToMono = false;
            
            // 文件过滤设置
            public bool importModels = true;
            public bool importTextures = true;
            public bool importAudio = true;
            public bool importScripts = true;
            
            // 重命名和目录设置
            public bool useCustomNaming = false;
            public string namingPattern = "asset_{0}";
            public bool preserveDirectoryStructure = true;
            public string customImportPath = "ImportedAssets";
            
            // 增量导入设置
            public bool incrementalImport = true;
            public bool overwriteExisting = false;
        }

        private static ImportSettings settings = new ImportSettings();
        private Vector2 scrollPosition;
        private string targetDirectory = "";
        private string presetPath = "Assets/AssetImportTool/Presets";
        private string currentPresetName = "Default";
        
        // 添加菜单项
        [MenuItem("Asaki 框架/批量资产导入器")]
        public static void ShowWindow()
        {
            GetWindow<BatchAssetImporter>("Batch Asset Importer");
        }
        
        void OnEnable()
        {
            // 确保预设目录存在
            if (!Directory.Exists(presetPath))
            {
                Directory.CreateDirectory(presetPath);
            }
            
            // 加载默认预设
            LoadPreset("Default");
        }
        
        void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            // 预设管理部分
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("预设管理", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            currentPresetName = EditorGUILayout.TextField("预设名称", currentPresetName);
            if (GUILayout.Button("保存预设", GUILayout.Width(80)))
            {
                SavePreset(currentPresetName);
            }
            if (GUILayout.Button("加载预设", GUILayout.Width(80)))
            {
                LoadPreset(currentPresetName);
            }
            EditorGUILayout.EndHorizontal();
            
            // 目录选择部分
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("源目录", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            targetDirectory = EditorGUILayout.TextField("目录路径", targetDirectory);
            if (GUILayout.Button("浏览", GUILayout.Width(60)))
            {
                targetDirectory = EditorUtility.OpenFolderPanel("选择源目录", "", "");
            }
            EditorGUILayout.EndHorizontal();
            
            // 文件类型过滤
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("文件类型过滤", EditorStyles.boldLabel);
            settings.importModels = EditorGUILayout.Toggle("导入3D模型", settings.importModels);
            settings.importTextures = EditorGUILayout.Toggle("导入纹理", settings.importTextures);
            settings.importAudio = EditorGUILayout.Toggle("导入音频", settings.importAudio);
            settings.importScripts = EditorGUILayout.Toggle("导入脚本", settings.importScripts);
            
            // 模型设置部分
            if (settings.importModels)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("模型设置", EditorStyles.boldLabel);
                settings.modelScale = EditorGUILayout.FloatField("缩放比例", settings.modelScale);
                settings.generateColliders = EditorGUILayout.Toggle("生成碰撞体", settings.generateColliders);
            }
            
            // 纹理设置部分
            if (settings.importTextures)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("纹理设置", EditorStyles.boldLabel);
                settings.textureType = (TextureImporterType)EditorGUILayout.EnumPopup("纹理类型", settings.textureType);
                settings.textureCompression = (TextureImporterCompression)EditorGUILayout.EnumPopup("压缩格式", settings.textureCompression);
            }
            
            // 音频设置部分
            if (settings.importAudio)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("音频设置", EditorStyles.boldLabel);
                settings.audioLoadType = (AudioClipLoadType)EditorGUILayout.EnumPopup("加载方式", settings.audioLoadType);
                settings.audioForceToMono = EditorGUILayout.Toggle("强制单声道", settings.audioForceToMono);
            }
            
            // 重命名和目录设置
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("目录和命名设置", EditorStyles.boldLabel);
            settings.preserveDirectoryStructure = EditorGUILayout.Toggle("保持目录结构", settings.preserveDirectoryStructure);
            settings.useCustomNaming = EditorGUILayout.Toggle("使用自定义命名", settings.useCustomNaming);
            
            if (settings.useCustomNaming)
            {
                settings.namingPattern = EditorGUILayout.TextField("命名模式", settings.namingPattern);
                EditorGUILayout.HelpBox("使用 {0} 作为原始文件名占位符，{1} 作为计数器", MessageType.Info);
            }
            
            settings.customImportPath = EditorGUILayout.TextField("导入路径", settings.customImportPath);
            
            // 增量导入设置
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("增量导入设置", EditorStyles.boldLabel);
            settings.incrementalImport = EditorGUILayout.Toggle("启用增量导入", settings.incrementalImport);
            if (!settings.incrementalImport)
            {
                settings.overwriteExisting = EditorGUILayout.Toggle("覆盖已存在文件", settings.overwriteExisting);
            }
            
            // 导入按钮
            EditorGUILayout.Space();
            if (GUILayout.Button("开始导入", GUILayout.Height(30)))
            {
                if (!string.IsNullOrEmpty(targetDirectory) && Directory.Exists(targetDirectory))
                {
                    ImportAssets(targetDirectory);
                }
                else
                {
                    EditorUtility.DisplayDialog("错误", "请选择有效的目录", "确定");
                }
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        private void SavePreset(string presetName)
        {
            string presetFilePath = Path.Combine(presetPath, $"{presetName}.json");
            string json = JsonUtility.ToJson(settings, true);
            File.WriteAllText(presetFilePath, json);
            AssetDatabase.Refresh();
            Debug.Log($"预设已保存: {presetFilePath}");
        }
        
        private void LoadPreset(string presetName)
        {
            string presetFilePath = Path.Combine(presetPath, $"{presetName}.json");
            if (File.Exists(presetFilePath))
            {
                string json = File.ReadAllText(presetFilePath);
                JsonUtility.FromJsonOverwrite(json, settings);
                Debug.Log($"预设已加载: {presetFilePath}");
            }
            else
            {
                Debug.Log($"预设不存在，使用默认设置: {presetFilePath}");
            }
        }
        
        private void ImportAssets(string sourceDirectory)
        {
            int importedCount = 0;
            int skippedCount = 0;
            int counter = 1;
            
            // 获取所有支持的文件
            string[] allFiles = Directory.GetFiles(sourceDirectory, "*.*", SearchOption.AllDirectories);
            List<string> supportedFiles = new List<string>();
            
            foreach (string file in allFiles)
            {
                if (IsSupportedFileType(file) && ShouldImportFileType(file))
                {
                    supportedFiles.Add(file);
                }
            }
            
            // 显示进度条
            EditorUtility.DisplayProgressBar("导入资产", "准备导入...", 0);
            
            try
            {
                for (int i = 0; i < supportedFiles.Count; i++)
                {
                    string filePath = supportedFiles[i];
                    string fileName = Path.GetFileName(filePath);
                    
                    EditorUtility.DisplayProgressBar("导入资产", $"正在导入 {fileName}", (float)i / supportedFiles.Count);
                    
                    // 检查是否需要跳过已存在文件（增量导入）
                    string destinationPath = GetDestinationPath(filePath, counter);
                    if (settings.incrementalImport && File.Exists(destinationPath))
                    {
                        skippedCount++;
                        continue;
                    }
                    
                    // 根据文件类型应用不同的导入设置
                    if (ImportFileWithSettings(filePath, destinationPath, counter))
                    {
                        importedCount++;
                        counter++;
                    }
                    else
                    {
                        skippedCount++;
                    }
                    
                    // 稍微延迟以避免UI冻结
                    if (i % 10 == 0)
                    {
                        System.Threading.Thread.Sleep(10);
                    }
                }
                
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("完成", 
                    $"导入完成！\n成功导入: {importedCount}\n跳过文件: {skippedCount}", 
                    "确定");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
        
        private bool ImportFileWithSettings(string filePath, string destinationPath, int counter)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            
            try
            {
                // 确保目标目录存在
                string destDir = Path.GetDirectoryName(destinationPath);
                if (!Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }
                
                // 复制文件到Assets目录
                File.Copy(filePath, destinationPath, settings.overwriteExisting);
                
                // 根据文件类型应用导入设置
                string assetPath = destinationPath.Replace(Application.dataPath, "Assets");
                
                switch (extension)
                {
                    case ".fbx":
                    case ".obj":
                    case ".blend":
                        ApplyModelSettings(assetPath);
                        break;
                    case ".png":
                    case ".jpg":
                    case ".jpeg":
                    case ".tga":
                    case ".tif":
                    case ".tiff":
                    case ".psd":
                        ApplyTextureSettings(assetPath);
                        break;
                    case ".wav":
                    case ".mp3":
                    case ".ogg":
                    case ".aiff":
                        ApplyAudioSettings(assetPath);
                        break;
                    case ".cs":
                    case ".js":
                        // 脚本不需要特殊设置
                        break;
                }
                
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"导入文件失败 {filePath}: {e.Message}");
                return false;
            }
        }
        
        private string GetDestinationPath(string sourcePath, int counter)
        {
            string fileName = Path.GetFileNameWithoutExtension(sourcePath);
            string extension = Path.GetExtension(sourcePath);
            string relativePath = "";
            
            // 处理目录结构 - 修复这里的逻辑
            if (settings.preserveDirectoryStructure)
            {
                // 获取相对于源目录的相对路径
                relativePath = Path.GetDirectoryName(sourcePath);
                if (!string.IsNullOrEmpty(relativePath) && relativePath.StartsWith(targetDirectory))
                {
                    relativePath = relativePath.Substring(targetDirectory.Length);
                    relativePath = relativePath.TrimStart(Path.DirectorySeparatorChar);
                }
                else
                {
                    relativePath = "";
                }
            }
            
            // 处理自定义命名
            string finalFileName;
            if (settings.useCustomNaming)
            {
                finalFileName = string.Format(settings.namingPattern, fileName, counter) + extension;
            }
            else
            {
                finalFileName = fileName + extension;
            }
            
            // 构建完整路径 - 确保路径正确指向Assets目录下的目标文件夹
            string importBasePath = Path.Combine(Application.dataPath, settings.customImportPath);
            
            // 如果relativePath为空，直接返回基础路径+文件名
            if (string.IsNullOrEmpty(relativePath))
            {
                return Path.Combine(importBasePath, finalFileName);
            }
            
            return Path.Combine(importBasePath, relativePath, finalFileName);
        }
        
        private bool ShouldImportFileType(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            
            if (extension == ".fbx" || extension == ".obj" || extension == ".blend")
                return settings.importModels;
            
            if (extension == ".png" || extension == ".jpg" || extension == ".jpeg" || 
                extension == ".tga" || extension == ".psd" || extension == ".tif" || extension == ".tiff")
                return settings.importTextures;
            
            if (extension == ".wav" || extension == ".mp3" || extension == ".ogg" || extension == ".aiff")
                return settings.importAudio;
            
            if (extension == ".cs" || extension == ".js")
                return settings.importScripts;
            
            return false;
        }
        
        private bool IsSupportedFileType(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            return extension == ".fbx" || extension == ".obj" || extension == ".blend" ||
                   extension == ".png" || extension == ".jpg" || extension == ".jpeg" ||
                   extension == ".tga" || extension == ".psd" || extension == ".tif" || extension == ".tiff" ||
                   extension == ".wav" || extension == ".mp3" || extension == ".ogg" || extension == ".aiff" ||
                   extension == ".cs" || extension == ".js";
        }
        
        private void ApplyModelSettings(string assetPath)
        {
            ModelImporter modelImporter = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (modelImporter != null)
            {
                modelImporter.globalScale = settings.modelScale;
                modelImporter.addCollider = settings.generateColliders;
                modelImporter.SaveAndReimport();
            }
        }
        
        private void ApplyTextureSettings(string assetPath)
        {
            TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (textureImporter != null)
            {
                textureImporter.textureType = settings.textureType;
                textureImporter.textureCompression = settings.textureCompression;
                textureImporter.SaveAndReimport();
            }
        }
        
        private void ApplyAudioSettings(string assetPath)
        {
            AudioImporter audioImporter = AssetImporter.GetAtPath(assetPath) as AudioImporter;
            if (audioImporter != null)
            {
                AudioImporterSampleSettings sampleSettings = audioImporter.defaultSampleSettings;
                sampleSettings.loadType = settings.audioLoadType;
                
                audioImporter.defaultSampleSettings = sampleSettings;
                audioImporter.forceToMono = settings.audioForceToMono;
                audioImporter.SaveAndReimport();
            }
        }
    }
}