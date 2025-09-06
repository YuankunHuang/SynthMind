using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEditor.Localization;

namespace YuankunHuang.Unity.Editor
{
    public class LocalizationExporter : EditorWindow
    {
        private string _exportPath = "Assets/Localization/Export";
        private string _fileName = "Localization";
        private ExportFormat _exportFormat = ExportFormat.CSV;
        
        private enum ExportFormat
        {
            CSV,
            XLSX
        }

        [MenuItem("Tools/Localization/Export Localization Data")]
        public static void ShowWindow()
        {
            GetWindow<LocalizationExporter>("Localization Exporter");
        }

        private void OnGUI()
        {
            GUILayout.Label("Export Localization Data", EditorStyles.boldLabel);
            
            EditorGUILayout.Space();
            
            _exportPath = EditorGUILayout.TextField("Export Path:", _exportPath);
            _fileName = EditorGUILayout.TextField("File Name:", _fileName);
            _exportFormat = (ExportFormat)EditorGUILayout.EnumPopup("Export Format:", _exportFormat);
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Browse Folder"))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("Select Export Folder", _exportPath, "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    _exportPath = FileUtil.GetProjectRelativePath(selectedPath);
                    if (string.IsNullOrEmpty(_exportPath))
                    {
                        _exportPath = selectedPath;
                    }
                }
            }
            
            EditorGUILayout.Space();
            
            GUI.enabled = !string.IsNullOrEmpty(_exportPath) && !string.IsNullOrEmpty(_fileName);
            
            if (GUILayout.Button("Export"))
            {
                ExportLocalizationData();
            }
            
            GUI.enabled = true;
        }

        private void ExportLocalizationData()
        {
            try
            {
                var stringTables = LocalizationEditorSettings.GetStringTableCollection("Localization");
                if (stringTables == null)
                {
                    EditorUtility.DisplayDialog("Error", "Could not find 'Localization' string table collection.", "OK");
                    return;
                }

                var data = new Dictionary<string, Dictionary<string, string>>();
                var locales = new List<string>();

                foreach (var table in stringTables.StringTables)
                {
                    var localeCode = table.LocaleIdentifier.Code;
                    locales.Add(localeCode);
                    
                    foreach (var entry in table.SharedData.Entries)
                    {
                        var key = entry.Key;
                        
                        if (!data.ContainsKey(key))
                        {
                            data[key] = new Dictionary<string, string>();
                        }
                        
                        var tableEntry = table.GetEntry(entry.Id);
                        var value = tableEntry?.GetLocalizedString() ?? "";
                        
                        data[key][localeCode] = value;
                    }
                }

                Directory.CreateDirectory(_exportPath);
                
                if (_exportFormat == ExportFormat.CSV)
                {
                    ExportToCSV(data, locales);
                }
                else
                {
                    ExportToXLSX(data, locales);
                }
                
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("Success", $"Localization data exported to {_exportPath}/{_fileName}.{_exportFormat.ToString().ToLower()}", "OK");
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Error", $"Export failed: {e.Message}", "OK");
                Debug.LogException(e);
            }
        }

        private void ExportToCSV(Dictionary<string, Dictionary<string, string>> data, List<string> locales)
        {
            var filePath = Path.Combine(_exportPath, $"{_fileName}.csv");
            var csv = new StringBuilder();
            
            csv.Append("Key");
            foreach (var locale in locales)
            {
                csv.Append($",{locale}");
            }
            csv.AppendLine();
            
            foreach (var kvp in data)
            {
                csv.Append($"\"{kvp.Key}\"");
                
                foreach (var locale in locales)
                {
                    var value = kvp.Value.ContainsKey(locale) ? kvp.Value[locale] : "";
                    csv.Append($",\"{value.Replace("\"", "\"\"")}\"");
                }
                csv.AppendLine();
            }
            
            File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);
        }

        private void ExportToXLSX(Dictionary<string, Dictionary<string, string>> data, List<string> locales)
        {
            EditorUtility.DisplayDialog("Info", "XLSX export requires additional libraries. For now, please use CSV format.", "OK");
        }
    }
}