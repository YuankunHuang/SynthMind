using System;
using System.Reflection;
using System.Linq;
using YuankunHuang.Unity.Core;

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

            LogHelper.Log("[GameDataManager] Starting initialization...");

#if UNITY_WEBGL && !UNITY_EDITOR
            // WebGL requires async initialization - start the async process
            _ = InitializeWebGLAsync();
#else
            InitializeDefault();
            _isInitialized = true;
            LogHelper.Log("[GameDataManager] Initialization completed");
#endif
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        /// <summary>
        /// Async initialization for WebGL platform
        /// </summary>
        public static async System.Threading.Tasks.Task InitializeWebGLAsync()
        {
            LogHelper.Log("[GameDataManager] Using WebGL async initialization mode");

            var initTasks = new System.Collections.Generic.List<System.Threading.Tasks.Task>();

            // Start all config initializations in parallel
            initTasks.Add(InitializeConfigAsync<YuankunHuang.Unity.GameDataConfig.AccountTestData>("AccountTestConfig",
                () => YuankunHuang.Unity.GameDataConfig.AccountTestConfig.InitializeAsync(
                    System.IO.Path.Combine(UnityEngine.Application.streamingAssetsPath, "ConfigData", "AccountTest.data"))));

            initTasks.Add(InitializeConfigAsync<YuankunHuang.Unity.GameDataConfig.AudioData>("AudioConfig",
                () => YuankunHuang.Unity.GameDataConfig.AudioConfig.InitializeAsync(
                    System.IO.Path.Combine(UnityEngine.Application.streamingAssetsPath, "ConfigData", "Audio.data"))));

            initTasks.Add(InitializeConfigAsync<YuankunHuang.Unity.GameDataConfig.AvatarData>("AvatarConfig",
                () => YuankunHuang.Unity.GameDataConfig.AvatarConfig.InitializeAsync(
                    System.IO.Path.Combine(UnityEngine.Application.streamingAssetsPath, "ConfigData", "Avatar.data"))));

            initTasks.Add(InitializeConfigAsync<YuankunHuang.Unity.GameDataConfig.LanguageData>("LanguageConfig",
                () => YuankunHuang.Unity.GameDataConfig.LanguageConfig.InitializeAsync(
                    System.IO.Path.Combine(UnityEngine.Application.streamingAssetsPath, "ConfigData", "Language.data"))));

            initTasks.Add(InitializeConfigAsync<YuankunHuang.Unity.GameDataConfig.SampleData>("SampleConfig",
                () => YuankunHuang.Unity.GameDataConfig.SampleConfig.InitializeAsync(
                    System.IO.Path.Combine(UnityEngine.Application.streamingAssetsPath, "ConfigData", "Sample.data"))));

            // Wait for all configurations to load
            await System.Threading.Tasks.Task.WhenAll(initTasks);

            _isInitialized = true;
            LogHelper.Log("[GameDataManager] WebGL async initialization completed");
        }

        private static async System.Threading.Tasks.Task InitializeConfigAsync<T>(string configName, System.Func<System.Threading.Tasks.Task> initFunc)
        {
            try
            {
                await initFunc();
                LogHelper.Log($"[GameDataManager] {configName} initialized successfully");
            }
            catch (System.Exception e)
            {
                LogHelper.LogWarning($"[GameDataManager] {configName} failed: {e.Message}");
            }
        }
#endif

                                                        /// <summary>
        /// WebGL-compatible initialization (auto-generated)
        /// </summary>
        private static void InitializeWebGL()
        {
            LogHelper.Log("[GameDataManager] Using WebGL initialization mode");

            try { AccountTestConfig.Initialize(); LogHelper.Log("[GameDataManager] AccountTestConfig initialized"); }
            catch (Exception e) { LogHelper.LogWarning($"[GameDataManager] AccountTestConfig failed: {e.Message}"); }

            try { AudioConfig.Initialize(); LogHelper.Log("[GameDataManager] AudioConfig initialized"); }
            catch (Exception e) { LogHelper.LogWarning($"[GameDataManager] AudioConfig failed: {e.Message}"); }

            try { AvatarConfig.Initialize(); LogHelper.Log("[GameDataManager] AvatarConfig initialized"); }
            catch (Exception e) { LogHelper.LogWarning($"[GameDataManager] AvatarConfig failed: {e.Message}"); }

            try { LanguageConfig.Initialize(); LogHelper.Log("[GameDataManager] LanguageConfig initialized"); }
            catch (Exception e) { LogHelper.LogWarning($"[GameDataManager] LanguageConfig failed: {e.Message}"); }

            try { SampleConfig.Initialize(); LogHelper.Log("[GameDataManager] SampleConfig initialized"); }
            catch (Exception e) { LogHelper.LogWarning($"[GameDataManager] SampleConfig failed: {e.Message}"); }

        }







        /// <summary>
        /// Default initialization using reflection (for other platforms)
        /// </summary>
        private static void InitializeDefault()
        {
            LogHelper.Log("[GameDataManager] Using reflection-based initialization mode");

            // Search in multiple assemblies for better detection
            var assemblies = new[]
            {
                Assembly.GetExecutingAssembly(),
                typeof(GameDataManager).Assembly
            };

            var allTypes = new System.Collections.Generic.List<System.Type>();
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
                        LogHelper.LogWarning($"[GameDataManager] Failed to get types from assembly {assembly.FullName}: {e.Message}");
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
                });

            foreach (var type in configTypes)
            {
                var method = type.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Static, null, Type.EmptyTypes, null);
                if (method != null)
                {
                    try
                    {
                        method.Invoke(null, null);
                        LogHelper.Log($"[GameDataManager] {type.Name} initialized");
                    }
                    catch (Exception e)
                    {
                        LogHelper.LogWarning($"[GameDataManager] {type.Name} failed: {e.Message}");
                    }
                }
            }
        }
    }
}
