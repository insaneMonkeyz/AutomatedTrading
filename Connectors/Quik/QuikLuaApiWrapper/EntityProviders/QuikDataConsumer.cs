using Quik.EntityProviders.Notification;
using Quik.Lua;

namespace Quik.EntityProviders
{
    abstract class QuikDataConsumer<TEntity> : IQuikDataConsumer where TEntity : class
    {
        protected abstract string QuikCallbackMethod { get; }

        protected readonly Log _log;
        protected readonly object _callbackLock = new();
        protected IEntityEventSignalizer<TEntity> _eventSignalizer = new DirectEntitySignalizer<TEntity>();
        protected LuaFunction _onNewDataCallback;
        private bool _disposed;

        protected const string CALLBACK_EXCEPTION_MSG = "";

        public QuikDataConsumer()
        {
            _log = LogManagement.GetLogger(this.GetType().Name);
            _onNewDataCallback = OnNewData;
        }

        public virtual void Initialize(ExecutionLoop internalRoutinesThread)
        {
#if TRACE
            this.Trace();
#endif
            _eventSignalizer = new EventSignalizer<TEntity>(internalRoutinesThread)
            {
                IsEnabled = true
            };
        }

        public void SubscribeCallback()
        {
#if TRACE
            this.Trace();
#endif
            Quik.Lua.RegisterCallback(_onNewDataCallback, QuikCallbackMethod);
        }
        public void UnsubscribeCallback()
        {
#if TRACE
            this.Trace();
#endif
            Quik.Lua.UnregisterCallback(QuikCallbackMethod);
        }

        protected abstract int OnNewData(IntPtr state);

        protected virtual void DisposeManaged() { }
        protected virtual void DisposeUnmanaged() { }
        protected virtual void Dispose(bool disposing)
        {
#if TRACE
            this.Trace();
#endif
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventSignalizer.Dispose();
                DisposeManaged();
            }

            UnsubscribeCallback();
            DisposeUnmanaged();
            _onNewDataCallback = null;
            _disposed = true;
        }
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        ~QuikDataConsumer()
        {
            Dispose(disposing: false);
        }
    }
}
