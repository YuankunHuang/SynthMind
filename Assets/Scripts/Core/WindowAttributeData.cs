using UnityEngine;

namespace YuankunHuang.Unity.Core
{
    [CreateAssetMenu(fileName = "WindowAttributeData", menuName = "SynthMind/Window Attribute Data", order = 1)]
    public class WindowAttributeData : ScriptableObject
    {
        public bool hasMask = true;
        public bool usePopupScaleAnimation = false;
        public float popupScaleDuration = 0.3f;
        public bool useBlurredBackground = false;
        public bool selfDestructOnCovered = false;
    }
}