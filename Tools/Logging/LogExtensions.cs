using System.Runtime.CompilerServices;

namespace Tools.Logging
{
    public static class LogExtensions
    {
        private static readonly Log _log = LogManagement.GetLogger(nameof(LogExtensions));

        public static void Trace(this object obj, [CallerMemberName] string methodName = "UNRESOLVED_METHOD")
        {
            _log.Trace(obj.GetType().Name, methodName);
        }
        public static void Error(this object obj, string? message, [CallerMemberName] string methodName = "UNRESOLVED_METHOD")
        {
            _log.Error(obj.GetType().Name, methodName, message);
        }
        public static void Fatal(this object obj, string? message, [CallerMemberName] string methodName = "UNRESOLVED_METHOD")
        {
            _log.Fatal(obj.GetType().Name, methodName, message);
        }
    }
}
