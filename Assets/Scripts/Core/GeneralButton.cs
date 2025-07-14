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
    /// @ingroup Core
    /// @class GeneralButton
    /// @brief A custom button class that extends the Unity Button component and adds visual effects, scaling, and handling of pointer events.
    /// 
    /// The GeneralButton class is a customized version of the Unity Button, providing additional functionality such as visual
    /// feedback when the pointer enters, exits, or presses the button. The button scales down when pressed and restores its
    /// original size when released. It also allows for custom navigation behavior within the Unity UI system.
    public class GeneralButton : Button
    {
        /// <summary>
        /// The scaling factor applied when the button is pressed.
        /// </summary>
        public static readonly float PressedSize = .95f;

        /// <summary>
        /// The default scale of the button when not pressed.
        /// </summary>
        public static readonly Vector3 DefaultScale = Vector3.zero;

        private Vector3 _initialScale = DefaultScale;
        private bool _isPointerOnTop;

        /// <summary>
        /// Initializes the button's navigation settings when enabled.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            var nav = navigation;
            var useDefaultButtonNavigation = false;
            if (useDefaultButtonNavigation)
            {
                nav.mode = Navigation.Mode.Automatic;
            }
            else
            {
                nav.mode = Navigation.Mode.None;
            }
            navigation = nav;
        }

        /// <summary>
        /// Called when the pointer enters the button area. Changes the button's appearance to the highlighted sprite.
        /// </summary>
        /// <param name="eventData">Event data associated with the pointer entering the button.</param>
        public override void OnPointerEnter(PointerEventData eventData)
        {
            _isPointerOnTop = true;

            base.OnPointerEnter(eventData);

            if (image != null)
            {
                image.sprite = spriteState.highlightedSprite;
            }
        }

        /// <summary>
        /// Called when the pointer exits the button area. Changes the button's appearance to the disabled sprite.
        /// </summary>
        /// <param name="eventData">Event data associated with the pointer exiting the button.</param>
        public override void OnPointerExit(PointerEventData eventData)
        {
            _isPointerOnTop = false;

            base.OnPointerExit(eventData);

            if (image != null)
            {
                image.sprite = spriteState.disabledSprite;
            }
        }

        /// <summary>
        /// Called when the button is pressed. Scales the button down and changes its appearance to the pressed sprite.
        /// </summary>
        /// <param name="eventData">Event data associated with the pointer pressing the button.</param>
        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);

            _initialScale = transform.localScale;
            transform.localScale = _initialScale * PressedSize;
            
            if (image != null)
            {
                image.sprite = spriteState.pressedSprite;
            }
        }

        /// <summary>
        /// Called when the pointer is released. Restores the button's scale and updates its appearance based on whether the
        /// pointer is still on top of the button or not.
        /// </summary>
        /// <param name="eventData">Event data associated with the pointer release.</param>
        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            transform.localScale = _initialScale;

            if (image != null)
            {
                if (_isPointerOnTop)
                {
                    image.sprite = spriteState.highlightedSprite;
                }
                else
                {
                    image.sprite = spriteState.disabledSprite;
                }
            }
        }

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
                Logger.LogError($"No Canvas can be found!");
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