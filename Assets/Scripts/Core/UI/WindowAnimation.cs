using System;
using System.Collections;
using UnityEngine;

namespace YuankunHuang.Unity.UICore
{
    public enum AnimationType
    {
        Scale,
        Slide,
        Fade
    }

    public enum SlideDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    [Serializable]
    public class PopupAnimationSettings
    {
        public AnimationType enterAnimation = AnimationType.Scale;
        public AnimationType exitAnimation = AnimationType.Scale;
        public SlideDirection slideDirection = SlideDirection.Up;
        public float enterDuration = 0.3f;
        public float exitDuration = 0.2f;
        public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    }

    public static class WindowAnimation
    {
        public static IEnumerator PlayEnter(Transform target, CanvasGroup canvas, PopupAnimationSettings settings)
        {
            yield return Play(target, canvas, settings.enterAnimation, settings.slideDirection, 
                             settings.enterDuration, settings.curve, true);
        }

        public static IEnumerator PlayExit(Transform target, CanvasGroup canvas, PopupAnimationSettings settings)
        {
            yield return Play(target, canvas, settings.exitAnimation, settings.slideDirection, 
                             settings.exitDuration, settings.curve, false);
        }

        private static IEnumerator Play(Transform target, CanvasGroup canvas, AnimationType type, 
                                       SlideDirection direction, float duration, AnimationCurve curve, bool isEnter)
        {
            if (target == null) yield break;

            var startScale = target.localScale;
            var startPos = target.localPosition;
            var startAlpha = canvas ? canvas.alpha : 1f;

            SetInitialState(target, canvas, type, direction, isEnter, startScale, startPos, startAlpha);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = curve.Evaluate(elapsed / duration);
                ApplyAnimation(target, canvas, type, direction, isEnter, t, startScale, startPos, startAlpha);
                
                elapsed += Time.deltaTime;
                yield return null;
            }

            ApplyAnimation(target, canvas, type, direction, isEnter, 1f, startScale, startPos, startAlpha);
        }

        private static void SetInitialState(Transform target, CanvasGroup canvas, AnimationType type, 
                                          SlideDirection direction, bool isEnter, Vector3 originalScale, 
                                          Vector3 originalPos, float originalAlpha)
        {
            if (isEnter)
            {
                switch (type)
                {
                    case AnimationType.Scale:
                        target.localScale = Vector3.zero;
                        break;
                    case AnimationType.Slide:
                        target.localPosition = originalPos + GetSlideOffset(direction);
                        break;
                    case AnimationType.Fade:
                        if (canvas) canvas.alpha = 0f;
                        target.localScale = Vector3.zero;
                        break;
                }
            }
        }

        private static void ApplyAnimation(Transform target, CanvasGroup canvas, AnimationType type, 
                                         SlideDirection direction, bool isEnter, float t, 
                                         Vector3 originalScale, Vector3 originalPos, float originalAlpha)
        {
            float progress = isEnter ? t : (1f - t);

            switch (type)
            {
                case AnimationType.Scale:
                    target.localScale = Vector3.Lerp(Vector3.zero, originalScale, progress);
                    break;
                    
                case AnimationType.Slide:
                    var offset = GetSlideOffset(direction);
                    target.localPosition = Vector3.Lerp(originalPos + offset, originalPos, progress);
                    break;
                    
                case AnimationType.Fade:
                    if (canvas) canvas.alpha = Mathf.Lerp(0f, originalAlpha, progress);
                    target.localScale = Vector3.Lerp(Vector3.zero, originalScale, progress);
                    break;
            }
        }

        private static Vector3 GetSlideOffset(SlideDirection direction)
        {
            return direction switch
            {
                SlideDirection.Up => Vector3.up * 1000f,
                SlideDirection.Down => Vector3.down * 1000f,
                SlideDirection.Left => Vector3.left * 1000f,
                SlideDirection.Right => Vector3.right * 1000f,
                _ => Vector3.up * 1000f
            };
        }
    }
}