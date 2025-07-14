using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace YuankunHuang.Unity.Core
{
    /// @ingroup Core
    /// @class Logger
    /// @brief Provides static methods for logging messages, warnings, errors, and exceptions with additional context (frame count and time).
    /// 
    /// The `Logger` class is a utility class designed to simplify logging within the game. It provides static methods for logging
    /// messages, warnings, errors, and exceptions to the Unity console. Each log entry includes additional context, such as the 
    /// current frame count and time, to help with debugging.
    public static class Logger
    {
        /// <summary>
        /// Logs a message to the Unity console.
        /// Includes the frame count and the current time for context.
        /// </summary>
        /// <param name="msg">The message to log.</param>
        public static void Log(string msg)
        {
            Debug.Log($"{msg}\nFrame: {Time.frameCount}\nTime: {Time.time}");
        }

        /// <summary>
        /// Logs a warning message to the Unity console.
        /// Includes the frame count and the current time for context.
        /// </summary>
        /// <param name="msg">The warning message to log.</param>
        public static void LogWarning(string msg)
        {
            Debug.LogWarning($"{msg}\nFrame: {Time.frameCount}\nTime: {Time.time}");
        }

        /// <summary>
        /// Logs an error message to the Unity console.
        /// Includes the frame count and the current time for context.
        /// </summary>
        /// <param name="msg">The error message to log.</param>
        public static void LogError(string msg)
        {
            Debug.LogError($"{msg}\nFrame: {Time.frameCount}\nTime: {Time.time}");
        }

        /// <summary>
        /// Logs an exception to the Unity console.
        /// Includes the exception message and stack trace for detailed debugging.
        /// </summary>
        /// <param name="e">The exception to log.</param>
        public static void LogException(Exception e)
        {
            if (e != null)
            {
                Debug.LogError($"Exception::{e.Message}.\nStackTrace: {e.StackTrace}");
            }
        }
    }
}