using System;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEditor.Build.Reporting;
using YuankunHuang.Unity.Core;

namespace YuankunHuang.Unity.Editor.BuildPipeline
{
    public class BuildPipelineManager : EditorWindow
    {
        [MenuItem("SynthMind/Build Pipeline")]
        public static void ShowWindow()
        {
            GetWindow<BuildPipelineManager>("SynthMind Build Pipeline");
        }

        private BuildTarget _selectedTarget = BuildTarget.StandaloneWindows64;
        private BuildProfile _selectedProfile = BuildProfile.Development;
        private string _buildVersion = "1.0.0";
        private string _customBuildPath = "";
        private bool _buildAddressables = true;
        private bool _autoOpenFolder = true;
        private bool _createZip = false;
        
        private enum BuildProfile
        {
            Development,
            Release,
            Master
        }

        private void OnGUI()
        {
            GUILayout.Label("SynthMind Build Pipeline", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Build Configuration
            EditorGUILayout.LabelField("Build Configuration", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            
            _selectedTarget = (BuildTarget)EditorGUILayout.EnumPopup("Target Platform", _selectedTarget);
            _selectedProfile = (BuildProfile)EditorGUILayout.EnumPopup("Build Profile", _selectedProfile);
            _buildVersion = EditorGUILayout.TextField("Version", _buildVersion);
            
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            // Build Options
            EditorGUILayout.LabelField("Build Options", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            
            _buildAddressables = EditorGUILayout.Toggle("Build Addressables", _buildAddressables);
            _autoOpenFolder = EditorGUILayout.Toggle("Open Build Folder", _autoOpenFolder);
            _createZip = EditorGUILayout.Toggle("Create ZIP Archive", _createZip);
            
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            // Build Path
            EditorGUILayout.LabelField("Build Path", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            
            EditorGUILayout.BeginHorizontal();
            _customBuildPath = EditorGUILayout.TextField("Custom Path", _customBuildPath);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("Select Build Directory", "", "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    _customBuildPath = selectedPath;
                }
            }
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("Reset to Default"))
            {
                _customBuildPath = "";
            }
            
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            // Build Info Display
            var buildPath = GetBuildPath();
            EditorGUILayout.HelpBox($"Build will be created at:\n{buildPath}", MessageType.Info);
            EditorGUILayout.Space();

            // Build Buttons
            EditorGUILayout.BeginHorizontal();
            
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("🚀 Build Now", GUILayout.Height(30)))
            {
                BuildGame();
            }
            
            GUI.backgroundColor = Color.yellow;
            if (GUILayout.Button("🔧 Build Addressables Only", GUILayout.Height(30)))
            {
                BuildAddressablesOnly();
            }
            
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("🗑️ Clean Build", GUILayout.Height(30)))
            {
                CleanBuild();
            }
            
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // Quick Actions
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("📁 Open Build Folder"))
            {
                OpenBuildFolder();
            }
            
            if (GUILayout.Button("📊 Player Settings"))
            {
                SettingsService.OpenProjectSettings("Project/Player");
            }
            
            if (GUILayout.Button("🎯 Build Settings"))
            {
                EditorWindow.GetWindow(System.Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private string GetBuildPath()
        {
            if (!string.IsNullOrEmpty(_customBuildPath))
            {
                return Path.Combine(_customBuildPath, GetBuildFolderName());
            }
            
            return Path.Combine(Application.dataPath, "..", "Builds", GetBuildFolderName());
        }

        private string GetBuildFolderName()
        {
            var targetName = _selectedTarget.ToString();
            var profileName = _selectedProfile.ToString();
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            
            return $"SynthMind_{targetName}_{profileName}_{_buildVersion}_{timestamp}";
        }

        private void BuildGame()
        {
            try
            {
                EditorUtility.DisplayProgressBar("SynthMind Build Pipeline", "Preparing build...", 0.0f);
                
                // Step 1: Validate build settings
                if (!ValidateBuildSettings())
                {
                    EditorUtility.ClearProgressBar();
                    return;
                }
                
                // Step 2: Apply build profile settings
                ApplyBuildProfile();
                
                // Step 3: Build Addressables if requested
                if (_buildAddressables)
                {
                    EditorUtility.DisplayProgressBar("SynthMind Build Pipeline", "Building Addressables...", 0.2f);
                    BuildAddressables();
                }
                
                // Step 4: Build the player
                EditorUtility.DisplayProgressBar("SynthMind Build Pipeline", "Building player...", 0.5f);
                var buildReport = BuildPlayer();
                
                // Step 5: Post-build processing
                EditorUtility.DisplayProgressBar("SynthMind Build Pipeline", "Post-build processing...", 0.8f);
                PostBuildProcessing(buildReport);
                
                EditorUtility.ClearProgressBar();
                
                if (buildReport.summary.result == BuildResult.Succeeded)
                {
                    UnityEngine.Debug.Log($"✅ Build completed successfully!\nBuild path: {buildReport.summary.outputPath}");
                    
                    if (_autoOpenFolder)
                    {
                        OpenBuildFolder();
                    }
                    
                    EditorUtility.DisplayDialog("Build Complete", 
                        $"Build completed successfully!\n\nBuild time: {buildReport.summary.totalTime}\nSize: {FormatBytes(buildReport.summary.totalSize)}", 
                        "OK");
                }
                else
                {
                    UnityEngine.Debug.LogError($"❌ Build failed: {buildReport.summary.result}");
                    EditorUtility.DisplayDialog("Build Failed", $"Build failed: {buildReport.summary.result}", "OK");
                }
            }
            catch (Exception ex)
            {
                EditorUtility.ClearProgressBar();
                UnityEngine.Debug.LogError($"❌ Build pipeline error: {ex.Message}");
                EditorUtility.DisplayDialog("Build Error", $"Build pipeline error:\n{ex.Message}", "OK");
            }
        }

        private bool ValidateBuildSettings()
        {
            // Check if scenes are added to build
            if (EditorBuildSettings.scenes.Length == 0)
            {
                UnityEngine.Debug.LogError("❌ No scenes added to build settings!");
                return false;
            }

            // Check if Addressables is properly configured
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                LogHelper.LogWarning("⚠️ Addressables not configured, but continuing build...");
            }

            return true;
        }

        private void ApplyBuildProfile()
        {
            // Set player settings based on profile
            switch (_selectedProfile)
            {
                case BuildProfile.Development:
                    EditorUserBuildSettings.development = true;
                    EditorUserBuildSettings.allowDebugging = true;
                    EditorUserBuildSettings.buildScriptsOnly = false;
                    break;
                    
                case BuildProfile.Release:
                    EditorUserBuildSettings.development = false;
                    EditorUserBuildSettings.allowDebugging = false;
                    EditorUserBuildSettings.buildScriptsOnly = false;
                    break;
                    
                case BuildProfile.Master:
                    EditorUserBuildSettings.development = false;
                    EditorUserBuildSettings.allowDebugging = false;
                    EditorUserBuildSettings.buildScriptsOnly = false;
                    // Additional master build optimizations could go here
                    break;
            }

            // Set version
            PlayerSettings.bundleVersion = _buildVersion;
        }

        private void BuildAddressables()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                LogHelper.LogWarning("⚠️ Addressables settings not found, skipping...");
                return;
            }

            UnityEngine.Debug.Log("🔧 Building Addressables...");
            AddressableAssetSettings.BuildPlayerContent();
            UnityEngine.Debug.Log("✅ Addressables build completed");
        }

        private BuildReport BuildPlayer()
        {
            var buildPath = GetBuildPath();
            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = GetEnabledScenePaths(),
                locationPathName = Path.Combine(buildPath, GetExecutableName()),
                target = _selectedTarget,
                options = GetBuildOptions()
            };

            UnityEngine.Debug.Log($"🚀 Starting build to: {buildPlayerOptions.locationPathName}");
            return UnityEditor.BuildPipeline.BuildPlayer(buildPlayerOptions);
        }

        private string[] GetEnabledScenePaths()
        {
            var scenes = new string[EditorBuildSettings.scenes.Length];
            for (int i = 0; i < scenes.Length; i++)
            {
                scenes[i] = EditorBuildSettings.scenes[i].path;
            }
            return scenes;
        }

        private string GetExecutableName()
        {
            switch (_selectedTarget)
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

        private BuildOptions GetBuildOptions()
        {
            BuildOptions options = BuildOptions.None;
            
            switch (_selectedProfile)
            {
                case BuildProfile.Development:
                    options |= BuildOptions.Development;
                    options |= BuildOptions.AllowDebugging;
                    break;
                case BuildProfile.Release:
                    // No special options for release
                    break;
                case BuildProfile.Master:
                    // Additional optimizations for master builds
                    break;
            }

            return options;
        }

        private void PostBuildProcessing(BuildReport buildReport)
        {
            if (buildReport.summary.result != BuildResult.Succeeded)
                return;

            var buildPath = Path.GetDirectoryName(buildReport.summary.outputPath);
            
            // Copy additional files if needed
            CopyAdditionalFiles(buildPath);
            
            // Create ZIP if requested
            if (_createZip)
            {
                CreateZipArchive(buildPath);
            }
        }

        private void CopyAdditionalFiles(string buildPath)
        {
            // Copy README, license, or other files
            // Example: Copy a README file to the build directory
            /*
            var readmePath = Path.Combine(Application.dataPath, "..", "README.md");
            if (File.Exists(readmePath))
            {
                File.Copy(readmePath, Path.Combine(buildPath, "README.md"), true);
            }
            */
        }

        private void CreateZipArchive(string buildPath)
        {
            try
            {
                var zipPath = buildPath + ".zip";
                UnityEngine.Debug.Log($"📦 Creating ZIP archive: {zipPath}");
                
                // This would require System.IO.Compression or a third-party library
                // For now, just log the intention
                UnityEngine.Debug.Log("💡 ZIP creation not implemented - consider adding System.IO.Compression");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"❌ Failed to create ZIP: {ex.Message}");
            }
        }

        private void BuildAddressablesOnly()
        {
            try
            {
                EditorUtility.DisplayProgressBar("Build Pipeline", "Building Addressables...", 0.5f);
                BuildAddressables();
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Addressables Build Complete", "Addressables have been built successfully!", "OK");
            }
            catch (Exception ex)
            {
                EditorUtility.ClearProgressBar();
                UnityEngine.Debug.LogError($"❌ Addressables build failed: {ex.Message}");
                EditorUtility.DisplayDialog("Build Failed", $"Addressables build failed:\n{ex.Message}", "OK");
            }
        }

        private void CleanBuild()
        {
            if (EditorUtility.DisplayDialog("Clean Build", "This will delete all build files and Addressables cache. Continue?", "Yes", "Cancel"))
            {
                try
                {
                    var buildDir = Path.Combine(Application.dataPath, "..", "Builds");
                    if (Directory.Exists(buildDir))
                    {
                        Directory.Delete(buildDir, true);
                        UnityEngine.Debug.Log("🗑️ Cleaned build directory");
                    }

                    // Clean Addressables cache
                    var addressableSettings = AddressableAssetSettingsDefaultObject.Settings;
                    if (addressableSettings != null)
                    {
                        AddressableAssetSettings.CleanPlayerContent();
                        UnityEngine.Debug.Log("🗑️ Cleaned Addressables cache");
                    }

                    EditorUtility.DisplayDialog("Clean Complete", "Build cache has been cleaned!", "OK");
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"❌ Clean failed: {ex.Message}");
                }
            }
        }

        private void OpenBuildFolder()
        {
            var buildDir = string.IsNullOrEmpty(_customBuildPath) 
                ? Path.Combine(Application.dataPath, "..", "Builds")
                : _customBuildPath;
                
            if (Directory.Exists(buildDir))
            {
                System.Diagnostics.Process.Start("explorer.exe", buildDir.Replace("/", "\\"));
            }
            else
            {
                EditorUtility.DisplayDialog("Folder Not Found", $"Build directory does not exist:\n{buildDir}", "OK");
            }
        }

        private string FormatBytes(ulong bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            double size = bytes;
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size = size / 1024;
            }
            return $"{size:0.##} {sizes[order]}";
        }
    }
}