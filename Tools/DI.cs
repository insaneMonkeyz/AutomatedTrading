﻿namespace Core
{
    /// <summary>
    /// Dependency injections provider
    /// </summary>
    public static class DI
    {
        private static readonly Dictionary<Type, object> _map = new();

        public static T? Resolve<T>() where T : class
        {
            return _map.TryGetValue(typeof(T), out object? instance)
                ? (T)instance
                : default;
        }

        public static void RegisterInstance<T>(T instance)
        {
            if (instance is null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            _map[typeof(T)] = instance;
        }

        public static bool IsRegistered<T>()
        {
            return _map.ContainsKey(typeof(T));

        }

        public static void Unregister<T>()
        {
            _map.Remove(typeof(T));
        }

    }
}
