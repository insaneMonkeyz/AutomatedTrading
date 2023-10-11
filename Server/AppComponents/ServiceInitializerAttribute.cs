using System.Reflection;
using Tools;

namespace AppComponents
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ServiceInitializerAttribute : Attribute
    {
        public static void Initialize<T>(object? configuration) where T : class
        {
            var type = typeof(T);

            static MethodInfo? getInitializer(Type t)
            {
                return t.GetMethods().FirstOrDefault(m => m.GetCustomAttribute<ServiceInitializerAttribute>() is not null);
            }

            try
            {
                var initializer =
                    type.Assembly
                        .GetTypes()
                        .Where(t => t.IsAssignableTo(type))
                        .Select(getInitializer)
                        .FirstOrDefault(i => i != null) ?? throw new Exception("Initializer method was not found");

                var serviceInstance = Activator.CreateInstance(initializer.DeclaringType, nonPublic: true);

                initializer.Invoke(serviceInstance, configuration.ToArray());
            }
            catch (Exception e)
            {
                throw new Exception("Cannot initialize the service via reflection", e);
            }
        }
    }
}
