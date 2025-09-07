using UnityEditor;
using UnityEngine;
using System;
using System.Linq;

namespace YuankunHuang.Unity.Editor.BuildPipeline
{
    /// <summary>
    /// Command line build helper for CI/CD automation
    /// Usage: Unity -batchmode -quit -projectPath "C:\Project" -executeMethod CommandLineBuildHelper.BuildWindows64
    /// </summary>
    public static class CommandLineBuildHelper
    {
        /// <summary>
        /// Build for Windows 64-bit from command line
        /// </summary>
        public static void BuildWindows64()
        {
            BuildForTarget(BuildTarget.StandaloneWindows64);
        }
        
        /// <summary>
        /// Build for Windows 32-bit from command line
        /// </summary>
        public static void BuildWindows32()
        {
            BuildForTarget(BuildTarget.StandaloneWindows);
        }
        
        /// <summary>
        /// Build for macOS from command line
        /// </summary>
        public static void BuildMacOS()
        {
            BuildForTarget(BuildTarget.StandaloneOSX);
        }
        
        /// <summary>
        /// Build for Linux from command line
        /// </summary>
        public static void BuildLinux64()
        {
            BuildForTarget(BuildTarget.StandaloneLinux64);
        }
        
        private static void BuildForTarget(BuildTarget target)
        {
            try
            {
                Debug.Log($"🚀 Starting command line build for {target}");
                
                // Parse command line arguments
                var args = ParseCommandLineArgs();
                
                // Get build configuration
                var profile = args.ContainsKey("profile") ? 
                    (BuildConfigurationScriptableObject.BuildProfile)Enum.Parse(typeof(BuildConfigurationScriptableObject.BuildProfile), args["profile"]) : 
                    BuildConfigurationScriptableObject.BuildProfile.Release;
                
                var version = args.ContainsKey("version") ? args["version"] : "1.0.0";
                var buildPath = args.ContainsKey("buildPath") ? args["buildPath"] : "";
                
                // Apply build settings
                ApplyBuildSettings(target, profile, version);
                
                // Build Addressables first
                Debug.Log("🔧 Building Addressables...");
                UnityEditor.AddressableAssets.Settings.AddressableAssetSettings.BuildPlayerContent();
                
                // Get scenes to build
                var scenePaths = EditorBuildSettings.scenes
                    .Where(scene => scene.enabled)
                    .Select(scene => scene.path)
                    .ToArray();
                
                if (scenePaths.Length == 0)
                {
                    throw new Exception("No scenes found in build settings!");
                }
                
                // Setup build options
                var buildOptions = new BuildPlayerOptions
                {
                    scenes = scenePaths,
                    locationPathName = GetBuildPath(target, profile, version, buildPath),
                    target = target,
                    options = GetBuildOptions(profile)
                };
                
                Debug.Log($"📁 Build path: {buildOptions.locationPathName}");
                
                // Execute build
                var report = UnityEditor.BuildPipeline.BuildPlayer(buildOptions);
                
                // Check result
                if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
                {
                    Debug.Log($"✅ Build succeeded! Time: {report.summary.totalTime}, Size: {report.summary.totalSize} bytes");
                    
                    // Exit with success code
                    if (Application.isBatchMode)
                    {
                        EditorApplication.Exit(0);
                    }
                }
                else
                {
                    Debug.LogError($"❌ Build failed: {report.summary.result}");
                    
                    // Print build errors
                    foreach (var step in report.steps)
                    {
                        foreach (var message in step.messages)
                        {
                            if (message.type == LogType.Error || message.type == LogType.Exception)
                            {
                                Debug.LogError($"Build Error: {message.content}");
                            }
                        }
                    }
                    
                    // Exit with error code
                    if (Application.isBatchMode)
                    {
                        EditorApplication.Exit(1);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Command line build failed: {ex.Message}");
                Debug.LogError(ex.StackTrace);
                
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
            }
        }
        
        private static System.Collections.Generic.Dictionary<string, string> ParseCommandLineArgs()
        {
            var args = new System.Collections.Generic.Dictionary<string, string>();
            var commandLineArgs = System.Environment.GetCommandLineArgs();
            
            for (int i = 0; i < commandLineArgs.Length - 1; i++)
            {
                if (commandLineArgs[i].StartsWith("-"))
                {
                    var key = commandLineArgs[i].Substring(1);
                    var value = commandLineArgs[i + 1];
                    args[key] = value;
                }
            }
            
            return args;
        }
        
        private static void ApplyBuildSettings(BuildTarget target, BuildConfigurationScriptableObject.BuildProfile profile, string version)
        {
            // Set target platform  
            EditorUserBuildSettings.SwitchActiveBuildTarget(UnityEditor.BuildPipeline.GetBuildTargetGroup(target), target);
            
            // Set version
            PlayerSettings.bundleVersion = version;
            
            // Apply profile settings
            switch (profile)
            {
                case BuildConfigurationScriptableObject.BuildProfile.Development:
                    EditorUserBuildSettings.development = true;
                    EditorUserBuildSettings.allowDebugging = true;
                    break;
                    
                case BuildConfigurationScriptableObject.BuildProfile.Release:
                    EditorUserBuildSettings.development = false;
                    EditorUserBuildSettings.allowDebugging = false;
                    break;
                    
                case BuildConfigurationScriptableObject.BuildProfile.Master:
                    EditorUserBuildSettings.development = false;
                    EditorUserBuildSettings.allowDebugging = false;
                    PlayerSettings.stripEngineCode = true;
                    break;
            }
            
            Debug.Log($"⚙️ Applied build settings: {profile} profile for {target}");
        }
        
        private static string GetBuildPath(BuildTarget target, BuildConfigurationScriptableObject.BuildProfile profile, string version, string customPath)
        {
            var basePath = string.IsNullOrEmpty(customPath) ? 
                System.IO.Path.Combine(Application.dataPath, "..", "Builds") : 
                customPath;
                
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var folderName = $"SynthMind_{target}_{profile}_{version}_{timestamp}";
            var buildFolder = System.IO.Path.Combine(basePath, folderName);
            
            var executable = GetExecutableName(target);
            return System.IO.Path.Combine(buildFolder, executable);
        }
        
        private static string GetExecutableName(BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "SynthMind.exe";
                case BuildTarget.StandaloneOSX:
                    return "SynthMind.app";
                case BuildTarget.StandaloneLinux64:
                    return "SynthMind";
                default:
                    return "SynthMind";
            }
        }
        
        private static BuildOptions GetBuildOptions(BuildConfigurationScriptableObject.BuildProfile profile)
        {
            var options = BuildOptions.None;
            
            switch (profile)
            {
                case BuildConfigurationScriptableObject.BuildProfile.Development:
                    options |= BuildOptions.Development;
                    options |= BuildOptions.AllowDebugging;
                    break;
                    
                case BuildConfigurationScriptableObject.BuildProfile.Release:
                    // No special options
                    break;
                    
                case BuildConfigurationScriptableObject.BuildProfile.Master:
                    // Additional optimizations could be added here
                    break;
            }
            
            return options;
        }
    }
}