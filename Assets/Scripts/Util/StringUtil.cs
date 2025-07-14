using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

namespace YuankunHuang.Unity.Util
{
    /// <summary>
    /// @ingroup Utility
    /// @class StringUtil
    /// @brief A utility class for performing general string-related operations.
    /// </summary>
    public static class StringUtil
    {
        /// <summary>
        /// Converts a string to camel case (e.g., "ExampleString" becomes "exampleString").
        /// </summary>
        /// <param name="str">The string to convert to camel case.</param>
        /// <returns>The input string converted to camel case.</returns>
        public static string ConvertToCamel(string str)
        {
            var camel = char.ToLowerInvariant(str[0]) + str.Substring(1);
            return camel;
        }

        /// <summary>
        /// Converts a string to pascal case (e.g., "exampleString" becomes "ExampleString").
        /// </summary>
        /// <param name="str">The string to convert to pascal case.</param>
        /// <returns>The input string converted to pascal case.</returns>
        public static string ConvertToPascal(string str)
        {
            var pascal = char.ToUpperInvariant(str[0]) + str.Substring(1);
            return pascal;
        }
    }
}