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
            public static Color DateRed = GetColorFromHex("#C76645");
            public static Color LightYellow = GetColorFromHex("#FFF8B4");
            public static Color LaurelGreen = GetColorFromHex("#9AB89C");
            public static Color SwampGreen = GetColorFromHex("#0A2729");
            public static Color Black = GetColorFromHex("#151E16");
            public static Color Edward = GetColorFromHex("#A8AEA9");
            public static Color LightGrey = GetColorFromHex("#E1EAE2");
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