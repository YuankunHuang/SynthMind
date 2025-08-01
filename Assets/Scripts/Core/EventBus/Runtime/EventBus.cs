using System;
using System.Collections.Generic;

namespace YuankunHuang.SynthMind.Core
{
    public class EventBus
    {
        private static Dictionary<int, List<Delegate>> _listeners = new();
        private static Dictionary<int, Type> _typeMap = new();

        public static void Subscribe<T>(int eventId, Action<T> callback)
        {
            var type = typeof(T);

            if (_typeMap.TryGetValue(eventId, out var existingType))
            {
                if (existingType != type)
                {
                    throw new Exception($"Event Id: {eventId} is bond with {existingType} already.");
                }
            }
            else
            {
                _typeMap[eventId] = type;
            }

            if (!_listeners.ContainsKey(eventId))
            {
                _listeners[eventId] = new List<Delegate>();
            }
            _listeners[eventId].Add(callback);
        }

        public static void Unsubscribe<T>(int eventId, Action<T> listener)
        {
            if (_listeners.TryGetValue(eventId, out var list))
            {
                list.Remove(listener);
            }
        }

        public static void Publish<T>(int eventId, T data)
        {
            if (_listeners.TryGetValue(eventId, out var listeners))
            {
                foreach (var del in listeners.ToArray())
                {
                    if (del is Action<T> action)
                    {
                        action.Invoke(data);
                    }
                }
            }
        }
    }
}