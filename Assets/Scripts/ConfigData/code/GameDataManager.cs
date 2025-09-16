using System;
using System.Reflection;
using System.Linq;
using UnityEngine;

namespace YuankunHuang.Unity.GameDataConfig
{
    /// <summary>
    /// Centralized Game Data Manager
    /// Automatically initializes all Config classes via reflection
    /// </summary>
    public static class GameDataManager
    {
        private static bool _isInitialized = false;

        /// <summary>
        /// Initialize all Config classes (WebGL-compatible version)
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized) return;

            Debug.Log("[GameDataManager] Starting initialization...");

#if UNITY_WEBGL && !UNITY_EDITOR
            // WebGL requires async initialization - start the async process
            _ = InitializeWebGLAsync();
#else
            InitializeDefault();
            _isInitialized = true;
            Debug.Log("[GameDataManager] Initialization completed");
#endif
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        /// <summary>
        /// Async initialization for WebGL platform
        /// </summary>
        public static async System.Threading.Tasks.Task InitializeWebGLAsync()
        {
            Debug.Log("[GameDataManager] Using WebGL async initialization mode");

            var initTasks = new System.Collections.Generic.List<System.Threading.Tasks.Task>();

            // Start all config initializations in parallel
            initTasks.Add(InitializeConfigAsync<YuankunHuang.Unity.GameDataConfig.AccountTestData>("AccountTestConfig",
                () => YuankunHuang.Unity.GameDataConfig.BaseConfigData<YuankunHuang.Unity.GameDataConfig.AccountTestData>.InitializeAsync(
                    System.IO.Path.Combine(UnityEngine.Application.streamingAssetsPath, "ConfigData", "AccountTest.data"))));

            initTasks.Add(InitializeConfigAsync<YuankunHuang.Unity.GameDataConfig.AudioData>("AudioConfig",
                () => YuankunHuang.Unity.GameDataConfig.BaseConfigData<YuankunHuang.Unity.GameDataConfig.AudioData>.InitializeAsync(
                    System.IO.Path.Combine(UnityEngine.Application.streamingAssetsPath, "ConfigData", "Audio.data"))));

            initTasks.Add(InitializeConfigAsync<YuankunHuang.Unity.GameDataConfig.AvatarData>("AvatarConfig",
                () => YuankunHuang.Unity.GameDataConfig.BaseConfigData<YuankunHuang.Unity.GameDataConfig.AvatarData>.InitializeAsync(
                    System.IO.Path.Combine(UnityEngine.Application.streamingAssetsPath, "ConfigData", "Avatar.data"))));

            initTasks.Add(InitializeConfigAsync<YuankunHuang.Unity.GameDataConfig.LanguageData>("LanguageConfig",
                () => YuankunHuang.Unity.GameDataConfig.BaseConfigData<YuankunHuang.Unity.GameDataConfig.LanguageData>.InitializeAsync(
                    System.IO.Path.Combine(UnityEngine.Application.streamingAssetsPath, "ConfigData", "Language.data"))));

            initTasks.Add(InitializeConfigAsync<YuankunHuang.Unity.GameDataConfig.SampleData>("SampleConfig",
                () => YuankunHuang.Unity.GameDataConfig.BaseConfigData<YuankunHuang.Unity.GameDataConfig.SampleData>.InitializeAsync(
                    System.IO.Path.Combine(UnityEngine.Application.streamingAssetsPath, "ConfigData", "Sample.data"))));

            // Wait for all configurations to load
            await System.Threading.Tasks.Task.WhenAll(initTasks);

            _isInitialized = true;
            Debug.Log("[GameDataManager] WebGL async initialization completed");
        }

        private static async System.Threading.Tasks.Task InitializeConfigAsync<T>(string configName, System.Func<System.Threading.Tasks.Task> initFunc)
        {
            try
            {
                await initFunc();
                Debug.Log($"[GameDataManager] {configName} initialized successfully");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[GameDataManager] {configName} failed: {e.Message}");
            }
        }
#endif

        /// <summary>
        /// Default initialization using reflection (for other platforms)
        /// </summary>
        private static void InitializeDefault()
        {
            Debug.Log("[GameDataManager] InitializeDefault ENTER");
            Debug.Log("[GameDataManager] Using reflection-based initialization mode");

            try
            {
                // Search in multiple assemblies for better detection
                var assemblies = new[]
                {
                    Assembly.GetExecutingAssembly(),
                    typeof(GameDataManager).Assembly
                };

                Debug.Log($"[GameDataManager] Searching in {assemblies.Length} assemblies");

                var allTypes = new System.Collections.Generic.List<System.Type>();
                foreach (var assembly in assemblies.Distinct())
                {
                    if (assembly != null)
                    {
                        Debug.Log($"[GameDataManager] Processing assembly: {assembly.FullName}");
                        try
                        {
                            var types = assembly.GetTypes();
                            Debug.Log($"[GameDataManager] Found {types.Length} types in assembly");
                            allTypes.AddRange(types);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"[GameDataManager] Failed to get types from assembly {assembly.FullName}: {e.Message}");
                    }
                }
            }

                Debug.Log($"[GameDataManager] Total types collected: {allTypes.Count}");

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
                    }).ToList();

                Debug.Log($"[GameDataManager] Found {configTypes.Count} config types to initialize");

                foreach (var type in configTypes)
                {
                    Debug.Log($"[GameDataManager] Initializing {type.Name}...");

                    var method = type.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Static, null, Type.EmptyTypes, null);
                    if (method != null)
                    {
                        try
                        {
                            method.Invoke(null, null);
                            Debug.Log($"[GameDataManager] {type.Name} initialized successfully");
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"[GameDataManager] {type.Name} failed: {e.Message}");
                            Debug.LogException(e);
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[GameDataManager] No Initialize method found for {type.Name}");
                    }
                }

                Debug.Log("[GameDataManager] InitializeDefault COMPLETE");
            }
            catch (Exception e)
            {
                Debug.LogError($"[GameDataManager] InitializeDefault FAILED: {e.Message}");
                Debug.LogException(e);
            }
        }
    }
}
