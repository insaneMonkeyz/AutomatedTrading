using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public static class Extentions
    {
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection)
            {
                action(item);
            }
        }
        public static IEnumerable<T> ToEnumerable<T>(this T? obj)
        {
            if (obj is not null)
            {
                yield return obj;
            }
        }
    }
}
