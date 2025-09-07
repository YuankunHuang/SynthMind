using UnityEngine;
using UnityEditor;
using YuankunHuang.Unity.UICore;
using YuankunHuang.Unity.Core;

namespace YuankunHuang.Unity.Editor
{
    [CustomEditor(typeof(LocalizedText))]
    public class LocalizedTextEditor : UnityEditor.Editor
    {
        private SerializedProperty _localizationKey;
        private SerializedProperty _tableName;
        private SerializedProperty _updateOnLanguageChange;

        private void OnEnable()
        {
            _localizationKey = serializedObject.FindProperty("_localizationKey");
            _tableName = serializedObject.FindProperty("_tableName");
            _updateOnLanguageChange = serializedObject.FindProperty("_updateOnLanguageChange");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_tableName);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_localizationKey);
            
            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                ShowKeySelectionMenu();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.PropertyField(_updateOnLanguageChange);

            if (serializedObject.hasModifiedProperties)
            {
                serializedObject.ApplyModifiedProperties();
                ((LocalizedText)target).UpdateText();
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Update Text Now"))
            {
                ((LocalizedText)target).UpdateText();
            }
        }

        private void ShowKeySelectionMenu()
        {
            var menu = new GenericMenu();
            
            var keys = LocalizationKeys.GetAllKeys();
            foreach (var key in keys)
            {
                menu.AddItem(new GUIContent(key), 
                    _localizationKey.stringValue == key, 
                    () => {
                        _localizationKey.stringValue = key;
                        serializedObject.ApplyModifiedProperties();
                        ((LocalizedText)target).UpdateText();
                    });
            }
            
            menu.ShowAsContext();
        }
    }
}