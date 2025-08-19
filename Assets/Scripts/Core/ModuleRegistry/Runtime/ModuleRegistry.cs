using System;
using System.Collections.Generic;

namespace YuankunHuang.Unity.ModuleCore
{
    public interface IModule
    {
        bool IsInitialized { get; }

        void Dispose();
    }

    public static class ModuleRegistry
    {
        private static Dictionary<Type, object> Instances = new();

        public static void Register<T>(T instance)
        {
            Instances[typeof(T)] = instance;
        }

        public static void Unregister<T>()
        {
            Instances.Remove(typeof(T));
        }

        public static T Get<T>()
        {
            if (Instances.TryGetValue(typeof(T), out var instance))
            {
                return (T)instance;
            }

            throw new Exception($"Module {typeof(T)} not registered.");
        }

        public static void Clear()
        {
            Instances.Clear();
        }
    }
}