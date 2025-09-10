using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace YuankunHuang.Unity.Editor.BuildPipeline
{
    public class BuildReportWindow : EditorWindow
    {
        private BuildReport _currentReport;
        private Vector2 _scrollPosition;
        private int _selectedTab = 0;
        private readonly string[] _tabNames = { "Summary", "Steps", "Stripping", "Messages" };

        public static void ShowReport(BuildReport report)
        {
            var window = GetWindow<BuildReportWindow>("Build Report");
            window._currentReport = report;
            window.Show();
        }

        private void OnGUI()
        {
            if (_currentReport == null)
            {
                EditorGUILayout.HelpBox("No build report available.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField("Build Report", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabNames);
            
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            switch (_selectedTab)
            {
                case 0:
                    DrawSummaryTab();
                    break;
                case 1:
                    DrawStepsTab();
                    break;
                case 2:
                    DrawStrippingTab();
                    break;
                case 3:
                    DrawMessagesTab();
                    break;
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Export Report"))
            {
                ExportReport();
            }
            if (GUILayout.Button("Close"))
            {
                Close();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSummaryTab()
        {
            var summary = _currentReport.summary;

            EditorGUILayout.LabelField("Build Summary", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // Basic info
            DrawInfoRow("Result", summary.result.ToString());
            DrawInfoRow("Platform", summary.platform.ToString());
            DrawInfoRow("Output Path", summary.outputPath);
            DrawInfoRow("Build Size", FormatBytes(summary.totalSize));
            DrawInfoRow("Build Time", summary.totalTime.ToString(@"mm\:ss"));
            DrawInfoRow("Total Errors", summary.totalErrors.ToString());
            DrawInfoRow("Total Warnings", summary.totalWarnings.ToString());

            EditorGUILayout.Space();

            // Build options
            EditorGUILayout.LabelField("Build Options", EditorStyles.boldLabel);
            DrawInfoRow("Development Build", summary.options.HasFlag(BuildOptions.Development) ? "Yes" : "No");
            DrawInfoRow("Allow Debugging", summary.options.HasFlag(BuildOptions.AllowDebugging) ? "Yes" : "No");
            DrawInfoRow("Auto Run Player", summary.options.HasFlag(BuildOptions.AutoRunPlayer) ? "Yes" : "No");

            EditorGUILayout.EndVertical();
        }

        private void DrawStepsTab()
        {
            EditorGUILayout.LabelField("Build Steps", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            if (_currentReport.steps == null || _currentReport.steps.Length == 0)
            {
                EditorGUILayout.HelpBox("No step information available.", MessageType.Info);
                return;
            }

            foreach (var step in _currentReport.steps)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(step.name, EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(step.duration.ToString(@"mm\:ss\.fff"), EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();

                if (step.messages != null && step.messages.Length > 0)
                {
                    EditorGUI.indentLevel++;
                    foreach (var message in step.messages.Take(5)) // Show only first 5 messages
                    {
                        var messageType = GetMessageType(message.type);
                        EditorGUILayout.HelpBox($"{message.type}: {message.content}", messageType);
                    }
                    if (step.messages.Length > 5)
                    {
                        EditorGUILayout.LabelField($"... and {step.messages.Length - 5} more messages", EditorStyles.miniLabel);
                    }
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawStrippingTab()
        {
            EditorGUILayout.LabelField("Code Stripping", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            if (_currentReport.strippingInfo == null)
            {
                EditorGUILayout.HelpBox("No stripping information available.", MessageType.Info);
                return;
            }

            var strippingInfo = _currentReport.strippingInfo;
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            var moduleCount = strippingInfo.includedModules?.ToArray().Length ?? 0;
            DrawInfoRow("Included Modules", moduleCount.ToString() ?? "0");
            
            if (strippingInfo.includedModules != null)
            {
                EditorGUI.indentLevel++;
                foreach (var module in strippingInfo.includedModules.Take(10))
                {
                    EditorGUILayout.LabelField($"• {module}", EditorStyles.miniLabel);
                }
                if (moduleCount > 10)
                {
                    EditorGUILayout.LabelField($"... and {moduleCount - 10} more modules", EditorStyles.miniLabel);
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawMessagesTab()
        {
            EditorGUILayout.LabelField("Build Messages", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            var allMessages = _currentReport.steps
                ?.SelectMany(step => step.messages ?? new BuildStepMessage[0])
                .ToArray();

            if (allMessages == null || allMessages.Length == 0)
            {
                EditorGUILayout.HelpBox("No messages available.", MessageType.Info);
                return;
            }

            var errors = allMessages.Where(m => m.type == LogType.Error).ToArray();
            var warnings = allMessages.Where(m => m.type == LogType.Warning).ToArray();
            var logs = allMessages.Where(m => m.type == LogType.Log).ToArray();

            if (errors.Length > 0)
            {
                EditorGUILayout.LabelField($"Errors ({errors.Length})", EditorStyles.boldLabel);
                foreach (var error in errors)
                {
                    EditorGUILayout.HelpBox(error.content, MessageType.Error);
                }
                EditorGUILayout.Space();
            }

            if (warnings.Length > 0)
            {
                EditorGUILayout.LabelField($"Warnings ({warnings.Length})", EditorStyles.boldLabel);
                foreach (var warning in warnings.Take(10)) // Limit warnings display
                {
                    EditorGUILayout.HelpBox(warning.content, MessageType.Warning);
                }
                if (warnings.Length > 10)
                {
                    EditorGUILayout.LabelField($"... and {warnings.Length - 10} more warnings", EditorStyles.miniLabel);
                }
                EditorGUILayout.Space();
            }

            if (logs.Length > 0)
            {
                EditorGUILayout.LabelField($"Info ({logs.Length})", EditorStyles.boldLabel);
                foreach (var log in logs.Take(5)) // Limit log display
                {
                    EditorGUILayout.HelpBox(log.content, MessageType.Info);
                }
                if (logs.Length > 5)
                {
                    EditorGUILayout.LabelField($"... and {logs.Length - 5} more log messages", EditorStyles.miniLabel);
                }
            }
        }

        private void DrawInfoRow(string label, string value)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label + ":", GUILayout.Width(120));
            EditorGUILayout.LabelField(value, EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndHorizontal();
        }

        private MessageType GetMessageType(LogType logType)
        {
            switch (logType)
            {
                case LogType.Error:
                case LogType.Exception:
                case LogType.Assert:
                    return MessageType.Error;
                case LogType.Warning:
                    return MessageType.Warning;
                default:
                    return MessageType.Info;
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

        private void ExportReport()
        {
            var path = EditorUtility.SaveFilePanel("Export Build Report", "", "BuildReport", "txt");
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                using (var writer = new StreamWriter(path))
                {
                    writer.WriteLine("SynthMind Build Report");
                    writer.WriteLine("======================");
                    writer.WriteLine();

                    var summary = _currentReport.summary;
                    writer.WriteLine("SUMMARY");
                    writer.WriteLine($"Result: {summary.result}");
                    writer.WriteLine($"Platform: {summary.platform}");
                    writer.WriteLine($"Output Path: {summary.outputPath}");
                    writer.WriteLine($"Build Size: {FormatBytes(summary.totalSize)}");
                    writer.WriteLine($"Build Time: {summary.totalTime}");
                    writer.WriteLine($"Total Errors: {summary.totalErrors}");
                    writer.WriteLine($"Total Warnings: {summary.totalWarnings}");
                    writer.WriteLine();

                    if (_currentReport.steps != null)
                    {
                        writer.WriteLine("STEPS");
                        foreach (var step in _currentReport.steps)
                        {
                            writer.WriteLine($"{step.name}: {step.duration}");
                            if (step.messages != null)
                            {
                                foreach (var message in step.messages)
                                {
                                    writer.WriteLine($"  [{message.type}] {message.content}");
                                }
                            }
                        }
                    }
                }

                EditorUtility.DisplayDialog("Export Complete", $"Build report exported to:\n{path}", "OK");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Export Failed", $"Failed to export report:\n{ex.Message}", "OK");
            }
        }
    }
}