using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace YuankunHuang.Unity.AssetCore.Editor
{
    [CustomEditor(typeof(AssetManagerConfig))]
    public class AssetManagerConfigEditor : UnityEditor.Editor
    {
        private Dictionary<AssetType, bool> _foldoutStates = new Dictionary<AssetType, bool>();

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Script", MonoScript.FromScriptableObject((AssetManagerConfig)target), typeof(MonoScript), false);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();

            DrawAssetEntriesGrouped();

            EditorGUILayout.Space();

            if (GUILayout.Button("Batch Add From Folder", GUILayout.Height(30)))
            {
                BatchAddFromFolder();
            }

            if (GUILayout.Button("Clear Invalid", GUILayout.Height(30)))
            {
                var config = (AssetManagerConfig)target;
                config.ClearInvalidEntries();
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            if (GUILayout.Button("Clear All Assets", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Clear All Assets", "Are you sure you want to clear all assets? This action cannot be undone.", "Yes", "Cancel"))
                {
                    var config = (AssetManagerConfig)target;
                    config.AssetEntries.Clear();
                    EditorUtility.SetDirty(config);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }

            EditorGUILayout.Space(10);

            // allow us to generate asset keys from the config (immediately after editing asset entries)
            // convenient for us to update asset keys without leaving the editor :)
            GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button("Generate Asset Keys", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Generate Asset Keys", "Ready to generate asset keys? This will overwrite the existing keys.", "Yes", "Cancel"))
                {
                    AssetKeysGenerator.GenerateAssetKeys();
                }
            }
            GUI.backgroundColor = Color.white;
        }

        private void DrawAssetEntriesGrouped()
        {
            var config = target as AssetManagerConfig;
            var entriesProp = serializedObject.FindProperty("_assetEntries");

            // group with AssetType
            var groups = new Dictionary<AssetType, List<int>>();

            for (int i = 0; i < entriesProp.arraySize; i++)
            {
                var entryProp = entriesProp.GetArrayElementAtIndex(i);
                var typeProp = entryProp.FindPropertyRelative("type");
                var assetType = (AssetType)typeProp.enumValueIndex;

                if (!groups.ContainsKey(assetType))
                {
                    groups[assetType] = new List<int>();
                    // foldout states
                    if (!_foldoutStates.ContainsKey(assetType))
                    {
                        _foldoutStates[assetType] = false;
                    }
                }
                groups[assetType].Add(i);
            }

            // Display asset total count
            EditorGUILayout.LabelField($"Total number of assets: {entriesProp.arraySize}", EditorStyles.boldLabel);

            // Draw each group
            foreach (var group in groups.OrderBy(g => g.Key.ToString()))
            {
                DrawAssetTypeGroup(entriesProp, group.Key, group.Value);
            }
        }

        private void DrawAssetTypeGroup(SerializedProperty entriesProp, AssetType assetType, List<int> indices)
        {
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            // Draw the foldout for the asset type
            _foldoutStates[assetType] = EditorGUILayout.Foldout(_foldoutStates[assetType], $"{assetType} ({indices.Count})", true, EditorStyles.foldoutHeader);

            // Add button to remove the entire asset type group
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                if (EditorUtility.DisplayDialog($"Remove all {assetType}", $"Are you sure you want to remove all assets of type {assetType}? ({indices.Count})", "Yes", "Cancel"))
                {
                    DeleteAssetsByType(entriesProp, assetType, indices);
                    return; // immediate return after deletion, so we don't draw the entries
                }
            }

            EditorGUILayout.EndHorizontal();

            if (_foldoutStates[assetType])
            {
                EditorGUI.indentLevel++;

                for (int i = indices.Count - 1; i >= 0; i--)
                {
                    int index = indices[i];
                    var entryProp = entriesProp.GetArrayElementAtIndex(index);

                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.PropertyField(entryProp, new GUIContent($"[{index}]"), true);

                    GUI.backgroundColor = Color.red;
                    if (GUILayout.Button("¡Á", GUILayout.Width(20)))
                    {
                        entriesProp.DeleteArrayElementAtIndex(index);
                        serializedObject.ApplyModifiedProperties();
                        break;
                    }
                    GUI.backgroundColor = Color.white;

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUI.indentLevel--;
            }
        }

        private void DeleteAssetsByType(SerializedProperty entriesProp, AssetType assetType, List<int> indices)
        {
            var config = (AssetManagerConfig)target;
            // Remove entries from the config
            foreach (var idx in indices.OrderByDescending(i => i))
            {
                entriesProp.DeleteArrayElementAtIndex(idx);
            }
            // Save changes
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void BatchAddFromFolder()
        {
            var folderPath = EditorUtility.OpenFolderPanel("Select Folder", "Assets", "");

            if (!string.IsNullOrEmpty(folderPath))
            {
                var config = (AssetManagerConfig)target;
                var relativePath = folderPath.Replace(Application.dataPath, "Assets");
                var guids = AssetDatabase.FindAssets("", new[] { relativePath });

                foreach (var guid in guids)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (AssetDatabase.IsValidFolder(assetPath))
                    {
                        // Skip folders
                        continue;
                    }

                    var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
                    if (asset != null)
                    {
                        config.AddAsset(asset);
                    }
                    else
                    {
                        Debug.LogWarning($"Asset at path {assetPath} could not be loaded.");
                    }
                }
            }
        }
    }
}