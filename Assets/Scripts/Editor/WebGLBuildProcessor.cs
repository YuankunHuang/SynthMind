using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace YuankunHuang.Unity.Editor
{
    /// <summary>
    /// Automatically generates WebGL-compatible code during build process
    /// </summary>
    public class WebGLBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            // Only process WebGL builds
            if (report.summary.platform == BuildTarget.WebGL)
            {
                Debug.Log("[WebGLBuildProcessor] Generating WebGL-compatible GameDataManager...");
                GenerateWebGLGameDataManager();
            }
        }

        private void GenerateWebGLGameDataManager()
        {
            try
            {
                // Search in multiple assemblies, same logic as ConfigDataTools
                var assemblies = new[]
                {
                    Assembly.GetExecutingAssembly(),
                    typeof(YuankunHuang.Unity.GameDataConfig.GameDataManager).Assembly,
                    Assembly.GetAssembly(typeof(UnityEngine.MonoBehaviour))
                };

                var allTypes = new List<Type>();
                foreach (var assembly in assemblies.Distinct())
                {
                    if (assembly != null)
                    {
                        try
                        {
                            allTypes.AddRange(assembly.GetTypes());
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"[WebGLBuildProcessor] Failed to get types from assembly {assembly.FullName}: {e.Message}");
                        }
                    }
                }

                var configTypes = allTypes
                    .Where(t =>
                    {
                        if (!t.IsClass || !t.IsPublic || !t.Name.EndsWith("Config"))
                            return false;

                        if (t.BaseType == null)
                            return false;

                        // More flexible base type checking
                        var baseType = t.BaseType;
                        while (baseType != null)
                        {
                            if (baseType.IsGenericType)
                            {
                                var genericDef = baseType.GetGenericTypeDefinition();
                                if (genericDef.Name.StartsWith("BaseConfigData"))
                                    return true;
                            }
                            if (baseType.Name.StartsWith("BaseConfigData"))
                                return true;
                            baseType = baseType.BaseType;
                        }
                        return false;
                    })
                    .ToList();

                var sb = new StringBuilder();

                // Generate the complete WebGL initialization method
                sb.AppendLine("        /// <summary>");
                sb.AppendLine("        /// WebGL-compatible initialization (auto-generated)");
                sb.AppendLine("        /// </summary>");
                sb.AppendLine("        private static void InitializeWebGL()");
                sb.AppendLine("        {");
                sb.AppendLine("            LogHelper.Log(\"[GameDataManager] Using WebGL initialization mode\");");
                sb.AppendLine();

                foreach (var type in configTypes)
                {
                    sb.AppendLine($"            try {{ {type.Name}.Initialize(); LogHelper.Log(\"[GameDataManager] {type.Name} initialized\"); }}");
                    sb.AppendLine($"            catch (Exception e) {{ LogHelper.LogWarning($\"[GameDataManager] {type.Name} failed: {{e.Message}}\"); }}");
                    sb.AppendLine();
                }

                sb.AppendLine("        }");

                // Read current GameDataManager file
                var gameDataManagerPath = Path.Combine(Application.dataPath, "Scripts/ConfigData/code/GameDataManager.cs");
                if (!File.Exists(gameDataManagerPath))
                {
                    Debug.LogError($"[WebGLBuildProcessor] GameDataManager.cs not found at: {gameDataManagerPath}");
                    return;
                }

                var content = File.ReadAllText(gameDataManagerPath);
                var newMethodCode = sb.ToString();

                // Replace the WebGL initialization method - more flexible pattern matching
                var startMarker = "private static void InitializeWebGL()";
                var startIndex = content.IndexOf(startMarker);

                if (startIndex != -1)
                {
                    // Find the start of the method (including summary comment)
                    var summaryStart = content.LastIndexOf("/// <summary>", startIndex);
                    var methodStart = summaryStart != -1 ? summaryStart : startIndex;

                    // Find the opening brace after the method declaration
                    var openBraceIndex = content.IndexOf('{', startIndex);
                    if (openBraceIndex == -1)
                    {
                        Debug.LogError("[WebGLBuildProcessor] Could not find opening brace for InitializeWebGL method");
                        return;
                    }

                    // Count braces to find the matching closing brace
                    var braceCount = 1;
                    var endIndex = openBraceIndex + 1;

                    while (endIndex < content.Length && braceCount > 0)
                    {
                        if (content[endIndex] == '{')
                            braceCount++;
                        else if (content[endIndex] == '}')
                            braceCount--;

                        if (braceCount > 0)
                            endIndex++;
                    }

                    if (braceCount == 0)
                    {
                        // Include the closing brace and move to next line
                        endIndex++;

                        // Replace the entire method including comments
                        var newContent = content.Substring(0, methodStart) +
                                       newMethodCode +
                                       content.Substring(endIndex);

                        File.WriteAllText(gameDataManagerPath, newContent);
                        Debug.Log($"[WebGLBuildProcessor] Updated GameDataManager with {configTypes.Count} config classes");

                        // Refresh the asset database
                        AssetDatabase.Refresh();
                    }
                    else
                    {
                        Debug.LogError("[WebGLBuildProcessor] Could not find matching closing brace for InitializeWebGL method");
                    }
                }
                else
                {
                    Debug.LogError("[WebGLBuildProcessor] Could not find InitializeWebGL method in GameDataManager.cs");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[WebGLBuildProcessor] Failed to generate WebGL GameDataManager: {e.Message}");
            }
        }
    }
}