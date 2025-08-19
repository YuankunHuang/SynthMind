using System;
using System.Collections;
using UnityEngine;

namespace YuankunHuang.Unity.UICore
{
    public enum PopupAnimationType
    {
        None,
        BounceIn,
        ElasticIn,
        BackIn,
        ScaleInWithRotation,
        SlideInFromTop,
        SlideInFromBottom,
        FadeInScale,
        PunchScale,
        SpringIn,
        Wobble
    }

    public enum PopupExitType
    {
        None,
        ScaleOut,
        SlideOutToTop,
        SlideOutToBottom,
        FadeOutScale,
        RotateOut,
        PunchOut,
        ElasticOut
    }

    [System.Serializable]
    public class PopupAnimationSettings
    {
        [Header("Animation Types")]
        public PopupAnimationType enterAnimation = PopupAnimationType.BounceIn;
        public PopupExitType exitAnimation = PopupExitType.ScaleOut;

        [Header("Timing")]
        public float enterDuration = 0.25f;
        public float exitDuration = 0.25f;
        public float delayBefore = 0f;

        [Header("Easing Parameters")]
        [Range(0.1f, 3f)] public float bounceAmplitude = 1.5f;
        [Range(1f, 10f)] public float elasticStrength = 3f;
        [Range(0.1f, 2f)] public float backOvershoot = 1.2f;

        [Header("Additional Effects")]
        public bool useScaleEffect = true;
        public bool useFadeEffect = false;
        public bool useRotationEffect = false;
        public AnimationCurve customCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    }

    public static class PopupAnimator
    {
        public static class Easing
        {
            public static float BounceOut(float t)
            {
                if (t < 1f / 2.75f)
                {
                    return 7.5625f * t * t;
                }
                else if (t < 2f / 2.75f)
                {
                    t -= 1.5f / 2.75f;
                    return 7.5625f * t * t + 0.75f;
                }
                else if (t < 2.5f / 2.75f)
                {
                    t -= 2.25f / 2.75f;
                    return 7.5625f * t * t + 0.9375f;
                }
                else
                {
                    t -= 2.625f / 2.75f;
                    return 7.5625f * t * t + 0.984375f;
                }
            }

            public static float BounceIn(float t)
            {
                return 1f - BounceOut(1f - t);
            }

            public static float ElasticOut(float t)
            {
                if (t == 0) return 0;
                if (t == 1) return 1;

                float p = 0.3f;
                float s = p / 4f;
                return Mathf.Pow(2, -10 * t) * Mathf.Sin((t - s) * (2 * Mathf.PI) / p) + 1;
            }

            public static float ElasticIn(float t)
            {
                return 1f - ElasticOut(1f - t);
            }

            public static float BackOut(float t)
            {
                const float c1 = 1.70158f;
                const float c3 = c1 + 1f;
                return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
            }

            public static float BackIn(float t)
            {
                const float c1 = 1.70158f;
                const float c3 = c1 + 1f;
                return c3 * t * t * t - c1 * t * t;
            }

            public static float SpringDamping(float t, float damping = 0.8f)
            {
                return Mathf.Sin(t * Mathf.PI * 3f) * Mathf.Pow(1f - t, damping) + t;
            }
        }

        public static IEnumerator AnimatePopupEnter(Transform container, CanvasGroup canvasGroup, PopupAnimationSettings settings)
        {
            if (settings.delayBefore > 0)
            {
                yield return new WaitForSeconds(settings.delayBefore);
            }

            if (container == null)
            {
                yield break; // Exit if container is null
            }

            // stay as original state
            var originalScale = container.localScale;
            var originalPosition = container.localPosition;
            var originalRotation = container.localRotation;
            var originalAlpha = canvasGroup ? canvasGroup.alpha : 1f;

            // set initial state based on animation type
            SetInitialState(container, canvasGroup, settings.enterAnimation);

            float time = 0f;
            while (time < settings.enterDuration)
            {
                time += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(time / settings.enterDuration);

                ApplyEnterAnimation(container, canvasGroup, settings, normalizedTime,
                                 originalScale, originalPosition, originalRotation, originalAlpha);

                yield return null;
            }

            if (container == null)
            {
                yield break; // Exit if container is null
            }

            // ensure final state is correct
            container.localScale = originalScale;
            container.localPosition = originalPosition;
            container.localRotation = originalRotation;
            if (canvasGroup) canvasGroup.alpha = originalAlpha;
        }

        public static IEnumerator AnimatePopupExit(Transform container, CanvasGroup canvasGroup, PopupAnimationSettings settings, Action onComplete = null)
        {
            var originalScale = container.localScale;
            var originalPosition = container.localPosition;
            var originalRotation = container.localRotation;
            var originalAlpha = canvasGroup ? canvasGroup.alpha : 1f;

            float time = 0f;
            while (time < settings.exitDuration)
            {
                time += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(time / settings.exitDuration);

                ApplyExitAnimation(container, canvasGroup, settings, normalizedTime,
                                 originalScale, originalPosition, originalRotation, originalAlpha);

                yield return null;
            }

            onComplete?.Invoke();
        }

        private static void SetInitialState(Transform container, CanvasGroup canvasGroup, PopupAnimationType animationType)
        {
            switch (animationType)
            {
                case PopupAnimationType.None:
                    // No initial state change
                    break;
                case PopupAnimationType.BounceIn:
                case PopupAnimationType.ElasticIn:
                case PopupAnimationType.BackIn:
                case PopupAnimationType.PunchScale:
                case PopupAnimationType.SpringIn:
                    container.localScale = Vector3.zero;
                    break;

                case PopupAnimationType.ScaleInWithRotation:
                    container.localScale = Vector3.zero;
                    container.localRotation = Quaternion.Euler(0, 0, -180f);
                    break;

                case PopupAnimationType.SlideInFromTop:
                    container.localPosition += Vector3.up * Screen.height;
                    break;

                case PopupAnimationType.SlideInFromBottom:
                    container.localPosition += Vector3.down * Screen.height;
                    break;

                case PopupAnimationType.FadeInScale:
                    container.localScale = Vector3.zero;
                    if (canvasGroup) canvasGroup.alpha = 0f;
                    break;

                case PopupAnimationType.Wobble:
                    container.localScale = Vector3.zero;
                    break;
            }
        }

        private static void ApplyEnterAnimation(Transform container, CanvasGroup canvasGroup, PopupAnimationSettings settings,
            float t, Vector3 originalScale, Vector3 originalPosition, Quaternion originalRotation, float originalAlpha)
        {
            if (container == null)
            {
                return;
            }

            switch (settings.enterAnimation)
            {
                case PopupAnimationType.None:
                    // No animation, just set to original state
                    container.localScale = originalScale;
                    container.localPosition = originalPosition;
                    container.localRotation = originalRotation;
                    if (canvasGroup) canvasGroup.alpha = originalAlpha;
                    break;
                case PopupAnimationType.BounceIn:
                    {
                        float bounceT = Easing.BounceOut(t);
                        container.localScale = Vector3.Lerp(Vector3.zero, originalScale, bounceT);
                    }
                    break;

                case PopupAnimationType.ElasticIn:
                    {
                        float elasticT = Easing.ElasticOut(t);
                        container.localScale = Vector3.Lerp(Vector3.zero, originalScale, elasticT);
                    }
                    break;

                case PopupAnimationType.BackIn:
                    {
                        float backT = Easing.BackOut(t);
                        container.localScale = Vector3.Lerp(Vector3.zero, originalScale, backT);
                    }
                    break;

                case PopupAnimationType.ScaleInWithRotation:
                    {
                        float scaleT = Easing.BackOut(t);
                        container.localScale = Vector3.Lerp(Vector3.zero, originalScale, scaleT);
                        container.localRotation = Quaternion.Lerp(
                            Quaternion.Euler(0, 0, -180f), originalRotation, t);
                    }
                    break;

                case PopupAnimationType.SlideInFromTop:
                    {
                        float slideT = Easing.BackOut(t);
                        Vector3 startPos = originalPosition + Vector3.up * Screen.height;
                        container.localPosition = Vector3.Lerp(startPos, originalPosition, slideT);
                    }
                    break;

                case PopupAnimationType.SlideInFromBottom:
                    {
                        float slideT = Easing.BackOut(t);
                        Vector3 startPos = originalPosition + Vector3.down * Screen.height;
                        container.localPosition = Vector3.Lerp(startPos, originalPosition, slideT);
                    }
                    break;

                case PopupAnimationType.FadeInScale:
                    {
                        float easeT = Easing.BackOut(t);
                        container.localScale = Vector3.Lerp(Vector3.zero, originalScale, easeT);
                        if (canvasGroup) canvasGroup.alpha = Mathf.Lerp(0f, originalAlpha, t);
                    }
                    break;

                case PopupAnimationType.PunchScale:
                    {
                        float punchT = t < 0.5f ?
                            Mathf.Sin(t * Mathf.PI * 4f) * (1f - t) + t :
                            Easing.BounceOut((t - 0.5f) * 2f) * 0.5f + 0.5f;
                        container.localScale = Vector3.Lerp(Vector3.zero, originalScale, punchT);
                    }
                    break;

                case PopupAnimationType.SpringIn:
                    {
                        float springT = Easing.SpringDamping(t, 0.6f);
                        container.localScale = Vector3.Lerp(Vector3.zero, originalScale, springT);
                    }
                    break;

                case PopupAnimationType.Wobble:
                    {
                        float wobbleScale = Easing.BounceOut(t);
                        float wobbleRotation = Mathf.Sin(t * Mathf.PI * 8f) * (1f - t) * 15f;
                        container.localScale = Vector3.Lerp(Vector3.zero, originalScale, wobbleScale);
                        container.localRotation = originalRotation * Quaternion.Euler(0, 0, wobbleRotation);
                    }
                    break;
            }
        }

        private static void ApplyExitAnimation(Transform container, CanvasGroup canvasGroup, PopupAnimationSettings settings,
            float t, Vector3 originalScale, Vector3 originalPosition, Quaternion originalRotation, float originalAlpha)
        {
            if (container == null)
            {
                return;
            }

            switch (settings.exitAnimation)
            {
                case PopupExitType.None:
                    // No exit animation, just set to original state
                    container.localScale = originalScale;
                    container.localPosition = originalPosition;
                    container.localRotation = originalRotation;
                    if (canvasGroup) canvasGroup.alpha = originalAlpha;
                    break;
                case PopupExitType.ScaleOut:
                    container.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
                    break;

                case PopupExitType.SlideOutToTop:
                    {
                        Vector3 endPos = originalPosition + Vector3.up * Screen.height;
                        container.localPosition = Vector3.Lerp(originalPosition, endPos, Easing.BackIn(t));
                    }
                    break;

                case PopupExitType.SlideOutToBottom:
                    {
                        Vector3 endPos = originalPosition + Vector3.down * Screen.height;
                        container.localPosition = Vector3.Lerp(originalPosition, endPos, Easing.BackIn(t));
                    }
                    break;

                case PopupExitType.FadeOutScale:
                    container.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
                    if (canvasGroup) canvasGroup.alpha = Mathf.Lerp(originalAlpha, 0f, t);
                    break;

                case PopupExitType.RotateOut:
                    container.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
                    container.localRotation = originalRotation * Quaternion.Euler(0, 0, t * 360f);
                    break;

                case PopupExitType.PunchOut:
                    {
                        float punchScale = 1f + Mathf.Sin(t * Mathf.PI * 2f) * 0.3f * (1f - t);
                        container.localScale = originalScale * Mathf.Lerp(punchScale, 0f, t);
                    }
                    break;

                case PopupExitType.ElasticOut:
                    {
                        float elasticT = Easing.ElasticIn(t);
                        container.localScale = Vector3.Lerp(originalScale, Vector3.zero, elasticT);
                    }
                    break;
            }
        }
    }
}