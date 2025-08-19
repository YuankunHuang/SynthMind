using UnityEditor;
using UnityEngine;
using YuankunHuang.Unity.AssetCore;
using YuankunHuang.Unity.AssetCore.Editor;

namespace YuankunHuang.Unity.Core.Editor
{
    [CustomEditor(typeof(GameManager))]
    public class GameManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var gameManager = (GameManager)target;
            if (gameManager.AssetManagerConfig == null)
            {
                EditorGUILayout.HelpBox("AssetManagerConfig is not assigned. Please assign it in the GameManager component.", MessageType.Warning);

                if (GUILayout.Button("Auto Find AssetManagerConfig"))
                {
                    var config = AssetDatabase.LoadAssetAtPath<AssetManagerConfig>(AssetKeysGenerator.CONFIG_PATH);
                    if (config != null)
                    {
                        serializedObject.FindProperty("_assetManagerConfig").objectReferenceValue = config;
                        serializedObject.ApplyModifiedProperties();

                        Debug.Log("AssetManagerConfig assigned successfully.");
                    }
                    else
                    {
                        Debug.LogError("AssetManagerConfig not found. Please create one in the project.");
                    }
                }
            }
        }
    }
}
