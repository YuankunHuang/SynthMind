using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace YuankunHuang.Unity.Core
{
    public class GeneralButton : Button
    {
#if UNITY_EDITOR
        /// <summary>
        /// Adds a new GeneralButton to the scene when selected from the Unity Editor's GameObject menu.
        /// </summary>
        [MenuItem("GameObject/UI/GeneralButton")]
        static void AddGeneralButton()
        {
            GameObject canvas = GameObject.Find("Canvas");
            if (canvas == null)
            {
                LogHelper.LogError($"No Canvas can be found!");
                return;
            }

            var buttonObj = new GameObject("GeneralButton");
            var parent = Selection.activeGameObject != null ? Selection.activeGameObject.transform : canvas.transform;
            Undo.RegisterCreatedObjectUndo(buttonObj, "Create GeneralButton"); // Register the undo action
            Undo.SetTransformParent(buttonObj.transform, parent, "Parent GeneralButton"); // Register the undo action for parenting

            var img = Undo.AddComponent<Image>(buttonObj); // Register the undo action for adding a component
            var button = Undo.AddComponent<GeneralButton>(buttonObj); // Register the undo action for adding a component
            button.transform.localPosition = Vector3.zero;
            button.transform.localRotation = Quaternion.identity;
            button.transform.localScale = Vector3.one;

            Selection.activeGameObject = buttonObj;
        }
#endif
    }
}