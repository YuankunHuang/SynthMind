using UnityEditor;
using UnityEngine;

namespace YuankunHuang.Unity.Editor
{
    public class LocalizationToolsWindow : EditorWindow
    {
        [MenuItem("Tools/Localization/Localization Tools")]
        public static void ShowWindow()
        {
            GetWindow<LocalizationToolsWindow>("Localization Tools");
        }

        private void OnGUI()
        {
            GUILayout.Label("Localization Management Tools", EditorStyles.boldLabel);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.HelpBox("Simple localization pipeline tools\n- Based on Unity Localization Package\n- Supports CSV export/import\n- Provides simple GetLocalizedText interface", MessageType.Info);
            
            EditorGUILayout.Space();
            
            GUILayout.Label("Import/Export", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Export Localization to CSV", GUILayout.Height(30)))
            {
                LocalizationExporter.ShowWindow();
            }
            
            if (GUILayout.Button("Import Localization from CSV", GUILayout.Height(30)))
            {
                LocalizationImporter.ShowWindow();
            }
            
            if (GUILayout.Button("Generate Localization Keys", GUILayout.Height(30)))
            {
                LocalizationKeysGenerator.GenerateKeys();
            }
            
            EditorGUILayout.Space();
            
            GUILayout.Label("Usage", EditorStyles.boldLabel);
            
            EditorGUILayout.HelpBox("Usage in scripts:\n\n" +
                "// Get localized text\n" +
                "var locManager = ModuleRegistry.Get<ILocalizationManager>();\n" +
                "locManager.GetLocalizedText(LocalizationKeys.KeyName, (text) => {\n" +
                "    // Use the localized text here\n" +
                "    myLabel.text = text;\n" +
                "});\n\n" +
                "// Format localized text\n" +
                "locManager.GetLocalizedTextFormatted(LocalizationKeys.HelloUser, (formatted) => {\n" +
                "    myLabel.text = formatted;\n" +
                "}, userName);",
                MessageType.None);
        }
    }
}