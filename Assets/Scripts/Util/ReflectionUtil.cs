using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using System.Linq;

namespace YuankunHuang.SynthMind.Util
{
    /// <summary>
    /// @ingroup Utility
    /// @class ReflectionUtil
    /// @brief A utility class for performing general C# Reflection operations.
    /// </summary>
    public static class ReflectionUtil
    {
        /// <summary>
        /// Retrieves an array of public instance properties from a specified type, excluding properties declared by the 'object' class.
        /// </summary>
        /// <param name="type">The type to retrieve properties from.</param>
        /// <returns>An array of 'PropertyInfo' objects representing the public instance properties, excluding those from the 'object' class.</returns>
        /// <exception cref="ArgumentNullException">Throw when the provided Type is null.</exception>
        public static PropertyInfo[] GetPublicInstancePropertiesExcludingObject(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                       .Where(p => p.DeclaringType != typeof(object))
                       .ToArray();
        }
    }
}