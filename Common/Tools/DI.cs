namespace Tools
{
    /// <summary>
    /// Dependency injections provider
    /// </summary>
    public static class DI
    {
        private static readonly Dictionary<Type, object> _map = new();

        public static T Resolve<T>() where T : class
        {
            if (!_map.TryGetValue(typeof(T), out object? instance))
            {
                throw new Exception($"Instance of {typeof(T)} is not registered");
            }

            return (T)instance;
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
