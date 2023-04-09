using ZeroLog;
using ZeroLog.Appenders;
using ZeroLog.Configuration;
using ZeroLog.Formatting;

namespace Tools.Logging
{
    public class LogManagement
    {
        public const string LOGS_FOLDER   = "Logs";
        public const string MAIN_LOG_FILE = "FullLog.log";
        public const int    DEFAULT_LOG_BUFFER_SIZE = 1024;

        public static readonly LogLevel DefaultLevel = LogLevel.Debug;

        private const string DEFAULT_LOG_FORMAT = "\n[%date  %time]\t[%level]\t%thread\t%loggerCompact\n  ";
        private const string   ERROR_LOG_FORMAT = "\n[%date  %time]\t[%level]\t%thread\n  ";
        private const string   TRACE_LOG_FORMAT = "[%date  %time]\t[%level]\t%thread\t";

        private readonly Dictionary<string, TextWriter> _textWriters = new();
        private static readonly object _instanceLock = new();
        private static LogManagement? _instance;

        public static void Dispose()
        {
            lock (_instanceLock)
            {
                _instance?._textWriters.ForEach(kvp => kvp.Value.Close());
                LogManager.Shutdown();
            }
        }
        public static Log GetLogger<T>()
        {
            return WrapLogger(() => LogManager.GetLogger<T>());
        }

        public static Log GetLogger(Type type)
        {
            return WrapLogger(() => LogManager.GetLogger(type));
        }

        public static Log GetLogger(string name)
        {
            return WrapLogger(() => LogManager.GetLogger(name));
        }

        public static Log GetTraceDedicatedFileLogger<T>()
        {
            var logger = GetDedicatedFileLogger(typeof(T).Name, LogLevel.Trace);

            return WrapLogger(() => logger);
        }
        public static Log GetDebugDedicatedFileLogger<T>()
        {
            var logger = GetDedicatedFileLogger(typeof(T).Name, LogLevel.Debug);

            return WrapLogger(() => logger);
        }
        public static Log GetInfoDedicatedFileLogger<T>()
        {
            var logger = GetDedicatedFileLogger(typeof(T).Name, LogLevel.Info);

            return WrapLogger(() => logger);
        }
        public static Log GetWarnDedicatedFileLogger<T>()
        {
            var logger = GetDedicatedFileLogger(typeof(T).Name, LogLevel.Warn);

            return WrapLogger(() => logger);
        }
        public static Log GetErrorDedicatedFileLogger<T>()
        {
            var logger = GetDedicatedFileLogger(typeof(T).Name, LogLevel.Error);

            return WrapLogger(() => logger);
        }
        public static Log GetFatalDedicatedFileLogger<T>()
        {
            var logger = GetDedicatedFileLogger(typeof(T).Name, LogLevel.Fatal);

            return WrapLogger(() => logger);
        }
        internal static ZeroLog.Log GetDedicatedFileLogger(string name, LogLevel thresholdLevel)
        {
            lock (_instanceLock)
            {
                EnsureInitialization();

                var logConfiguration = LogManager.Configuration;

                if (!logConfiguration.Loggers.Any(l => l.Name == name))
                {
                    var dedicatedConfig = _instance.CreateLoggerConfiguration(thresholdLevel, name);
                    logConfiguration.Loggers.Add(dedicatedConfig);
                    logConfiguration.ApplyChanges();
                }

                return LogManager.GetLogger(name);
            }
        }
        private static Log WrapLogger(Func<ZeroLog.Log> getFullLogger)
        {
            EnsureInitialization();
            var tracer = LogManagement.GetDedicatedFileLogger("Tracer", ZeroLog.LogLevel.Trace);
            var error = LogManagement.GetDedicatedFileLogger("Errors", ZeroLog.LogLevel.Error);
            return new Log(getFullLogger(), tracer, error);
        }

        private LoggerConfiguration CreateLoggerConfiguration(LogLevel thresholdLevel, string logName)
        {
            return new LoggerConfiguration(logName)
            {
                Level = thresholdLevel,
                LogMessagePoolExhaustionStrategy = LogMessagePoolExhaustionStrategy.Allocate,
                Appenders =
                {
                    new TextWriterAppender(GetTextWriter($"{logName}.log"))
                    {
                        Formatter = new DefaultFormatter
                        {
                            PrefixPattern = thresholdLevel switch
                            {
                                LogLevel.Trace => TRACE_LOG_FORMAT,
                                LogLevel.Error => ERROR_LOG_FORMAT,
                                LogLevel.Fatal => ERROR_LOG_FORMAT,
                                             _ => DEFAULT_LOG_FORMAT
                            }
                        }
                    },
                }
            };
        }
        private ZeroLogConfiguration CreateConfiguration()
        {
            return new ZeroLogConfiguration
            {
                RootLogger =
                {
                    LogMessagePoolExhaustionStrategy = LogMessagePoolExhaustionStrategy.Allocate,
                    Level = DefaultLevel,
                    Appenders =
                    {
                        new TextWriterAppender(GetTextWriter(MAIN_LOG_FILE))
                        {
                            Formatter = new DefaultFormatter
                            {
                                PrefixPattern = DEFAULT_LOG_FORMAT
                            }
                        },
                    }
                },
                AppendingStrategy = AppendingStrategy.Synchronous,
                AutoRegisterEnums = true,
                LogMessageBufferSize = DEFAULT_LOG_BUFFER_SIZE,
            };
        }
        private TextWriter GetTextWriter(string file)
        {
            if (_textWriters.TryGetValue(file, out TextWriter writer))
            {
                return writer;
            }

            writer = new StreamWriter(GetPathToLogFile(file), append: true);

            _textWriters[file] = writer;

            return writer;
        }

        private static string GetPathToLogFile(string file)
        {
            return Path.Combine(GetLogsDirectory(), file);
        }
        private static string GetLogsDirectory()
        {
            var current = Directory.GetCurrentDirectory();
            return Path.Combine(current, LOGS_FOLDER);
        }
        private static void EnsureFolderExists()
        {
            var dir = GetLogsDirectory();

            if(!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
        private static void EnsureInitialization()
        {
            lock (_instanceLock)
            {
                if (_instance == null)
                {
                    _instance = new LogManagement();

                    if (LogManager.Configuration is null)
                    {
                        var configuration = _instance.CreateConfiguration();
                        LogManager.Initialize(configuration);
                    }

                    EnsureFolderExists();
                }
            }
        }
    }
}
