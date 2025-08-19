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
        private const float FINAL_FRAME_THRESHOLD = 0.99f;
        private const float MAX_OVERSHOOT = 1.05f;

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
                if (t >= FINAL_FRAME_THRESHOLD) return 1;

                float p = 0.3f;
                float s = p / 4f;
                float result = Mathf.Pow(2, -10 * t) * Mathf.Sin((t - s) * (2 * Mathf.PI) / p) + 1;
                return Mathf.Clamp(result, 0f, 1.1f);
            }

            public static float ElasticIn(float t)
            {
                if (t == 0) return 0;
                if (t >= FINAL_FRAME_THRESHOLD) return 1;
                return 1f - ElasticOut(1f - t);
            }

            public static float BackOut(float t)
            {
                const float c1 = 1.70158f;
                const float c3 = c1 + 1f;
                float result = 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
                return Mathf.Min(result, MAX_OVERSHOOT);
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
                yield break;
            }

            var originalScale = container.localScale;
            var originalPosition = container.localPosition;
            var originalRotation = container.localRotation;
            var originalAlpha = canvasGroup ? canvasGroup.alpha : 1f;

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
                yield break;
            }

            ApplyEnterAnimation(container, canvasGroup, settings, 1.0f,
                             originalScale, originalPosition, originalRotation, originalAlpha);
        }

        public static IEnumerator AnimatePopupExit(Transform container, CanvasGroup canvasGroup, PopupAnimationSettings settings, Action onComplete = null)
        {
            if (container == null)
            {
                onComplete?.Invoke();
                yield break;
            }

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

            ApplyExitAnimation(container, canvasGroup, settings, 1.0f,
                             originalScale, originalPosition, originalRotation, originalAlpha);

            if (settings.exitAnimation != PopupExitType.None)
            {
                switch (settings.exitAnimation)
                {
                    case PopupExitType.ScaleOut:
                    case PopupExitType.FadeOutScale:
                    case PopupExitType.ElasticOut:
                    case PopupExitType.RotateOut:
                    case PopupExitType.PunchOut:
                        if (container != null)
                            container.localScale = Vector3.zero;
                        break;
                }
            }

            onComplete?.Invoke();
        }

        private static void SetInitialState(Transform container, CanvasGroup canvasGroup, PopupAnimationType animationType)
        {
            switch (animationType)
            {
                case PopupAnimationType.None:
                    break;

                case PopupAnimationType.BounceIn:
                case PopupAnimationType.ElasticIn:
                case PopupAnimationType.BackIn:
                case PopupAnimationType.PunchScale:
                case PopupAnimationType.SpringIn:
                case PopupAnimationType.Wobble:
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
            }
        }

        private static void ApplyEnterAnimation(Transform container, CanvasGroup canvasGroup, PopupAnimationSettings settings,
            float t, Vector3 originalScale, Vector3 originalPosition, Quaternion originalRotation, float originalAlpha)
        {
            if (container == null) return;

            bool isFinalFrame = t >= FINAL_FRAME_THRESHOLD;

            switch (settings.enterAnimation)
            {
                case PopupAnimationType.None:
                    container.localScale = originalScale;
                    container.localPosition = originalPosition;
                    container.localRotation = originalRotation;
                    if (canvasGroup) canvasGroup.alpha = originalAlpha;
                    break;

                case PopupAnimationType.BounceIn:
                    if (isFinalFrame)
                    {
                        container.localScale = originalScale;
                    }
                    else
                    {
                        float bounceT = Easing.BounceOut(t);
                        container.localScale = Vector3.Lerp(Vector3.zero, originalScale, bounceT);
                    }
                    break;

                case PopupAnimationType.ElasticIn:
                    if (isFinalFrame)
                    {
                        container.localScale = originalScale;
                    }
                    else
                    {
                        float elasticT = Easing.ElasticOut(t);
                        float clampedT = Mathf.Clamp(elasticT, 0f, MAX_OVERSHOOT);
                        container.localScale = Vector3.Lerp(Vector3.zero, originalScale, clampedT);
                    }
                    break;

                case PopupAnimationType.BackIn:
                    if (isFinalFrame)
                    {
                        container.localScale = originalScale;
                    }
                    else
                    {
                        float backT = Easing.BackOut(t);
                        container.localScale = Vector3.Lerp(Vector3.zero, originalScale, Mathf.Clamp01(backT));
                    }
                    break;

                case PopupAnimationType.ScaleInWithRotation:
                    if (isFinalFrame)
                    {
                        container.localScale = originalScale;
                        container.localRotation = originalRotation;
                    }
                    else
                    {
                        float scaleT = Easing.BackOut(t);
                        container.localScale = Vector3.Lerp(Vector3.zero, originalScale, Mathf.Clamp01(scaleT));
                        container.localRotation = Quaternion.Lerp(
                            Quaternion.Euler(0, 0, -180f), originalRotation, t);
                    }
                    break;

                case PopupAnimationType.SlideInFromTop:
                    if (isFinalFrame)
                    {
                        container.localPosition = originalPosition;
                    }
                    else
                    {
                        float slideT = Easing.BackOut(t);
                        Vector3 startPos = originalPosition + Vector3.up * Screen.height;
                        container.localPosition = Vector3.Lerp(startPos, originalPosition, Mathf.Clamp01(slideT));
                    }
                    break;

                case PopupAnimationType.SlideInFromBottom:
                    if (isFinalFrame)
                    {
                        container.localPosition = originalPosition;
                    }
                    else
                    {
                        float slideT = Easing.BackOut(t);
                        Vector3 startPos = originalPosition + Vector3.down * Screen.height;
                        container.localPosition = Vector3.Lerp(startPos, originalPosition, Mathf.Clamp01(slideT));
                    }
                    break;

                case PopupAnimationType.FadeInScale:
                    if (isFinalFrame)
                    {
                        container.localScale = originalScale;
                        if (canvasGroup) canvasGroup.alpha = originalAlpha;
                    }
                    else
                    {
                        float easeT = Easing.BackOut(t);
                        container.localScale = Vector3.Lerp(Vector3.zero, originalScale, Mathf.Clamp01(easeT));
                        if (canvasGroup) canvasGroup.alpha = Mathf.Lerp(0f, originalAlpha, t);
                    }
                    break;

                case PopupAnimationType.PunchScale:
                    if (isFinalFrame)
                    {
                        container.localScale = originalScale;
                    }
                    else
                    {
                        float punchT = t < 0.5f ?
                            Mathf.Sin(t * Mathf.PI * 4f) * (1f - t) + t :
                            Easing.BounceOut((t - 0.5f) * 2f) * 0.5f + 0.5f;
                        container.localScale = Vector3.Lerp(Vector3.zero, originalScale, punchT);
                    }
                    break;

                case PopupAnimationType.SpringIn:
                    if (isFinalFrame)
                    {
                        container.localScale = originalScale;
                    }
                    else
                    {
                        float springT = Easing.SpringDamping(t, 0.6f);
                        container.localScale = Vector3.Lerp(Vector3.zero, originalScale, Mathf.Clamp01(springT));
                    }
                    break;

                case PopupAnimationType.Wobble:
                    if (isFinalFrame)
                    {
                        container.localScale = originalScale;
                        container.localRotation = originalRotation;
                    }
                    else
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
            if (container == null) return;

            bool isFinalFrame = t >= FINAL_FRAME_THRESHOLD;

            switch (settings.exitAnimation)
            {
                case PopupExitType.None:
                    container.localScale = originalScale;
                    container.localPosition = originalPosition;
                    container.localRotation = originalRotation;
                    if (canvasGroup) canvasGroup.alpha = originalAlpha;
                    break;

                case PopupExitType.ScaleOut:
                    if (isFinalFrame)
                    {
                        container.localScale = Vector3.zero;
                    }
                    else
                    {
                        container.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
                    }
                    break;

                case PopupExitType.SlideOutToTop:
                    if (isFinalFrame)
                    {
                        container.localPosition = originalPosition + Vector3.up * Screen.height;
                    }
                    else
                    {
                        Vector3 endPos = originalPosition + Vector3.up * Screen.height;
                        container.localPosition = Vector3.Lerp(originalPosition, endPos, Easing.BackIn(t));
                    }
                    break;

                case PopupExitType.SlideOutToBottom:
                    if (isFinalFrame)
                    {
                        container.localPosition = originalPosition + Vector3.down * Screen.height;
                    }
                    else
                    {
                        Vector3 endPos = originalPosition + Vector3.down * Screen.height;
                        container.localPosition = Vector3.Lerp(originalPosition, endPos, Easing.BackIn(t));
                    }
                    break;

                case PopupExitType.FadeOutScale:
                    if (isFinalFrame)
                    {
                        container.localScale = Vector3.zero;
                        if (canvasGroup) canvasGroup.alpha = 0f;
                    }
                    else
                    {
                        container.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
                        if (canvasGroup) canvasGroup.alpha = Mathf.Lerp(originalAlpha, 0f, t);
                    }
                    break;

                case PopupExitType.RotateOut:
                    if (isFinalFrame)
                    {
                        container.localScale = Vector3.zero;
                    }
                    else
                    {
                        container.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
                        container.localRotation = originalRotation * Quaternion.Euler(0, 0, t * 360f);
                    }
                    break;

                case PopupExitType.PunchOut:
                    if (isFinalFrame)
                    {
                        container.localScale = Vector3.zero;
                    }
                    else
                    {
                        float punchScale = 1f + Mathf.Sin(t * Mathf.PI * 2f) * 0.3f * (1f - t);
                        container.localScale = originalScale * Mathf.Lerp(punchScale, 0f, t);
                    }
                    break;

                case PopupExitType.ElasticOut:
                    if (isFinalFrame)
                    {
                        container.localScale = Vector3.zero;
                    }
                    else
                    {
                        float elasticT = Easing.ElasticIn(t);
                        container.localScale = Vector3.Lerp(originalScale, Vector3.zero, elasticT);
                    }
                    break;
            }
        }
    }
}