using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;
using YuankunHuang.Unity.Core;

namespace YuankunHuang.Unity.Editor
{
    public class WindowRemoverTool : EditorWindow
    {
        private string windowName = "";
        private Vector2 scrollPosition;
        
        private bool showPreview = false;
        private List<string> filesToDelete = new List<string>();
        private List<string> addressablesToRemove = new List<string>();
        private bool willUpdateWindowNames = false;

        private const string STACKABLE_PATH = "Assets/Addressables/Window/Stackable";
        private const string ATTRIBUTE_DATA_PATH = "Assets/Addressables/WindowAttributeData";
        private const string WINDOW_NAMES_PATH = "Assets/Scripts/HotUpdate/WindowNames.cs";
        private const string WINDOW_CONTROLLER_PATH = "Assets/Scripts/HotUpdate/Window";

        [MenuItem("Tools/UI/Window Remover")]
        public static void ShowWindow()
        {
            GetWindow<WindowRemoverTool>("Window Remover");
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Label("Remove Window", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("This tool will remove all files and references related to the specified window.", MessageType.Warning);
            GUILayout.Space(10);

            DrawWindowNameSection();
            GUILayout.Space(10);

            DrawPreviewSection();
            GUILayout.Space(10);

            DrawRemoveButton();

            EditorGUILayout.EndScrollView();
        }

        private void DrawWindowNameSection()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Window Name:", GUILayout.Width(100));
            
            EditorGUI.BeginChangeCheck();
            windowName = EditorGUILayout.TextField(windowName);
            if (EditorGUI.EndChangeCheck())
            {
                UpdatePreview();
            }
            
            EditorGUILayout.EndHorizontal();

            if (string.IsNullOrEmpty(windowName))
            {
                EditorGUILayout.HelpBox("Please enter a window name", MessageType.Info);
            }
            else if (!IsValidIdentifier(windowName))
            {
                EditorGUILayout.HelpBox("Window name must be a valid C# identifier", MessageType.Warning);
            }
        }

        private void DrawPreviewSection()
        {
            if (string.IsNullOrEmpty(windowName) || !IsValidIdentifier(windowName))
                return;

            showPreview = EditorGUILayout.Foldout(showPreview, "Preview - Files to be removed:", true, EditorStyles.foldoutHeader);
            
            if (showPreview)
            {
                EditorGUI.indentLevel++;

                // Files to delete
                if (filesToDelete.Count > 0)
                {
                    EditorGUILayout.LabelField("Files:", EditorStyles.boldLabel);
                    foreach (var file in filesToDelete)
                    {
                        var exists = File.Exists(file) || Directory.Exists(file);
                        GUI.color = exists ? Color.green : Color.gray;
                        EditorGUILayout.LabelField($"• {file} {(exists ? "(EXISTS)" : "(NOT FOUND)")}", EditorStyles.miniLabel);
                    }
                    GUI.color = Color.white;
                    EditorGUILayout.Space();
                }

                // Addressables to remove
                if (addressablesToRemove.Count > 0)
                {
                    EditorGUILayout.LabelField("Addressables:", EditorStyles.boldLabel);
                    foreach (var addressable in addressablesToRemove)
                    {
                        EditorGUILayout.LabelField($"• {addressable}", EditorStyles.miniLabel);
                    }
                    EditorGUILayout.Space();
                }

                // WindowNames update
                if (willUpdateWindowNames)
                {
                    EditorGUILayout.LabelField("WindowNames.cs:", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField($"• Remove field: public static readonly string {windowName}", EditorStyles.miniLabel);
                }

                EditorGUI.indentLevel--;
            }
        }

        private void DrawRemoveButton()
        {
            bool canRemove = !string.IsNullOrEmpty(windowName) && 
                            IsValidIdentifier(windowName) && 
                            (filesToDelete.Count > 0 || addressablesToRemove.Count > 0 || willUpdateWindowNames);

            if (!canRemove && !string.IsNullOrEmpty(windowName) && IsValidIdentifier(windowName))
            {
                EditorGUILayout.HelpBox("No files or resources found for this window name.", MessageType.Info);
            }

            GUI.enabled = canRemove;
            GUI.backgroundColor = canRemove ? Color.red : Color.gray;

            if (GUILayout.Button("Remove Window Resources", GUILayout.Height(35)))
            {
                if (EditorUtility.DisplayDialog("Confirm Removal", 
                    $"Are you sure you want to remove all resources for window '{windowName}'?\n\nThis action cannot be undone!", 
                    "Remove", "Cancel"))
                {
                    RemoveWindow();
                }
            }

            GUI.backgroundColor = Color.white;
            GUI.enabled = true;
        }

        private void UpdatePreview()
        {
            filesToDelete.Clear();
            addressablesToRemove.Clear();
            willUpdateWindowNames = false;

            if (string.IsNullOrEmpty(windowName) || !IsValidIdentifier(windowName))
                return;

            // Check for files to delete
            var prefabPath = Path.Combine(STACKABLE_PATH, windowName, $"{windowName}.prefab");
            var prefabFolder = Path.Combine(STACKABLE_PATH, windowName);
            var controllerFile = Path.Combine(WINDOW_CONTROLLER_PATH, windowName, $"{windowName}Controller.cs");
            var controllerFolder = Path.Combine(WINDOW_CONTROLLER_PATH, windowName);
            var attributeDataFile = Path.Combine(ATTRIBUTE_DATA_PATH, $"WindowAttributeData_{windowName}.asset");

            if (Directory.Exists(prefabFolder))
                filesToDelete.Add(prefabFolder);
            if (Directory.Exists(controllerFolder))
                filesToDelete.Add(controllerFolder);
            if (File.Exists(attributeDataFile))
                filesToDelete.Add(attributeDataFile);

            // Check for addressables
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings != null)
            {
                var prefabAddress = $"Assets/Addressables/Window/Stackable/{windowName}/{windowName}.prefab";
                var attributeAddress = $"Assets/Addressables/WindowAttributeData/WindowAttributeData_{windowName}.asset";

                var prefabEntry = settings.FindAssetEntry(AssetDatabase.AssetPathToGUID(prefabPath));
                if (prefabEntry != null)
                    addressablesToRemove.Add(prefabEntry.address);

                var attributeEntry = settings.FindAssetEntry(AssetDatabase.AssetPathToGUID(attributeDataFile));
                if (attributeEntry != null)
                    addressablesToRemove.Add(attributeEntry.address);
            }

            // Check if WindowNames needs update
            if (File.Exists(WINDOW_NAMES_PATH))
            {
                var content = File.ReadAllText(WINDOW_NAMES_PATH);
                willUpdateWindowNames = IsWindowNameExists(content, windowName);
            }

            showPreview = true;
        }

        private void RemoveWindow()
        {
            try
            {
                int removedCount = 0;
                var errors = new List<string>();

                EditorUtility.DisplayProgressBar("Removing Window", "Removing Addressable entries...", 0.1f);
                
                // Remove from Addressables first
                RemoveFromAddressables(ref removedCount, errors);

                EditorUtility.DisplayProgressBar("Removing Window", "Removing files...", 0.4f);
                
                // Remove files
                RemoveFiles(ref removedCount, errors);

                EditorUtility.DisplayProgressBar("Removing Window", "Updating WindowNames.cs...", 0.8f);
                
                // Update WindowNames.cs
                RemoveFromWindowNames(ref removedCount, errors);

                EditorUtility.DisplayProgressBar("Removing Window", "Updating WindowControllerFactory...", 0.85f);

                // Update WindowControllerFactory
                UpdateWindowControllerFactory(ref removedCount, errors);

                EditorUtility.DisplayProgressBar("Removing Window", "Refreshing assets...", 0.9f);
                
                AssetDatabase.Refresh();
                
                EditorUtility.ClearProgressBar();

                // Show result
                var message = $"Window '{windowName}' removal completed!\n\nRemoved {removedCount} items.";
                if (errors.Count > 0)
                {
                    message += $"\n\nErrors encountered:\n{string.Join("\n", errors)}";
                    EditorUtility.DisplayDialog("Removal Completed with Errors", message, "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("Removal Completed", message, "OK");
                }

                // Clear the window name and update preview
                windowName = "";
                UpdatePreview();
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Removal Failed", $"Failed to remove window: {e.Message}", "OK");
                LogHelper.LogError($"Window removal failed: {e}");
            }
        }

        private void RemoveFromAddressables(ref int removedCount, List<string> errors)
        {
            try
            {
                var settings = AddressableAssetSettingsDefaultObject.Settings;
                if (settings == null) return;

                foreach (var address in addressablesToRemove)
                {
                    var entry = settings.FindAssetEntry(address);
                    if (entry != null)
                    {
                        settings.RemoveAssetEntry(entry.guid);
                        removedCount++;
                        LogHelper.Log($"Removed addressable: {address}");
                    }
                }

                if (addressablesToRemove.Count > 0)
                {
                    EditorUtility.SetDirty(settings);
                }
            }
            catch (Exception e)
            {
                errors.Add($"Addressables removal error: {e.Message}");
            }
        }

        private void RemoveFiles(ref int removedCount, List<string> errors)
        {
            foreach (var filePath in filesToDelete)
            {
                try
                {
                    if (Directory.Exists(filePath))
                    {
                        Directory.Delete(filePath, true);
                        // Also remove .meta files
                        var metaFile = filePath + ".meta";
                        if (File.Exists(metaFile))
                        {
                            File.Delete(metaFile);
                        }
                        removedCount++;
                        LogHelper.Log($"Removed directory: {filePath}");
                    }
                    else if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                        // Also remove .meta file
                        var metaFile = filePath + ".meta";
                        if (File.Exists(metaFile))
                        {
                            File.Delete(metaFile);
                        }
                        removedCount++;
                        LogHelper.Log($"Removed file: {filePath}");
                    }
                }
                catch (Exception e)
                {
                    errors.Add($"File removal error ({filePath}): {e.Message}");
                }
            }
        }

        private void RemoveFromWindowNames(ref int removedCount, List<string> errors)
        {
            if (!willUpdateWindowNames) return;

            try
            {
                var content = File.ReadAllText(WINDOW_NAMES_PATH);
                var newContent = RemoveWindowNameFromFile(content, windowName);
                
                if (newContent != content)
                {
                    File.WriteAllText(WINDOW_NAMES_PATH, newContent);
                    removedCount++;
                    LogHelper.Log($"Removed '{windowName}' from WindowNames.cs");
                }
            }
            catch (Exception e)
            {
                errors.Add($"WindowNames.cs update error: {e.Message}");
                LogHelper.LogError($"Please manually remove 'public static readonly string {windowName} = \"{windowName}\";' from WindowNames.cs");
            }
        }

        private string RemoveWindowNameFromFile(string fileContent, string windowName)
        {
            var lines = fileContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var newLines = new List<string>();

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                
                // Skip the line that defines our window name
                if (trimmed.StartsWith("public static readonly string") && 
                    trimmed.Contains($"string {windowName} =") &&
                    trimmed.EndsWith(";"))
                {
                    LogHelper.Log($"Removing line: {line}");
                    continue; // Skip this line
                }
                
                newLines.Add(line);
            }

            return string.Join(System.Environment.NewLine, newLines);
        }

        private bool IsWindowNameExists(string fileContent, string windowName)
        {
            var lines = fileContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("public static readonly string") &&
                    trimmed.Contains($"string {windowName} ="))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsValidIdentifier(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            if (!char.IsLetter(name[0]) && name[0] != '_') return false;

            for (int i = 1; i < name.Length; i++)
            {
                if (!char.IsLetterOrDigit(name[i]) && name[i] != '_')
                    return false;
            }

            return true;
        }

        private void UpdateWindowControllerFactory(ref int removedCount, List<string> errors)
        {
            try
            {
                var factoryPath = "Assets/Scripts/Core/UI/WindowControllerFactory.cs";

                if (!File.Exists(factoryPath))
                {
                    LogHelper.LogWarning($"[WindowRemoverTool] WindowControllerFactory.cs not found at {factoryPath}");
                    return;
                }

                var content = File.ReadAllText(factoryPath);
                var newContent = RemoveEntryFromFactoryDictionary(content, windowName);

                if (newContent != content)
                {
                    File.WriteAllText(factoryPath, newContent);
                    removedCount++;
                    LogHelper.Log($"Successfully removed {windowName} from WindowControllerFactory");
                }
                else
                {
                    LogHelper.Log($"{windowName} was not found in WindowControllerFactory");
                }
            }
            catch (Exception e)
            {
                errors.Add($"WindowControllerFactory update error: {e.Message}");
                LogHelper.LogError($"Please manually remove '{windowName}' from WindowControllerFactory.cs");
            }
        }

        private string RemoveEntryFromFactoryDictionary(string content, string windowName)
        {
            var lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var newLines = new List<string>();

            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                // Skip the line that contains our window's factory entry
                if (trimmed.Contains($"\"{windowName}\"") &&
                    trimmed.Contains("() => new") &&
                    trimmed.Contains($"{windowName}Controller()"))
                {
                    LogHelper.Log($"Removing factory entry: {line.Trim()}");
                    continue; // Skip this line
                }

                newLines.Add(line);
            }

            return string.Join(System.Environment.NewLine, newLines);
        }
    }
}