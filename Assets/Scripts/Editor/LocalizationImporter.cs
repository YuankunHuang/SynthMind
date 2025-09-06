using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;

namespace YuankunHuang.Unity.Editor
{
    public class LocalizationImporter : EditorWindow
    {
        private string _importFilePath = "";
        private ImportFormat _importFormat = ImportFormat.CSV;
        
        private enum ImportFormat
        {
            CSV,
            XLSX
        }

        [MenuItem("Tools/Localization/Import Localization Data")]
        public static void ShowWindow()
        {
            GetWindow<LocalizationImporter>("Localization Importer");
        }

        private void OnGUI()
        {
            GUILayout.Label("Import Localization Data", EditorStyles.boldLabel);
            
            EditorGUILayout.Space();
            
            _importFilePath = EditorGUILayout.TextField("Import File Path:", _importFilePath);
            _importFormat = (ImportFormat)EditorGUILayout.EnumPopup("Import Format:", _importFormat);
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Browse File"))
            {
                string extension = _importFormat == ImportFormat.CSV ? "csv" : "xlsx";
                string selectedPath = EditorUtility.OpenFilePanel("Select Import File", "", extension);
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    _importFilePath = selectedPath;
                }
            }
            
            EditorGUILayout.Space();
            
            GUI.enabled = !string.IsNullOrEmpty(_importFilePath) && File.Exists(_importFilePath);
            
            if (GUILayout.Button("Import"))
            {
                ImportLocalizationData();
            }
            
            GUI.enabled = true;
        }

        private void ImportLocalizationData()
        {
            try
            {
                var stringTables = LocalizationEditorSettings.GetStringTableCollection("Localization");
                if (stringTables == null)
                {
                    EditorUtility.DisplayDialog("Error", "Could not find 'Localization' string table collection.", "OK");
                    return;
                }

                Dictionary<string, Dictionary<string, string>> importData;
                
                if (_importFormat == ImportFormat.CSV)
                {
                    importData = ImportFromCSV();
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "XLSX import not implemented yet. Please use CSV format.", "OK");
                    return;
                }

                UpdateStringTables(stringTables, importData);
                
                EditorUtility.SetDirty(stringTables.SharedData);
                foreach (var table in stringTables.StringTables)
                {
                    EditorUtility.SetDirty(table);
                    EditorUtility.SetDirty(table.SharedData);
                }
                
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                LocalizationKeysGenerator.GenerateKeys();
                
                EditorUtility.DisplayDialog("Success", "Localization data imported successfully!", "OK");
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Error", $"Import failed: {e.Message}", "OK");
                Debug.LogException(e);
            }
        }

        private Dictionary<string, Dictionary<string, string>> ImportFromCSV()
        {
            var data = new Dictionary<string, Dictionary<string, string>>();
            var lines = File.ReadAllLines(_importFilePath);
            
            if (lines.Length == 0)
            {
                throw new Exception("CSV file is empty");
            }
            
            var header = ParseCSVLine(lines[0]);
            if (header.Length < 2 || header[0] != "Key")
            {
                throw new Exception("CSV format invalid. First column must be 'Key'");
            }
            
            var locales = new string[header.Length - 1];
            for (int i = 1; i < header.Length; i++)
            {
                locales[i - 1] = header[i];
            }
            
            for (int i = 1; i < lines.Length; i++)
            {
                var values = ParseCSVLine(lines[i]);
                if (values.Length != header.Length) continue;
                
                var key = values[0];
                if (string.IsNullOrEmpty(key)) continue;
                
                data[key] = new Dictionary<string, string>();
                
                for (int j = 0; j < locales.Length; j++)
                {
                    var localeIndex = j + 1;
                    if (localeIndex < values.Length)
                    {
                        data[key][locales[j]] = values[localeIndex];
                    }
                }
            }
            
            return data;
        }

        private string[] ParseCSVLine(string line)
        {
            var result = new List<string>();
            var current = "";
            bool inQuotes = false;
            
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                
                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current += '"';
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current);
                    current = "";
                }
                else
                {
                    current += c;
                }
            }
            
            result.Add(current);
            return result.ToArray();
        }

        private void UpdateStringTables(StringTableCollection stringTables, Dictionary<string, Dictionary<string, string>> importData)
        {
            foreach (var kvp in importData)
            {
                var key = kvp.Key;
                var translations = kvp.Value;
                
                var sharedEntry = stringTables.SharedData.GetEntry(key);
                if (sharedEntry == null)
                {
                    sharedEntry = stringTables.SharedData.AddKey(key);
                }
                
                foreach (var table in stringTables.StringTables)
                {
                    var localeCode = table.LocaleIdentifier.Code;
                    if (translations.ContainsKey(localeCode))
                    {
                        var value = translations[localeCode];
                        var entry = table.GetEntry(sharedEntry.Id);
                        
                        if (entry == null)
                        {
                            entry = table.AddEntry(sharedEntry.Id, value);
                        }
                        else
                        {
                            entry.Value = value;
                        }
                    }
                }
            }
        }
    }
}