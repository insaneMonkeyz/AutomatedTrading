namespace Tools.Logging
{
    public class Log
    {
        private readonly ZeroLog.Log _tracer;
        private readonly ZeroLog.Log _error;
        private readonly ZeroLog.Log _common;

        internal Log(ZeroLog.Log common, ZeroLog.Log tracer, ZeroLog.Log error)
        {
            _common = common;
            _tracer = tracer;
            _error = error;
        }

        public void Trace(string type, string method)
        {
            _tracer.Trace($"{type}.{method}");
        }
        public void Debug(string? message)
        {
            _common.Debug(message);
        }
        public void Info(string? message)
        {
            _common.Info(message);
        }
        public void Warn(string? message)
        {
            _common.Warn(message);
        }
        public void Error(string type, string method, string? message)
        {
            _error.Error($"{type}.{method}\n  {message}");
        }
        public void Error(string? message, Exception? e)
        {
            _error.Error(message, e);
        }
        public void Fatal(string type, string method, string? message)
        {
            _error.Fatal($"{type}.{method}\n  {message}");
        }
        public void Fatal(string? message, Exception? e)
        {
            _error.Fatal(message, e);
        }
    }
}
