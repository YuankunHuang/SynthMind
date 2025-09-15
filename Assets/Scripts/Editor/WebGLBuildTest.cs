using UnityEngine;
using UnityEditor;
using System.IO;

namespace YuankunHuang.Unity.Editor
{
    public class WebGLBuildTest
    {
        [MenuItem("Build/Test WebGL Build")]
        public static void BuildWebGL()
        {
            // Ensure WebGL platform is selected
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);

            // Configure WebGL settings
            //PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.memorySize = 512;
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.None;
            PlayerSettings.WebGL.dataCaching = true;

            // Set template
            PlayerSettings.WebGL.template = "PROJECT:SynthMind";

            // Build settings
            string[] scenes = {
                "Assets/Scenes/Bootstrapper.unity",
                "Assets/Scenes/UIScene.unity",
                "Assets/Scenes/Sandbox.unity"
            };

            string buildPath = Path.Combine(Directory.GetCurrentDirectory(), "WebGLBuild");

            // Clean previous build
            if (Directory.Exists(buildPath))
            {
                Directory.Delete(buildPath, true);
            }

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = buildPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            Debug.Log("[WebGL Build Test] Starting WebGL build...");
            Debug.Log($"[WebGL Build Test] Build path: {buildPath}");
            Debug.Log($"[WebGL Build Test] Template: {PlayerSettings.WebGL.template}");

            var report = UnityEditor.BuildPipeline.BuildPlayer(buildPlayerOptions);

            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"[WebGL Build Test] Build succeeded! Build size: {report.summary.totalSize} bytes");
                Debug.Log($"[WebGL Build Test] Build time: {report.summary.totalTime}");

                // Open the build folder
                EditorUtility.RevealInFinder(buildPath);

                // Show success dialog
                EditorUtility.DisplayDialog("Build Successful",
                    $"WebGL build completed successfully!\n\nBuild location: {buildPath}\n\nTo test:\n1. Configure Firebase settings in index.html\n2. Serve the build folder with a web server\n3. Open in browser",
                    "OK");
            }
            else
            {
                Debug.LogError($"[WebGL Build Test] Build failed: {report.summary.result}");

                // Show build errors
                foreach (var step in report.steps)
                {
                    if (step.messages.Length > 0)
                    {
                        foreach (var message in step.messages)
                        {
                            if (message.type == LogType.Error || message.type == LogType.Exception)
                            {
                                Debug.LogError($"[WebGL Build Test] {message.content}");
                            }
                            else if (message.type == LogType.Warning)
                            {
                                Debug.LogWarning($"[WebGL Build Test] {message.content}");
                            }
                        }
                    }
                }

                EditorUtility.DisplayDialog("Build Failed",
                    "WebGL build failed. Check the console for details.",
                    "OK");
            }
        }

        [MenuItem("Build/Configure WebGL Settings")]
        public static void ConfigureWebGLSettings()
        {
            // Switch to WebGL platform if not already
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
            }

            // Configure optimal WebGL settings
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.memorySize = 512;
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.WebGL.template = "PROJECT:SynthMind";

            // Disable unnecessary features for WebGL
            PlayerSettings.stripEngineCode = true;

            Debug.Log("[WebGL Settings] WebGL build settings configured optimally");
            Debug.Log("[WebGL Settings] Template set to: PROJECT:SynthMind");
            Debug.Log("[WebGL Settings] Memory size: 512MB");
            Debug.Log("[WebGL Settings] Compression: Gzip");

            EditorUtility.DisplayDialog("Settings Configured",
                "WebGL build settings have been optimized:\n\n" +
                "• Template: SynthMind\n" +
                "• Memory: 512MB\n" +
                "• Compression: Gzip\n" +
                "• Exception Support: Explicit only\n" +
                "• Data Caching: Enabled",
                "OK");
        }
    }
}