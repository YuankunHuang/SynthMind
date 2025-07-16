using UnityEngine;

namespace YuankunHuang.Unity.Core
{
    [CreateAssetMenu(fileName = "WindowAttributeData", menuName = "SynthMind/Window Attribute Data", order = 1)]
    public class WindowAttributeData : ScriptableObject
    {
        public bool hasMask;
        public bool usePopupScaleAnimation;
        public bool useBlurredBackground;
    }
}