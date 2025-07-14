using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Logger = YuankunHuang.Unity.Core.Logger;

namespace YuankunHuang.Unity.Util
{
    /// <summary>
    /// @ingroup Utility
    /// @class PlayerPrefsUtil
    /// @brief A utility class for general PlayerPrefs-related operations.
    /// </summary>
    public static class PlayerPrefsUtil
    {
        /// <summary>
        /// Checks if a PlayerPrefs key is valid (not null or empty).
        /// </summary>
        /// <param name="key">The PlayerPrefs key to check.</param>
        /// <returns>True if the key is valid; otherwise, false.</returns>
        public static bool IsValidKey(string key)
        {
            return !string.IsNullOrEmpty(key);
        }

        /// <summary>
        /// Checks if a PlayerPrefs key is both valid and stored.
        /// </summary>
        /// <param name="key">The PlayerPrefs key to check.</param>
        /// <returns>True if the key is valid and exists in PlayerPrefs; otherwise, false.</returns>
        public static bool HasKey(string key)
        {
            if (!IsValidKey(key))
            {
                return false;
            }

            return PlayerPrefs.HasKey(key);
        }

        /// <summary>
        /// Retrieves an integer value from PlayerPrefs.
        /// If the key doesn't exist, logs an error and returns 0.
        /// </summary>
        /// <param name="key">The PlayerPrefs key to retrieve.</param>
        /// <returns>The integer value associated with the specified key.</returns>
        public static int GetInt(string key)
        {
            if (HasKey(key))
            {
                return PlayerPrefs.GetInt(key);
            }

            Logger.LogError($"GetInt::Invalid key: {key}");
            return 0;
        }

        /// <summary>
        /// Retrieves a string value from PlayerPrefs.
        /// If the key doesn't exist, logs an error and returns null.
        /// </summary>
        /// <param name="key">The PlayerPrefs key to retrieve.</param>
        /// <returns>The string value associated with the specified key.</returns>
        public static string GetString(string key)
        {
            if (HasKey(key))
            {
                return PlayerPrefs.GetString(key);
            }

            Logger.LogError($"GetString::Invalid key: {key}");
            return null;
        }

        /// <summary>
        /// Attempts to set an integer value for a specified PlayerPrefs key.
        /// </summary>
        /// <param name="key">The PlayerPrefs key to set.</param>
        /// <param name="value">The integer value to store.</param>
        /// <returns>True if the key is valid and the value is set successfully; otherwise, false.</returns>
        public static bool TrySetInt(string key, int value)
        {
            if (IsValidKey(key))
            {
                PlayerPrefs.SetInt(key, value);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to set a string value for a specified PlayerPrefs key.
        /// </summary>
        /// <param name="key">The PlayerPrefs key to set.</param>
        /// <param name="value">The string value to store.</param>
        /// <returns>True if the key is valid and the value is set successfully; otherwise, false</returns>
        public static bool TrySetString(string key, string value)
        {
            if (IsValidKey(key))
            {
                PlayerPrefs.SetString(key, value);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to delete a PlayerPrefs key.
        /// </summary>
        /// <param name="key">The PlayerPrefs key to delete.</param>
        /// <returns>True if the key is successfully deleted; otherwise, false.</returns>
        public static bool TryDeleteKey(string key)
        {
            if (PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.DeleteKey(key);
                return true;
            }

            return false;
        }
    }
}