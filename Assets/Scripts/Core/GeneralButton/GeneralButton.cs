using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using YuankunHuang.Unity.Core;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace YuankunHuang.Unity.UICore
{
    public class GeneralButton : Button, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] private bool _enableScaleAnimation = true;
        [SerializeField] private float _pressedScale = 0.9f;
        [SerializeField] private float _animationDuration = 0.05f;

        private Vector3 _originalScale;
        private Coroutine _scaleCoroutine;

        protected override void Awake()
        {
            base.Awake();
            _originalScale = transform.localScale;
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            AnimateScale(Vector3.one * _pressedScale);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            AnimateScale(_originalScale);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            AnimateScale(_originalScale);
        }

        private void AnimateScale(Vector3 targetScale)
        {
            if (_scaleCoroutine != null)
            {
                StopCoroutine(_scaleCoroutine);
            }
            
            if (_enableScaleAnimation)
            {
                _scaleCoroutine = StartCoroutine(ScaleTo(targetScale));
            }
        }

        private IEnumerator ScaleTo(Vector3 target)
        {
            var current = transform.localScale;
            var time = 0f;

            while (time < _animationDuration)
            {
                transform.localScale = Vector3.Lerp(current, target, time / _animationDuration);
                time += Time.unscaledDeltaTime;
                yield return null;
            }

            transform.localScale = target;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Adds a new GeneralButton to the scene when selected from the Unity Editor's GameObject menu.
        /// </summary>
        [MenuItem("GameObject/UI/GeneralButton")]
        static void AddGeneralButton()
        {
            var buttonObj = new GameObject("GeneralButton");
            Transform parent = null;
            if (Selection.activeGameObject != null)
            {
                parent = Selection.activeGameObject.transform;
            }
            else
            {
                GameObject canvas = GameObject.Find("Canvas");
                if (canvas == null)
                {
                    LogHelper.LogError($"No Canvas can be found!");
                    return;
                }
                parent = canvas.transform;
            }
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