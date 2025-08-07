#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization.Tables;
using UnityEditor.Localization;
using System.Linq;

namespace YuankunHuang.Unity.Core.Editor
{
    public class LocalizationWorkflowManager : EditorWindow
    {
        private const string CSV_EXPORT_PATH = "Assets/Localization/Export/";
        private const string CSV_IMPORT_PATH = "Assets/Localization/Import/";
        private const string GENERATED_CODE_PATH = "Assets/Scripts/Generated/LocalizationKeys.cs";

        private Vector2 scrollPosition;
        private string csvFileName = "localization_export";

        [MenuItem("SynthMind/Tools/Localization Workflow Manager")]
        public static void ShowWindow()
        {
            GetWindow<LocalizationWorkflowManager>("Localization Workflow");
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Label("Localization Workflow Manager", EditorStyles.boldLabel);
            GUILayout.Space(10);

            DrawDevelopmentPhase();
            GUILayout.Space(15);

            DrawTranslationPhase();
            GUILayout.Space(15);

            DrawIntegrationPhase();
            GUILayout.Space(15);

            DrawReleasePhase();

            EditorGUILayout.EndScrollView();
        }

        private void DrawDevelopmentPhase()
        {
            GUILayout.Label("1. Development Phase", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Create Sample CSV", GUILayout.Height(30)))
            {
                CreateSampleCSV();
            }
            if (GUILayout.Button("Import from CSV", GUILayout.Height(30)))
            {
                ImportFromCSV();
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Generate Typed Keys", GUILayout.Height(30)))
            {
                GenerateLocalizationKeys();
            }
        }

        private void DrawTranslationPhase()
        {
            GUILayout.Label("2. Translation Phase", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Filename:", GUILayout.Width(60));
            csvFileName = GUILayout.TextField(csvFileName);
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Export CSV for Translators", GUILayout.Height(30)))
            {
                ExportToCSV();
            }

            if (GUILayout.Button("Open Export Folder", GUILayout.Height(25)))
            {
                OpenExportFolder();
            }
        }

        private void DrawIntegrationPhase()
        {
            GUILayout.Label("3. Integration Phase", EditorStyles.boldLabel);

            if (GUILayout.Button("Import Translated CSV", GUILayout.Height(30)))
            {
                ImportTranslatedCSV();
            }

            if (GUILayout.Button("Validate Localization", GUILayout.Height(30)))
            {
                ValidateLocalization();
            }
        }

        private void DrawReleasePhase()
        {
            GUILayout.Label("4. Release Phase", EditorStyles.boldLabel);

            if (GUILayout.Button("Final Code Generation", GUILayout.Height(30)))
            {
                GenerateLocalizationKeys();
                Debug.Log("Typed keys updated, ready for release!");
            }

            if (GUILayout.Button("Clean Temp Files", GUILayout.Height(30)))
            {
                CleanupTempFiles();
            }
        }

        #region CSV Operations

        private void CreateSampleCSV()
        {
            var csv = new StringBuilder();
            csv.AppendLine("Key,English,Chinese");
            csv.AppendLine("ui.welcome_message,\"Welcome to our game!\",\"欢迎来到我们的游戏！\"");
            csv.AppendLine("ui.start_button,\"Start\",\"开始\"");
            csv.AppendLine("ui.settings_title,\"Settings\",\"设置\"");
            csv.AppendLine("ui.quit_confirm,\"Are you sure you want to quit?\",\"确定要退出吗？\"");
            csv.AppendLine("game.score_text,\"Score: {0}\",\"得分：{0}\"");
            csv.AppendLine("game.level_complete,\"Level Complete!\",\"关卡完成！\"");

            var path = Path.Combine(CSV_IMPORT_PATH, "sample_localization.csv");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, csv.ToString(), Encoding.UTF8);

            AssetDatabase.Refresh();
            Debug.Log($"Sample CSV created: {path}");
            EditorUtility.RevealInFinder(path);
        }

        private void ImportFromCSV()
        {
            var path = EditorUtility.OpenFilePanel("Select CSV File", CSV_IMPORT_PATH, "csv");
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                ImportCSVToLocalizationTables(path);
                Debug.Log("CSV import successful!");
            }
            catch (Exception e)
            {
                Debug.LogError($"CSV import failed: {e.Message}");
            }
        }

        private void ExportToCSV()
        {
            try
            {
                var stringTables = GetAllStringTables();
                if (stringTables.Count == 0)
                {
                    Debug.LogWarning("No string tables found!");
                    return;
                }

                var csv = GenerateCSVFromTables(stringTables);
                var fileName = $"{csvFileName}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                var path = Path.Combine(CSV_EXPORT_PATH, fileName);

                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, csv, Encoding.UTF8);

                AssetDatabase.Refresh();
                Debug.Log($"CSV exported: {path}");
                EditorUtility.RevealInFinder(path);
            }
            catch (Exception e)
            {
                Debug.LogError($"Export failed: {e.Message}");
            }
        }

        private void ImportTranslatedCSV()
        {
            var path = EditorUtility.OpenFilePanel("Select Translated CSV", CSV_EXPORT_PATH, "csv");
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                ImportCSVToLocalizationTables(path);
                Debug.Log("Translation import successful!");

                // Auto generate code
                GenerateLocalizationKeys();
            }
            catch (Exception e)
            {
                Debug.LogError($"Translation import failed: {e.Message}");
            }
        }

        #endregion

        #region Core Implementation

        private void ImportCSVToLocalizationTables(string csvPath)
        {
            var lines = File.ReadAllLines(csvPath, Encoding.UTF8);
            if (lines.Length < 2) return;

            // Parse headers
            var headers = ParseCSVLine(lines[0]);
            var keyIndex = Array.IndexOf(headers, "Key");
            if (keyIndex == -1) throw new Exception("'Key' column not found in CSV");

            // Get language columns
            var languageColumns = new Dictionary<string, int>();
            for (int i = 0; i < headers.Length; i++)
            {
                if (i != keyIndex)
                {
                    languageColumns[headers[i]] = i;
                }
            }

            // Get or create localization table
            var tableCollection = LocalizationEditorSettings.GetStringTableCollection("Localization");
            if (tableCollection == null)
            {
                Debug.LogError("String table collection 'Localization' not found!");
                return;
            }

            // Import data
            for (int lineIndex = 1; lineIndex < lines.Length; lineIndex++)
            {
                var values = ParseCSVLine(lines[lineIndex]);
                if (values.Length <= keyIndex) continue;

                var key = values[keyIndex];
                if (string.IsNullOrEmpty(key)) continue;

                foreach (var langColumn in languageColumns)
                {
                    if (values.Length > langColumn.Value)
                    {
                        var value = values[langColumn.Value];
                        if (!string.IsNullOrEmpty(value))
                        {
                            // Add or update localization entry
                            AddOrUpdateLocalizationEntry(tableCollection, key, langColumn.Key, value);
                        }
                    }
                }
            }

            EditorUtility.SetDirty(tableCollection);
            AssetDatabase.SaveAssets();
        }

        private void AddOrUpdateLocalizationEntry(StringTableCollection collection, string key, string locale, string value)
        {
            try
            {
                // Get corresponding language table
                var table = collection.GetTable(locale) as StringTable;
                if (table == null)
                {
                    Debug.LogWarning($"Language table not found: {locale}");
                    return;
                }

                // Add or update entry
                var entry = table.GetEntry(key);
                if (entry == null)
                {
                    table.AddEntry(key, value);
                }
                else
                {
                    entry.Value = value;
                }

                EditorUtility.SetDirty(table);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to add localization entry {key} -> {locale}: {e.Message}");
            }
        }

        private string GenerateCSVFromTables(List<StringTable> tables)
        {
            var csv = new StringBuilder();
            var allKeys = new HashSet<string>();

            // Collect all keys
            foreach (var table in tables)
            {
                foreach (var entry in table.Values)
                {
                    allKeys.Add(entry.Key);
                }
            }

            // Generate header row
            var header = new List<string> { "Key" };
            header.AddRange(tables.Select(t => t.LocaleIdentifier.Code));
            csv.AppendLine(string.Join(",", header.Select(EscapeCSVField)));

            // Generate data rows
            foreach (var key in allKeys.OrderBy(k => k))
            {
                var row = new List<string> { key };

                foreach (var table in tables)
                {
                    var entry = table.GetEntry(key);
                    var value = entry?.Value ?? "";
                    row.Add(value);
                }

                csv.AppendLine(string.Join(",", row.Select(EscapeCSVField)));
            }

            return csv.ToString();
        }

        private void GenerateLocalizationKeys()
        {
            try
            {
                var stringTables = GetAllStringTables();
                var keys = ExtractAllKeys(stringTables);
                var code = GenerateKeysCode(keys);

                WriteCodeToFile(code);
                AssetDatabase.Refresh();

                Debug.Log($"Generated LocalizationKeys.cs with {keys.Count} keys");
            }
            catch (Exception e)
            {
                Debug.LogError($"Code generation failed: {e.Message}");
            }
        }

        private List<StringTable> GetAllStringTables()
        {
            var tables = new List<StringTable>();
            var guids = AssetDatabase.FindAssets("t:StringTable");

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var table = AssetDatabase.LoadAssetAtPath<StringTable>(path);
                if (table != null)
                {
                    tables.Add(table);
                }
            }

            return tables;
        }

        private HashSet<string> ExtractAllKeys(List<StringTable> tables)
        {
            var keys = new HashSet<string>();

            foreach (var table in tables)
            {
                foreach (var entry in table.Values)
                {
                    keys.Add(entry.Key);
                }
            }

            return keys;
        }

        private string GenerateKeysCode(HashSet<string> keys)
        {
            var sb = new StringBuilder();

            // header
            sb.AppendLine("// This file is auto-generated. Do not modify manually.");
            sb.AppendLine($"// Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"// Total keys: {keys.Count}");
            sb.AppendLine();
            sb.AppendLine("namespace YuankunHuang.Unity.Core");
            sb.AppendLine("{");
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// Auto-generated localization keys class");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    public static class LocalizationKeys");
            sb.AppendLine("    {");

            // group by prefix (table name)
            var groups = GroupKeysByPrefix(keys);

            foreach (var group in groups.OrderBy(g => g.Key))
            {
                if (group.Value.Count > 1)
                {
                    var className = ToPascalCase(group.Key);
                    sb.AppendLine($"        /// <summary>");
                    sb.AppendLine($"        /// {group.Key} related localization keys");
                    sb.AppendLine($"        /// </summary>");
                    sb.AppendLine($"        public static class {className}");
                    sb.AppendLine("        {");

                    foreach (var key in group.Value.OrderBy(k => k))
                    {
                        var constantName = ToConstantName(key.Substring(group.Key.Length + 1));
                        sb.AppendLine($"            public const string {constantName} = \"{key}\";");
                    }

                    sb.AppendLine("        }");
                    sb.AppendLine();
                }
            }

            // single keys
            var singleKeys = keys.Where(k => !HasCommonPrefix(k, keys)).OrderBy(k => k).ToList();
            if (singleKeys.Count > 0)
            {
                sb.AppendLine("        /// <summary>");
                sb.AppendLine("        /// Individual localization keys");
                sb.AppendLine("        /// </summary>");
                foreach (var key in singleKeys)
                {
                    var constantName = ToConstantName(key);
                    sb.AppendLine($"        public const string {constantName} = \"{key}\";");
                }
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        #endregion

        #region Utility Methods

        private string[] ParseCSVLine(string line)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++; // Skip next quote
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            result.Add(current.ToString());
            return result.ToArray();
        }

        private string EscapeCSVField(string field)
        {
            if (string.IsNullOrEmpty(field)) return "\"\"";

            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            {
                return "\"" + field.Replace("\"", "\"\"") + "\"";
            }

            return field;
        }

        private Dictionary<string, List<string>> GroupKeysByPrefix(HashSet<string> keys)
        {
            var groups = new Dictionary<string, List<string>>();

            foreach (var key in keys)
            {
                var parts = key.Split('.');
                if (parts.Length > 1)
                {
                    var prefix = parts[0];
                    if (!groups.ContainsKey(prefix))
                    {
                        groups[prefix] = new List<string>();
                    }
                    groups[prefix].Add(key);
                }
            }

            return groups;
        }

        private bool HasCommonPrefix(string key, HashSet<string> allKeys)
        {
            var parts = key.Split('.');
            if (parts.Length <= 1) return false;

            var prefix = parts[0];
            int count = 0;
            foreach (var otherKey in allKeys)
            {
                if (otherKey.StartsWith(prefix + "."))
                {
                    count++;
                    if (count > 1) return true;
                }
            }

            return false;
        }

        private string ToPascalCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            var parts = input.Split('_', '-');
            var sb = new StringBuilder();

            foreach (var part in parts)
            {
                if (!string.IsNullOrEmpty(part))
                {
                    sb.Append(char.ToUpper(part[0]));
                    if (part.Length > 1)
                    {
                        sb.Append(part.Substring(1).ToLower());
                    }
                }
            }

            return sb.ToString();
        }

        private string ToConstantName(string input)
        {
            var sb = new StringBuilder();
            bool nextUpper = true;

            foreach (char c in input)
            {
                if (c == '.' || c == '_' || c == '-')
                {
                    nextUpper = true;
                }
                else if (nextUpper)
                {
                    sb.Append(char.ToUpper(c));
                    nextUpper = false;
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        private void WriteCodeToFile(string content)
        {
            var directory = Path.GetDirectoryName(GENERATED_CODE_PATH);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(GENERATED_CODE_PATH, content, Encoding.UTF8);
        }

        private void ValidateLocalization()
        {
            var tables = GetAllStringTables();
            var allKeys = ExtractAllKeys(tables);
            var issues = new List<string>();

            foreach (var key in allKeys)
            {
                var translations = new Dictionary<string, string>();
                foreach (var table in tables)
                {
                    var entry = table.GetEntry(key);
                    translations[table.LocaleIdentifier.Code] = entry?.Value ?? "";
                }

                var emptyTranslations = translations.Where(t => string.IsNullOrEmpty(t.Value)).ToList();
                if (emptyTranslations.Count > 0)
                {
                    issues.Add($"Key '{key}' missing translations: {string.Join(", ", emptyTranslations.Select(t => t.Key))}");
                }
            }

            if (issues.Count > 0)
            {
                Debug.LogWarning($"Found {issues.Count} localization issues:\n" + string.Join("\n", issues));
            }
            else
            {
                Debug.Log("Localization validation passed! All keys have complete translations.");
            }
        }

        private void OpenExportFolder()
        {
            Directory.CreateDirectory(CSV_EXPORT_PATH);
            EditorUtility.RevealInFinder(CSV_EXPORT_PATH);
        }

        private void CleanupTempFiles()
        {
            if (Directory.Exists(CSV_EXPORT_PATH))
            {
                var files = Directory.GetFiles(CSV_EXPORT_PATH, "*.csv");
                var filesToDelete = files.Where(f =>
                    Path.GetFileName(f).Contains("_") &&
                    DateTime.TryParseExact(
                        Path.GetFileNameWithoutExtension(f).Split('_').LastOrDefault(),
                        "yyyyMMdd_HHmmss",
                        null,
                        System.Globalization.DateTimeStyles.None,
                        out var fileDate) &&
                    (DateTime.Now - fileDate).TotalDays > 7
                ).ToList();

                foreach (var file in filesToDelete)
                {
                    File.Delete(file);
                }

                Debug.Log($"Deleted {filesToDelete.Count} expired temp output files.");
            }

            AssetDatabase.Refresh();
        }

        #endregion
    }
}
#endif