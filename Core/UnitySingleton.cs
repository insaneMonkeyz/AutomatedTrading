using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public static class UnitySingleton
    {
        private static Dictionary<Type, object> _instances = new();

        public static T Resolve<T>()
        {
            var type = typeof(T);

            if(!type.IsInterface)
            {
                throw new ArgumentException($"{type.Name} is not an interface!");
            }

            return _instances.TryGetValue(type, out object result)
                ? (T)result
                : default;
        }
        public static void Register<T>(object instance)
        {
            var type = typeof(T);

            if (instance == null)
            {
                throw new ArgumentNullException($"{nameof(instance)} is null!");
            }
            else if (!type.IsInterface)
            {
                throw new ArgumentException($"{type.Name} is not an interface!");
            }
            else if (!instance.GetType().IsAssignableFrom(type))
            {
                throw new ArgumentException($"{nameof(instance)} does not derive from {type.Name}");
            }

            _instances[type] = instance;
        }
    }
}
