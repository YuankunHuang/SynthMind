using System;
using System.Reflection;
using System.Linq;

namespace YuankunHuang.Unity.GameDataConfig
{
    /// <summary>
    /// Centralized Game Data Manager
    /// Automatically initializes all Config classes via reflection
    /// </summary>
    public static class GameDataManager
    {
        private static bool _isInitialized = false;

        /// <summary>
        /// Initialize all Config classes (auto-discovered via reflection)
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized) return;

            var configTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsClass && t.IsPublic && t.Name.EndsWith("Config") && t.BaseType != null && t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition().Name.StartsWith("BaseConfigData"));

            foreach (var type in configTypes)
            {
                var method = type.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Static, null, Type.EmptyTypes, null);
                if (method != null)
                {
                    method.Invoke(null, null);
                }
            }

            _isInitialized = true;
        }
    }
}
