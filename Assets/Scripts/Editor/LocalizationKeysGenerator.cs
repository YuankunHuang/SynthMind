using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;

namespace YuankunHuang.Unity.Editor
{
    public static class LocalizationKeysGenerator
    {
        private const string KEYS_FILE_PATH = "Assets/Scripts/Generated/LocalizationKeys.cs";

        [MenuItem("Tools/Localization/Generate Keys")]
        public static void GenerateKeys()
        {
            var stringTables = LocalizationEditorSettings.GetStringTableCollection("Localization");
            if (stringTables == null)
            {
                Debug.LogError("String table collection 'Localization' not found!");
                return;
            }

            var keys = ExtractKeys(stringTables);
            var code = GenerateCode(keys);
            WriteToFile(code);

            Debug.Log($"Generated LocalizationKeys.cs with {keys.Count} keys");
        }

        private static HashSet<string> ExtractKeys(StringTableCollection stringTables)
        {
            var keys = new HashSet<string>();
            
            foreach (var table in stringTables.StringTables)
            {
                foreach (var entry in table.SharedData.Entries)
                {
                    keys.Add(entry.Key);
                }
                break; // Only need one table to get all shared keys
            }
            
            return keys;
        }

        private static string GenerateCode(HashSet<string> keys)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("// Auto-generated LocalizationKeys");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Reflection;");
            sb.AppendLine();
            sb.AppendLine("namespace YuankunHuang.Unity.Core");
            sb.AppendLine("{");
            sb.AppendLine("    public static class LocalizationKeys");
            sb.AppendLine("    {");
            
            foreach (var key in keys.OrderBy(k => k))
            {
                var constantName = ToConstantName(key);
                sb.AppendLine($"        public const string {constantName} = \"{key}\";");
            }
            
            sb.AppendLine();
            sb.AppendLine("        private static List<string> _allKeys;");
            sb.AppendLine();
            sb.AppendLine("        public static List<string> GetAllKeys()");
            sb.AppendLine("        {");
            sb.AppendLine("            if (_allKeys == null)");
            sb.AppendLine("            {");
            sb.AppendLine("                _allKeys = new List<string>();");
            sb.AppendLine("                var fields = typeof(LocalizationKeys).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);");
            sb.AppendLine("                ");
            sb.AppendLine("                foreach (var field in fields)");
            sb.AppendLine("                {");
            sb.AppendLine("                    if (field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))");
            sb.AppendLine("                    {");
            sb.AppendLine("                        _allKeys.Add((string)field.GetValue(null));");
            sb.AppendLine("                    }");
            sb.AppendLine("                }");
            sb.AppendLine("            }");
            sb.AppendLine("            ");
            sb.AppendLine("            return _allKeys;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            
            return sb.ToString();
        }

        private static string ToConstantName(string key)
        {
            var sb = new StringBuilder();
            bool nextUpper = true;

            foreach (char c in key)
            {
                if (c == '.' || c == '_' || c == '-')
                {
                    nextUpper = true;
                }
                else if (char.IsLetterOrDigit(c))
                {
                    sb.Append(nextUpper ? char.ToUpper(c) : c);
                    nextUpper = false;
                }
            }

            return sb.ToString();
        }

        private static void WriteToFile(string content)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(KEYS_FILE_PATH));
            File.WriteAllText(KEYS_FILE_PATH, content, Encoding.UTF8);
            AssetDatabase.Refresh();
        }
    }
}