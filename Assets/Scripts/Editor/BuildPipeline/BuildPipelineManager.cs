using System;
using System.IO;
using System.IO.Compression;
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
        [MenuItem("Tools/Build Pipeline")]
        public static void ShowWindow()
        {
            var window = GetWindow<BuildPipelineManager>("SynthMind Build Pipeline");
            window.LoadSettings();
        }

        private BuildTarget _selectedTarget = BuildTarget.StandaloneWindows64;
        private BuildProfile _selectedProfile = BuildProfile.Development;
        private string _buildVersion = "1.0.0";
        private string _customBuildPath = "";
        private bool _buildAddressables = true;
        private bool _autoOpenFolder = true;
        private bool _createZip = false;
        private bool _showBuildHistory = false;
        private UnityEngine.Vector2 _historyScrollPosition;
        
        private const string PREF_TARGET = "SynthMind.BuildPipeline.Target";
        private const string PREF_PROFILE = "SynthMind.BuildPipeline.Profile";
        private const string PREF_VERSION = "SynthMind.BuildPipeline.Version";
        private const string PREF_CUSTOM_PATH = "SynthMind.BuildPipeline.CustomPath";
        private const string PREF_BUILD_ADDRESSABLES = "SynthMind.BuildPipeline.BuildAddressables";
        private const string PREF_AUTO_OPEN = "SynthMind.BuildPipeline.AutoOpen";
        private const string PREF_CREATE_ZIP = "SynthMind.BuildPipeline.CreateZip";
        
        private enum BuildProfile
        {
            Development,
            Release,
            Master
        }

        private void OnGUI()
        {
            // Build Configuration
            EditorGUILayout.LabelField("Build Configuration", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            
            EditorGUI.BeginChangeCheck();
            _selectedTarget = (BuildTarget)EditorGUILayout.EnumPopup("Target Platform", _selectedTarget);
            _selectedProfile = (BuildProfile)EditorGUILayout.EnumPopup("Build Profile", _selectedProfile);
            
            // Version with auto-increment
            EditorGUILayout.BeginHorizontal();
            _buildVersion = EditorGUILayout.TextField("Version", _buildVersion);
            if (GUILayout.Button("++", GUILayout.Width(30)))
            {
                _buildVersion = IncrementVersion(_buildVersion);
            }
            if (GUILayout.Button("Reset", GUILayout.Width(50)))
            {
                _buildVersion = "1.0.0";
            }
            EditorGUILayout.EndHorizontal();
            
            if (EditorGUI.EndChangeCheck())
            {
                SaveSettings();
            }
            
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            // Build Options
            EditorGUILayout.LabelField("Build Options", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            
            EditorGUI.BeginChangeCheck();
            _buildAddressables = EditorGUILayout.Toggle("Build Addressables", _buildAddressables);
            _autoOpenFolder = EditorGUILayout.Toggle("Open Build Folder", _autoOpenFolder);
            _createZip = EditorGUILayout.Toggle("Create ZIP Archive", _createZip);
            if (EditorGUI.EndChangeCheck())
            {
                SaveSettings();
            }
            
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            // Build Path
            EditorGUILayout.LabelField("Build Path", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            
            EditorGUI.BeginChangeCheck();
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
            
            if (EditorGUI.EndChangeCheck())
            {
                SaveSettings();
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
            if (GUILayout.Button("Build Now", GUILayout.Height(30)))
            {
                BuildGame();
            }
            
            GUI.backgroundColor = Color.yellow;
            if (GUILayout.Button("Build Addressables Only", GUILayout.Height(30)))
            {
                BuildAddressablesOnly();
            }
            
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Clean Build", GUILayout.Height(30)))
            {
                CleanBuild();
            }
            
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // Quick Actions
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Open Build Folder"))
            {
                OpenBuildFolder();
            }
            
            if (GUILayout.Button("Player Settings"))
            {
                SettingsService.OpenProjectSettings("Project/Player");
            }
            
            if (GUILayout.Button("Build Settings"))
            {
                EditorWindow.GetWindow(System.Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
            }
            
            if (GUILayout.Button("View Last Report"))
            {
                var history = BuildHistoryManager.LoadHistory();
                if (history.entries.Count > 0)
                {
                    var lastBuild = history.entries[0];
                    EditorUtility.DisplayDialog("Last Build Info", 
                        $"Version: {lastBuild.buildVersion}\n" +
                        $"Target: {lastBuild.buildTarget}\n" +
                        $"Result: {lastBuild.buildResult}\n" +
                        $"Duration: {lastBuild.buildDuration:mm\\:ss}\n" +
                        $"Size: {FormatBytes((ulong)lastBuild.buildSize)}\n" +
                        $"Path: {lastBuild.buildPath}", "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("No Build History", "No previous builds found.", "OK");
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // Build History
            _showBuildHistory = EditorGUILayout.Foldout(_showBuildHistory, "Build History", true);
            if (_showBuildHistory)
            {
                DrawBuildHistory();
            }
        }

        private string GetBuildPath()
        {
            if (!string.IsNullOrEmpty(_customBuildPath))
            {
                return Path.Combine(_customBuildPath, GetBuildFolderName());
            }

            var projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Path.Combine(projectRoot, "Builds", GetBuildFolderName());
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
            var startTime = DateTime.Now;
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
                
                // Step 6: Record build history
                var endTime = DateTime.Now;
                var duration = endTime - startTime;
                RecordBuildHistory(buildReport, startTime, duration);
                
                if (buildReport.summary.result == BuildResult.Succeeded)
                {
                    LogHelper.Log($"Build completed successfully!\nBuild path: {buildReport.summary.outputPath}");
                    
                    if (_autoOpenFolder)
                    {
                        OpenBuildFolder();
                    }
                    
                    var dialogResult = EditorUtility.DisplayDialogComplex("Build Complete", 
                        $"Build completed successfully!\n\nBuild time: {buildReport.summary.totalTime}\nSize: {FormatBytes(buildReport.summary.totalSize)}", 
                        "OK", "View Report", "");
                    
                    if (dialogResult == 1) // View Report button
                    {
                        BuildReportWindow.ShowReport(buildReport);
                    }
                }
                else
                {
                    LogHelper.LogError($"Build failed: {buildReport.summary.result}");
                    
                    var dialogResult = EditorUtility.DisplayDialogComplex("Build Failed", 
                        $"Build failed: {buildReport.summary.result}\n\nErrors: {buildReport.summary.totalErrors}\nWarnings: {buildReport.summary.totalWarnings}", 
                        "OK", "View Report", "");
                    
                    if (dialogResult == 1) // View Report button
                    {
                        BuildReportWindow.ShowReport(buildReport);
                    }
                }
            }
            catch (Exception ex)
            {
                EditorUtility.ClearProgressBar();
                LogHelper.LogError($"Build pipeline error: {ex.Message}");
                EditorUtility.DisplayDialog("Build Error", $"Build pipeline error:\n{ex.Message}", "OK");
                
                // Record failed build
                var endTime = DateTime.Now;
                var duration = endTime - startTime;
                RecordFailedBuild(startTime, duration, ex.Message);
            }
        }

        private bool ValidateBuildSettings()
        {
            // Check if scenes are added to build
            if (EditorBuildSettings.scenes.Length == 0)
            {
                LogHelper.LogError("No scenes added to build settings!");
                return false;
            }

            // Check if Addressables is properly configured
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                LogHelper.LogWarning("Addressables not configured, but continuing build...");
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
                LogHelper.LogWarning("Addressables settings not found, skipping...");
                return;
            }

            LogHelper.Log("Building Addressables...");
            AddressableAssetSettings.BuildPlayerContent();
            LogHelper.Log("Addressables build completed");
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

            LogHelper.Log($"Starting build to: {buildPlayerOptions.locationPathName}");
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
                LogHelper.Log($"Creating ZIP archive: {zipPath}");
                
                if (File.Exists(zipPath))
                {
                    File.Delete(zipPath);
                }
                
                ZipFile.CreateFromDirectory(buildPath, zipPath, System.IO.Compression.CompressionLevel.Optimal, false);
                
                var zipInfo = new FileInfo(zipPath);
                LogHelper.Log($"ZIP archive created successfully: {zipPath} ({FormatBytes((ulong)zipInfo.Length)})");
                
                EditorUtility.DisplayDialog("ZIP Created", 
                    $"ZIP archive created successfully!\n\nLocation: {zipPath}\nSize: {FormatBytes((ulong)zipInfo.Length)}", 
                    "OK");
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"Failed to create ZIP: {ex.Message}");
                EditorUtility.DisplayDialog("ZIP Creation Failed", $"Failed to create ZIP archive:\n{ex.Message}", "OK");
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
                LogHelper.LogError($"Addressables build failed: {ex.Message}");
                EditorUtility.DisplayDialog("Build Failed", $"Addressables build failed:\n{ex.Message}", "OK");
            }
        }

        private void CleanBuild()
        {
            if (EditorUtility.DisplayDialog("Clean Build", "This will delete all build files and Addressables cache. Continue?", "Yes", "Cancel"))
            {
                try
                {
                    var projectRoot = Directory.GetParent(Application.dataPath).FullName;
                    var buildDir = Path.Combine(projectRoot, "Builds");
                    if (Directory.Exists(buildDir))
                    {
                        Directory.Delete(buildDir, true);
                        LogHelper.Log("Cleaned build directory");
                    }

                    // Clean Addressables cache
                    var addressableSettings = AddressableAssetSettingsDefaultObject.Settings;
                    if (addressableSettings != null)
                    {
                        AddressableAssetSettings.CleanPlayerContent();
                        LogHelper.Log("Cleaned Addressables cache");
                    }

                    EditorUtility.DisplayDialog("Clean Complete", "Build cache has been cleaned!", "OK");
                }
                catch (Exception ex)
                {
                    LogHelper.LogError($"Clean failed: {ex.Message}");
                }
            }
        }

        private void OpenBuildFolder()
        {
            var projectRoot = Directory.GetParent(Application.dataPath).FullName;
            var buildDir = Path.Combine(projectRoot, "Builds");

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

        private void LoadSettings()
        {
            _selectedTarget = (BuildTarget)EditorPrefs.GetInt(PREF_TARGET, (int)BuildTarget.StandaloneWindows64);
            _selectedProfile = (BuildProfile)EditorPrefs.GetInt(PREF_PROFILE, (int)BuildProfile.Development);
            _buildVersion = EditorPrefs.GetString(PREF_VERSION, "1.0.0");
            _customBuildPath = EditorPrefs.GetString(PREF_CUSTOM_PATH, "");
            _buildAddressables = EditorPrefs.GetBool(PREF_BUILD_ADDRESSABLES, true);
            _autoOpenFolder = EditorPrefs.GetBool(PREF_AUTO_OPEN, true);
            _createZip = EditorPrefs.GetBool(PREF_CREATE_ZIP, false);
        }

        private void SaveSettings()
        {
            EditorPrefs.SetInt(PREF_TARGET, (int)_selectedTarget);
            EditorPrefs.SetInt(PREF_PROFILE, (int)_selectedProfile);
            EditorPrefs.SetString(PREF_VERSION, _buildVersion);
            EditorPrefs.SetString(PREF_CUSTOM_PATH, _customBuildPath);
            EditorPrefs.SetBool(PREF_BUILD_ADDRESSABLES, _buildAddressables);
            EditorPrefs.SetBool(PREF_AUTO_OPEN, _autoOpenFolder);
            EditorPrefs.SetBool(PREF_CREATE_ZIP, _createZip);
        }

        private void DrawBuildHistory()
        {
            var history = BuildHistoryManager.LoadHistory();
            if (history.entries.Count == 0)
            {
                EditorGUILayout.HelpBox("No build history available.", MessageType.Info);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Showing {history.entries.Count} recent builds", EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Clear History", EditorStyles.miniButton))
            {
                if (EditorUtility.DisplayDialog("Clear Build History", "Are you sure you want to clear all build history?", "Yes", "No"))
                {
                    BuildHistoryManager.ClearHistory();
                    return;
                }
            }
            EditorGUILayout.EndHorizontal();

            _historyScrollPosition = EditorGUILayout.BeginScrollView(_historyScrollPosition, GUILayout.Height(200));
            
            foreach (var entry in history.entries)
            {
                DrawBuildHistoryEntry(entry);
            }
            
            EditorGUILayout.EndScrollView();
        }

        private void DrawBuildHistoryEntry(BuildHistoryEntry entry)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Header line
            EditorGUILayout.BeginHorizontal();
            var statusColor = entry.buildResult == BuildResult.Succeeded ? Color.green : Color.red;
            var statusIcon = entry.buildResult == BuildResult.Succeeded ? "✓" : "✗";
            
            GUI.color = statusColor;
            GUILayout.Label(statusIcon, GUILayout.Width(20));
            GUI.color = Color.white;
            
            EditorGUILayout.LabelField($"{entry.buildVersion} - {entry.buildTarget} ({entry.buildProfile})", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(entry.buildTime.ToString("MM/dd HH:mm"), EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
            
            // Details
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Duration: {entry.buildDuration:mm\\:ss}", EditorStyles.miniLabel);
            if (entry.buildSize > 0)
            {
                EditorGUILayout.LabelField($"Size: {FormatBytes((ulong)entry.buildSize)}", EditorStyles.miniLabel);
            }
            if (entry.addressablesBuilt)
            {
                EditorGUILayout.LabelField("Addressables", EditorStyles.miniLabel);
            }
            if (entry.wasZipped)
            {
                EditorGUILayout.LabelField("Zipped", EditorStyles.miniLabel);
            }
            EditorGUILayout.EndHorizontal();
            
            // Path and actions
            if (!string.IsNullOrEmpty(entry.buildPath))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Path:", GUILayout.Width(40));
                EditorGUILayout.SelectableLabel(entry.buildPath, EditorStyles.miniTextField, GUILayout.Height(16));
                if (GUILayout.Button("Open", EditorStyles.miniButton, GUILayout.Width(50)))
                {
                    if (Directory.Exists(entry.buildPath))
                    {
                        System.Diagnostics.Process.Start("explorer.exe", entry.buildPath.Replace("/", "\\"));
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Folder Not Found", "Build folder no longer exists.", "OK");
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();
        }

        private void RecordBuildHistory(BuildReport buildReport, DateTime startTime, TimeSpan duration)
        {
            var entry = new BuildHistoryEntry(
                _buildVersion,
                _selectedTarget.ToString(),
                _selectedProfile.ToString(),
                Path.GetDirectoryName(buildReport.summary.outputPath),
                startTime,
                duration,
                (long)buildReport.summary.totalSize,
                buildReport.summary.result,
                "", // Build log could be added here
                _createZip,
                _buildAddressables
            );

            BuildHistoryManager.AddBuildEntry(entry);
        }

        private void RecordFailedBuild(DateTime startTime, TimeSpan duration, string errorMessage)
        {
            var entry = new BuildHistoryEntry(
                _buildVersion,
                _selectedTarget.ToString(),
                _selectedProfile.ToString(),
                "", // No path for failed build
                startTime,
                duration,
                0,
                BuildResult.Failed,
                errorMessage,
                false,
                _buildAddressables
            );

            BuildHistoryManager.AddBuildEntry(entry);
        }

        private string IncrementVersion(string version)
        {
            try
            {
                var parts = version.Split('.');
                if (parts.Length >= 3)
                {
                    // Increment patch version (third number)
                    if (int.TryParse(parts[2], out int patchVersion))
                    {
                        parts[2] = (patchVersion + 1).ToString();
                        return string.Join(".", parts);
                    }
                }
                else if (parts.Length == 2)
                {
                    // Add patch version
                    return version + ".1";
                }
                else if (parts.Length == 1)
                {
                    // Add minor and patch version
                    return version + ".0.1";
                }
            }
            catch
            {
                // If parsing fails, just append .1
            }
            
            return version + ".1";
        }
    }
}