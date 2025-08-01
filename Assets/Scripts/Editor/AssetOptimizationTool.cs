using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace YuankunHuang.SynthMind.Editor
{
    /// <summary>
    /// 资源优化编辑器工具
    /// </summary>
    public class AssetOptimizationTool : EditorWindow
    {
        private Vector2 scrollPosition;
        private List<SpriteAnalysisResult> analysisResults = new List<SpriteAnalysisResult>();
        private bool showOptimalSprites = true;
        private bool showNonOptimalSprites = true;
        private string searchFilter = "";

        [MenuItem("SynthMind/Tools/资源优化工具")]
        public static void ShowWindow()
        {
            GetWindow<AssetOptimizationTool>("资源优化工具");
        }

        private void OnEnable()
        {
            AnalyzeAssets();
        }

        private void OnGUI()
        {
            GUILayout.Label("资源优化分析工具", EditorStyles.boldLabel);
            
            // 工具栏
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("重新分析", GUILayout.Width(100)))
            {
                AnalyzeAssets();
            }
            if (GUILayout.Button("导出报告", GUILayout.Width(100)))
            {
                ExportReport();
            }
            EditorGUILayout.EndHorizontal();

            // 过滤选项
            EditorGUILayout.BeginHorizontal();
            showOptimalSprites = EditorGUILayout.Toggle("显示优化资源", showOptimalSprites);
            showNonOptimalSprites = EditorGUILayout.Toggle("显示需优化资源", showNonOptimalSprites);
            EditorGUILayout.EndHorizontal();

            // 搜索框
            searchFilter = EditorGUILayout.TextField("搜索", searchFilter);

            // 统计信息
            DisplayStatistics();

            // 资源列表
            DisplayAssetList();
        }

        /// <summary>
        /// 分析资源
        /// </summary>
        private void AnalyzeAssets()
        {
            analysisResults.Clear();

            // 查找所有Sprite资源
            string[] guids = AssetDatabase.FindAssets("t:Sprite");
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                
                if (sprite != null)
                {
                    SpriteAnalysisResult result = AnalyzeSprite(sprite, path);
                    analysisResults.Add(result);
                }
            }

            // 按优化状态排序
            analysisResults = analysisResults.OrderBy(r => r.IsOptimal).ThenBy(r => r.Name).ToList();
        }

        /// <summary>
        /// 分析单个Sprite
        /// </summary>
        /// <param name="sprite">Sprite对象</param>
        /// <param name="path">资源路径</param>
        /// <returns>分析结果</returns>
        private SpriteAnalysisResult AnalyzeSprite(Sprite sprite, string path)
        {
            Texture2D texture = sprite.texture;
            bool isOptimal = texture.width % 4 == 0 && texture.height % 4 == 0;
            
            // 计算文件大小（估算）
            long fileSize = GetEstimatedFileSize(texture);
            
            // 判断是否适合Atlas
            bool shouldUseAtlas = !isOptimal || fileSize < 1024 * 10; // 小于10KB建议用Atlas

            return new SpriteAnalysisResult
            {
                Name = sprite.name,
                Path = path,
                Width = texture.width,
                Height = texture.height,
                IsOptimal = isOptimal,
                FileSize = fileSize,
                ShouldUseAtlas = shouldUseAtlas,
                CompressionFormat = GetCompressionFormat(path)
            };
        }

        /// <summary>
        /// 估算文件大小
        /// </summary>
        /// <param name="texture">纹理</param>
        /// <returns>估算的文件大小（字节）</returns>
        private long GetEstimatedFileSize(Texture2D texture)
        {
            // 简单的估算，实际大小取决于压缩格式
            int pixelCount = texture.width * texture.height;
            return pixelCount * 4; // 假设RGBA32格式
        }

        /// <summary>
        /// 获取压缩格式
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns>压缩格式</returns>
        private string GetCompressionFormat(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                TextureImporterPlatformSettings settings = importer.GetDefaultPlatformTextureSettings();
                return settings.format.ToString();
            }
            return "Unknown";
        }

        /// <summary>
        /// 显示统计信息
        /// </summary>
        private void DisplayStatistics()
        {
            var optimalSprites = analysisResults.Where(r => r.IsOptimal).ToList();
            var nonOptimalSprites = analysisResults.Where(r => !r.IsOptimal).ToList();
            var atlasCandidates = analysisResults.Where(r => r.ShouldUseAtlas).ToList();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("统计信息", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"总资源数: {analysisResults.Count}");
            EditorGUILayout.LabelField($"优化资源: {optimalSprites.Count}");
            EditorGUILayout.LabelField($"需优化: {nonOptimalSprites.Count}");
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"建议使用Atlas: {atlasCandidates.Count}");
            EditorGUILayout.LabelField($"总文件大小: {FormatFileSize(analysisResults.Sum(r => r.FileSize))}");
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 显示资源列表
        /// </summary>
        private void DisplayAssetList()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("资源详情", EditorStyles.boldLabel);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            var filteredResults = analysisResults.Where(r => 
                (showOptimalSprites && r.IsOptimal || showNonOptimalSprites && !r.IsOptimal) &&
                (string.IsNullOrEmpty(searchFilter) || r.Name.ToLower().Contains(searchFilter.ToLower()))
            ).ToList();

            foreach (var result in filteredResults)
            {
                DisplayAssetItem(result);
            }

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// 显示单个资源项
        /// </summary>
        /// <param name="result">分析结果</param>
        private void DisplayAssetItem(SpriteAnalysisResult result)
        {
            EditorGUILayout.BeginHorizontal("box");
            
            // 状态图标
            string statusIcon = result.IsOptimal ? "✅" : "⚠️";
            EditorGUILayout.LabelField(statusIcon, GUILayout.Width(30));
            
            // 资源信息
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(result.Name, EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"尺寸: {result.Width}x{result.Height}");
            EditorGUILayout.LabelField($"大小: {FormatFileSize(result.FileSize)}");
            EditorGUILayout.LabelField($"压缩: {result.CompressionFormat}");
            EditorGUILayout.EndVertical();
            
            // 建议
            EditorGUILayout.BeginVertical();
            if (!result.IsOptimal)
            {
                EditorGUILayout.LabelField("建议: 调整尺寸为4的倍数", EditorStyles.miniLabel);
            }
            if (result.ShouldUseAtlas)
            {
                EditorGUILayout.LabelField("建议: 放入SpriteAtlas", EditorStyles.miniLabel);
            }
            EditorGUILayout.EndVertical();
            
            // 操作按钮
            EditorGUILayout.BeginVertical();
            if (GUILayout.Button("选择", GUILayout.Width(60)))
            {
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(result.Path);
            }
            if (GUILayout.Button("打开", GUILayout.Width(60)))
            {
                AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<Object>(result.Path));
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        /// <summary>
        /// 格式化文件大小
        /// </summary>
        /// <param name="bytes">字节数</param>
        /// <returns>格式化的文件大小</returns>
        private string FormatFileSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes}B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024}KB";
            return $"{bytes / (1024 * 1024)}MB";
        }

        /// <summary>
        /// 导出报告
        /// </summary>
        private void ExportReport()
        {
            string report = GenerateReport();
            string path = EditorUtility.SaveFilePanel("保存优化报告", "", "AssetOptimizationReport", "txt");
            
            if (!string.IsNullOrEmpty(path))
            {
                System.IO.File.WriteAllText(path, report);
                Debug.Log($"优化报告已保存到: {path}");
            }
        }

        /// <summary>
        /// 生成报告
        /// </summary>
        /// <returns>报告内容</returns>
        private string GenerateReport()
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("=== SynthMind 资源优化报告 ===");
            report.AppendLine($"生成时间: {System.DateTime.Now}");
            report.AppendLine();
            
            var optimalSprites = analysisResults.Where(r => r.IsOptimal).ToList();
            var nonOptimalSprites = analysisResults.Where(r => !r.IsOptimal).ToList();
            
            report.AppendLine($"总资源数: {analysisResults.Count}");
            report.AppendLine($"优化资源: {optimalSprites.Count}");
            report.AppendLine($"需优化资源: {nonOptimalSprites.Count}");
            report.AppendLine();
            
            if (nonOptimalSprites.Count > 0)
            {
                report.AppendLine("=== 需要优化的资源 ===");
                foreach (var sprite in nonOptimalSprites)
                {
                    report.AppendLine($"- {sprite.Name}: {sprite.Width}x{sprite.Height} ({sprite.Path})");
                }
                report.AppendLine();
            }
            
            return report.ToString();
        }
    }

    /// <summary>
    /// Sprite分析结果
    /// </summary>
    public class SpriteAnalysisResult
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsOptimal { get; set; }
        public long FileSize { get; set; }
        public bool ShouldUseAtlas { get; set; }
        public string CompressionFormat { get; set; }
    }
} 