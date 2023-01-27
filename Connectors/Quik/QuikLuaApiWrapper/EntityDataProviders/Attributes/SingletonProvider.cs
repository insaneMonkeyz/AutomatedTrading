using System.Reflection;

namespace Quik.EntityDataProviders.Attributes
{
    public static class SingletonProvider
    {
        public static IEnumerable<T> GetInstances<T>()
        {
            return Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsAssignableTo(typeof(T)))
                .Select(t => t.GetTaggedProperty<SingletonInstanceAttribute>())
                .OfType<PropertyInfo>()
                .Select(info => info.GetValue(null, null))
                .OfType<T>();
        }
    }
}
