using System.Reflection;

namespace QuikLuaWrapperTests
{
    internal static class Extentions
    {
        public static object? InvokePrivateMethod(this object containingClass, string methodName, params object?[]? args)
        {
            return containingClass
                .GetType()
                .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)?
                .Invoke(containingClass, args);
        }
    }
}
