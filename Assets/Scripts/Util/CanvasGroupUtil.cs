using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YuankunHuang.Unity.Util
{
    /// <summary>
    /// @ingroup Utility
    /// @class CanvasGroupUtil
    /// @brief A utility class for general CanvasGroup-related operations.
    /// </summary>
    public static class CanvasGroupUtil
    {
        /// <summary>
        /// Enables the CanvasGroup, making it fully visible and interactive.
        /// </summary>
        /// <param name="cg">The CanvasGroup to modify.</param>
        public static void CanvasGroupOn(this CanvasGroup cg)
        {
            if (cg == null)
                return;

            cg.alpha = 1;
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }

        /// <summary>
        /// Disables the CanvasGroup, making it fully invisible and non-interactive.
        /// </summary>
        /// <param name="cg">The CanvasGroup to modify.</param>
        public static void CanvasGroupOff(this CanvasGroup cg)
        {
            if (cg == null)
                return;

            cg.alpha = 0;
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }
    }
}