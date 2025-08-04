using UnityEngine;
using YuankunHuang.Unity.Core;

namespace YuankunHuang.Unity.Util
{
    public static class RectTransformUtil
    {
        public static Rect GetWorldRect(this RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                LogHelper.LogError("RectTransform is null.");
                return default;
            }
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            return new Rect(corners[0], corners[2] - corners[0]);
        }

        /// <summary>
        /// Sets the anchor and pivot of a RectTransform to the center.
        /// </summary>
        /// <param name="rectTransform">The RectTransform to modify.</param>
        public static void SetAnchorAndPivotToCenter(this RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                LogHelper.LogError("RectTransform is null.");
                return;
            }
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }
    }
}