using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YuankunHuang.Unity.Util
{
    /// <summary>
    /// @ingroup Utility
    /// @class ColorUtil
    /// @brief A utility class for general color-related definitions and operations.
    /// </summary>
    public static class ColorUtil
    {
        /// <summary>
        /// A static class containing predefined colors.
        /// </summary>
        public static class Colors
        {
            public static Color ChatBase = GetColorFromHex("#B2FFFFFF");
            public static Color Disabled = GetColorFromHex("#6A7C92FF");
            public static Color Error = GetColorFromHex("#FF4D4DFF");
            public static Color Friendly = GetColorFromHex("#FFE77AFF");
            public static Color Highlight = GetColorFromHex("#00FFFFFF");
            public static Color Neutral = GetColorFromHex("#F0F0F0FF");
            public static Color Primary = GetColorFromHex("#00CFFFFF");
            public static Color Secondary = GetColorFromHex("#88F7FFFF");
            public static Color Success = GetColorFromHex("#00FF88FF");
            public static Color System = GetColorFromHex("#FFBA4AFF");
            public static Color Warning = GetColorFromHex("#FFAD4DFF");
            public static Color ButtonBG = GetColorFromHex("#1C245CFF");
            public static Color ChatPanelBG = GetColorFromHex("#3A1F0FFF");
            public static Color GridOverlay = GetColorFromHex("#2A4B6EFF");
            public static Color MainBG = GetColorFromHex("#0B0F1CFF");
        }

        /// <summary>
        /// Converts a hex color string to a Unity Color object.
        /// Supports both RGB and RGBA formats.
        /// </summary>
        /// <param name="hex">The hex color string (e.g., "#FFFFFF" or "#FFFFFFFF").</param>
        /// <returns>A Unity Color object representing the given hex color.</returns>
        public static Color GetColorFromHex(string hex)
        {
            // In case the string is formatted '0xFFFFFF'
            hex = hex.Replace("0x", "");

            // In case the string is formatted '#FFFFFF'
            hex = hex.Replace("#", "");

            byte a = 255; // assume fully opaque unless specified in the string

            // Get the individual components
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

            // Check if alpha is specified in the string
            if (hex.Length == 8)
            {
                a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
            }

            // Return the color
            return new Color32(r, g, b, a);
        }
    }
}