using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

namespace YuankunHuang.SynthMind.Util
{
    /// <summary>
    /// @ingroup Utility
    /// @class TypeUtil
    /// @brief A utility class for performing general C# type-related operations.
    /// </summary>
    public static class TypeUtil
    {
        /// <summary>
        /// The name of Unity's runtime assembly
        /// </summary>
        public static string RuntimeAssemblyName = "Assembly-CSharp";

        /// <summary>
        /// The name of Unity's editor assembly
        /// </summary>
        public static string EditorAssemblyName = "Assembly-CSharp-Editor";

        /// <summary>
        /// Retrieves a type by its full name from the runtime or editor assembly.
        /// </summary>
        /// <param name="fullName">The full name of the type to retrieve.</param>
        /// <returns>The Type object if found, or null if not found.</returns>
        public static Type GetType(string fullName)
        {
            var output = Type.GetType($"{fullName}, {RuntimeAssemblyName}");
            if (output != null)
            {
                return output;
            }

            output = Type.GetType($"{fullName}, {EditorAssemblyName}");
            if (output != null)
            {
                return output;
            }

            return null;
        }

        /// <summary>
        /// Returns a friendly name for the type, including generic arguments if applicable.
        /// </summary>
        /// <param name="type">The type to get the friendly name for.</param>
        /// <returns>The friendly name of the type, including its generic arguments (e.g., "List<int>").</returns>
        public static string GetFriendlyName(this Type type)
        {
            if (!type.IsGenericType)
                return type.Name;

            var sb = new StringBuilder();
            var typeName = type.Name.Substring(0, type.Name.IndexOf('`'));
            sb.Append(typeName);
            sb.Append("<");

            Type[] typeArguments = type.GetGenericArguments();
            for (int i = 0; i < typeArguments.Length; i++)
            {
                sb.Append(GetFriendlyName(typeArguments[i]));
                if (i < typeArguments.Length - 1)
                    sb.Append(", ");
            }

            sb.Append(">");
            return sb.ToString();
        }

        /// <summary>
        /// Returns the full friendly name of the type, including its namespace and generic arguments.
        /// </summary>
        /// <param name="type">The type to get the full friendly name for.</param>
        /// <returns>The full friendly name of the type, including its namespace and generic arguments (e.g., "System.Collections.Generic.List<int>").</returns>
        public static string GetFriendlyFullName(this Type type)
        {
            if (!type.IsGenericType)
                return type.FullName;

            var sb = new StringBuilder();
            var typeName = type.FullName.Substring(0, type.FullName.IndexOf('`'));
            sb.Append(typeName);
            sb.Append("<");

            Type[] typeArguments = type.GetGenericArguments();
            for (int i = 0; i < typeArguments.Length; i++)
            {
                sb.Append(GetFriendlyName(typeArguments[i]));
                if (i < typeArguments.Length - 1)
                    sb.Append(", ");
            }

            sb.Append(">");
            return sb.ToString();
        }
    }
}