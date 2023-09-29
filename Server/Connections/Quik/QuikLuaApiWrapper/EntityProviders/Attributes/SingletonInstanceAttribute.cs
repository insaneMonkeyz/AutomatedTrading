using System.Diagnostics;
using System.Reflection;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

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
            try
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
            catch (ReflectionTypeLoadException ex)
            {
                var sb = new StringBuilder();
                sb.AppendLine("==========================================");

                foreach (var exSub in ex.LoaderExceptions.OfType<Exception>())
                {
                    sb.AppendLine(exSub.Message);

                    if (exSub is FileNotFoundException e)
                    {
                        if (!string.IsNullOrEmpty(e.FusionLog))
                        {
                            sb.AppendLine("Fusion Log:");
                            sb.AppendLine(e.FusionLog);
                        }
                    }
                    sb.AppendLine();
                }
                sb.AppendLine("==========================================");

                Debug.Print(sb.ToString());
                
                throw;
            }
        }
    }
}
