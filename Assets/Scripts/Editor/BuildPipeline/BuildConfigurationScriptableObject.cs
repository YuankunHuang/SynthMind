using System.IO;
using UnityEditor;
using UnityEngine;

namespace YuankunHuang.Unity.Editor.BuildPipeline
{
    [CreateAssetMenu(fileName = "BuildConfiguration", menuName = "SynthMind/Build Configuration")]
    public class BuildConfigurationScriptableObject : ScriptableObject
    {
        [Header("Build Information")]
        public string buildName = "SynthMind";
        public string companyName = "Yuankun Huang";
        public string version = "1.0.0";
        
        [Header("Build Settings")]
        public BuildTarget targetPlatform = BuildTarget.StandaloneWindows64;
        public BuildProfile buildProfile = BuildProfile.Development;
        
        [Header("Optimization")]
        public bool stripEngineCode = true;
        public bool useIncrementalGC = true;
        public ScriptingImplementation scriptingBackend = ScriptingImplementation.IL2CPP;
        public Il2CppCompilerConfiguration il2CppCompilerConfiguration = Il2CppCompilerConfiguration.Release;
        
        [Header("Addressables")]
        public bool buildAddressables = true;
        public bool cleanAddressablesBeforeBuild = true;
        
        [Header("Post Build")]
        public bool openFolderAfterBuild = true;
        public bool createZipArchive = false;
        public bool copyReadmeFile = true;
        
        [Header("Custom Build Path")]
        public bool useCustomBuildPath = false;
        public string customBuildPath = "";
        
        public enum BuildProfile
        {
            Development,
            Release,
            Master
        }
        
        public void ApplySettings()
        {
            // Apply Player Settings
            PlayerSettings.companyName = companyName;
            PlayerSettings.productName = buildName;
            PlayerSettings.bundleVersion = version;
            
            // Apply build optimization settings
            PlayerSettings.stripEngineCode = stripEngineCode;
            PlayerSettings.gcIncremental = useIncrementalGC;
            PlayerSettings.SetScriptingBackend(EditorUserBuildSettings.selectedBuildTargetGroup, scriptingBackend);
            PlayerSettings.SetIl2CppCompilerConfiguration(EditorUserBuildSettings.selectedBuildTargetGroup, il2CppCompilerConfiguration);
            
            // Apply build profile specific settings
            switch (buildProfile)
            {
                case BuildProfile.Development:
                    EditorUserBuildSettings.development = true;
                    EditorUserBuildSettings.allowDebugging = true;
                    PlayerSettings.SetStackTraceLogType(LogType.Error, StackTraceLogType.ScriptOnly);
                    PlayerSettings.SetStackTraceLogType(LogType.Assert, StackTraceLogType.ScriptOnly);
                    PlayerSettings.SetStackTraceLogType(LogType.Warning, StackTraceLogType.ScriptOnly);
                    PlayerSettings.SetStackTraceLogType(LogType.Log, StackTraceLogType.ScriptOnly);
                    break;
                    
                case BuildProfile.Release:
                    EditorUserBuildSettings.development = false;
                    EditorUserBuildSettings.allowDebugging = false;
                    PlayerSettings.SetStackTraceLogType(LogType.Error, StackTraceLogType.ScriptOnly);
                    PlayerSettings.SetStackTraceLogType(LogType.Assert, StackTraceLogType.None);
                    PlayerSettings.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
                    PlayerSettings.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
                    break;
                    
                case BuildProfile.Master:
                    EditorUserBuildSettings.development = false;
                    EditorUserBuildSettings.allowDebugging = false;
                    // Disable all logging in master builds
                    PlayerSettings.SetStackTraceLogType(LogType.Error, StackTraceLogType.None);
                    PlayerSettings.SetStackTraceLogType(LogType.Assert, StackTraceLogType.None);
                    PlayerSettings.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
                    PlayerSettings.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
                    break;
            }
            
            Debug.Log($"✅ Applied build configuration: {buildProfile} for {targetPlatform}");
        }
        
        public string GetBuildPath()
        {
            if (useCustomBuildPath && !string.IsNullOrEmpty(customBuildPath))
            {
                return customBuildPath;
            }

            var projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return System.IO.Path.Combine(projectRoot, "Builds");
        }
    }
}