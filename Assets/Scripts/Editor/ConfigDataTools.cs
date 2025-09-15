using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace YuankunHuang.Unity.Editor
{
    /// <summary>
    /// Tools for managing config data
    /// </summary>
    public static class ConfigDataTools
    {
        [MenuItem("Tools/Config Data/Update WebGL GameDataManager")]
        public static void UpdateWebGLGameDataManager()
        {
            var processor = new WebGLBuildProcessor();
            // Use reflection to call the private method
            var method = typeof(WebGLBuildProcessor).GetMethod("GenerateWebGLGameDataManager",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (method != null)
            {
                try
                {
                    method.Invoke(processor, null);
                    Debug.Log("[ConfigDataTools] WebGL GameDataManager updated successfully!");
                    EditorUtility.DisplayDialog("Success", "WebGL GameDataManager has been updated with all current config classes.", "OK");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[ConfigDataTools] Failed to update WebGL GameDataManager: {e.Message}");
                    EditorUtility.DisplayDialog("Error", $"Failed to update WebGL GameDataManager:\n{e.Message}", "OK");
                }
            }
        }

        [MenuItem("Tools/Config Data/List All Config Classes")]
        public static void ListAllConfigClasses()
        {
            // Search in multiple assemblies
            var assemblies = new[]
            {
                System.Reflection.Assembly.GetExecutingAssembly(),
                typeof(YuankunHuang.Unity.GameDataConfig.GameDataManager).Assembly,
                System.Reflection.Assembly.GetAssembly(typeof(UnityEngine.MonoBehaviour))
            };

            var allTypes = new List<System.Type>();
            foreach (var assembly in assemblies.Distinct())
            {
                if (assembly != null)
                {
                    try
                    {
                        var types = assembly.GetTypes();
                        allTypes.AddRange(types);
                        Debug.Log($"Assembly: {assembly.FullName} - {types.Length} types");
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"Failed to get types from assembly {assembly.FullName}: {e.Message}");
                    }
                }
            }

            Debug.Log($"Total types across all assemblies: {allTypes.Count}");

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

            Debug.Log($"Found {configTypes.Count} config classes:");
            foreach (var type in configTypes)
            {
                Debug.Log($"- {type.Name} : {type.BaseType?.Name}");
            }

            // Debug: Show types ending with "Config"
            var debugConfigTypes = allTypes
                .Where(t => t.IsClass && t.IsPublic && t.Name.EndsWith("Config"))
                .ToList();
            Debug.Log($"Debug - All types ending with 'Config': {debugConfigTypes.Count}");
            foreach (var type in debugConfigTypes)
            {
                Debug.Log($"  - {type.Name} : {type.BaseType?.Name} (Generic: {type.BaseType?.IsGenericType}) in {type.Namespace}");
            }
        }
    }
}