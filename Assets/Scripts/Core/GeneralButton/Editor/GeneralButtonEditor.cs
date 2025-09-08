using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;
using UnityEditor;
using YuankunHuang.Unity.UICore;

namespace YuankunHuang.Unity.Core.Editor
{
    [CustomEditor(typeof(GeneralButton), true)]
    [CanEditMultipleObjects]
    public class GeneralButtonEditor : ButtonEditor
    {
        SerializedProperty enableScaleAnimationProp;
        SerializedProperty pressedScaleProp;
        SerializedProperty animationDurationProp;

        protected override void OnEnable()
        {
            base.OnEnable();
            enableScaleAnimationProp = serializedObject.FindProperty("_enableScaleAnimation");
            pressedScaleProp = serializedObject.FindProperty("_pressedScale");
            animationDurationProp = serializedObject.FindProperty("_animationDuration");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.LabelField("General Button Settings", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(enableScaleAnimationProp);
            if (enableScaleAnimationProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(pressedScaleProp);
                EditorGUILayout.PropertyField(animationDurationProp);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();


            serializedObject.ApplyModifiedProperties();
        }
    }
}