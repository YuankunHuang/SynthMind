using UnityEngine;

namespace YuankunHuang.Unity.UICore
{
    [CreateAssetMenu(fileName = "WindowAttributeData", menuName = "SynthMind/Window Attribute Data", order = 1)]
    public class WindowAttributeData : ScriptableObject
    {
        [Header("Basic Settings")]
        public bool hasMask = true;
        public bool useBlurredBackground = false;
        public bool selfDestructOnCovered = false;

        [Header("Animation Settings")]
        public bool usePopupAnimation = true;
        public PopupAnimationSettings animationSettings = new PopupAnimationSettings();
    }
}