using UnityEditor;
using UnityEngine;

namespace YuankunHuang.Unity.AssetCore.Editor
{
    [CustomPropertyDrawer(typeof(AssetEntry))]
    public class AssetEntryDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var keyProp = property.FindPropertyRelative("key");
            var assetProp = property.FindPropertyRelative("asset");
            var typeProp = property.FindPropertyRelative("type");

            float width = position.width;
            float keyWidth = width * 0.3f;
            float assetWidth = width * 0.5f;
            float typeWidth = width * 0.2f;

            Rect keyRect = new Rect(position.x, position.y, keyWidth, position.height);
            Rect assetRect = new Rect(position.x + keyWidth, position.y, assetWidth, position.height);
            Rect typeRect = new Rect(position.x + keyWidth + assetWidth, position.y, typeWidth, position.height);

            EditorGUI.PropertyField(keyRect, keyProp, GUIContent.none);
            EditorGUI.PropertyField(assetRect, assetProp, GUIContent.none);
            EditorGUI.PropertyField(typeRect, typeProp, GUIContent.none);

            EditorGUI.EndProperty();
        }
    }
}