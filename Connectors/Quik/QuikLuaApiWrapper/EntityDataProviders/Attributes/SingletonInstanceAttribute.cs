using System.Reflection;

namespace Quik.EntityProviders.Attributes
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    internal sealed class SingletonInstanceAttribute : Attribute
    {
        public int Rank { get; }
        public SingletonInstanceAttribute(int rank = 0)
        {
            Rank = rank;
        }

        public static T[] GetInstances<T>()
        {
            return Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsAssignableTo(typeof(T)))
                .Select(t => t.GetValueByAttribute<T, SingletonInstanceAttribute>())
                .OfType<(T value, SingletonInstanceAttribute attribute)>()
                .OrderByDescending(kvp => kvp.attribute.Rank)
                .Select(kvp => kvp.value)
                .ToArray();
        }
    }
}
