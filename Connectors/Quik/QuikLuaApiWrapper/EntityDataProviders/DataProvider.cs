using System.Diagnostics;
using Quik.EntityProviders.QuikApiWrappers;
using Quik.EntityProviders.RequestContainers;
using Quik.Lua;

namespace Quik.EntityProviders
{
    internal delegate void EntityEventHandler<T>(T entity);
    internal delegate TEntity? GetChangingEntityHandler<TEntity, TRequestContainer>(TRequestContainer dummy);
    internal delegate bool AllowEntityCreationFilter<TRequestContainer>(ref TRequestContainer request);
     
    /// <summary>
    /// Fetches business entity from quik
    /// </summary>
    /// <typeparam name="TEntity">Type of the entity to fetch</typeparam>
    /// <typeparam name="TRequestContainer">
    /// Type of container used to request dependent entities, 
    /// such as an Order for OrderExecution
    /// </typeparam>
    internal abstract class DataProvider<TEntity, TRequestContainer> : IDisposable
        where TRequestContainer : struct, IRequestContainer<TEntity>
        where TEntity : class
    {
        protected IEntityEventSignalizer<TEntity> _eventSignalizer = new DirectEntitySignalizer<TEntity>();
        protected readonly object _requestInProgressLock = new();
        protected readonly object _callbackLock = new();
        private bool _disposed;

        protected abstract string QuikCallbackMethod { get; }
        protected abstract string AllEntitiesTable { get; }
        protected abstract Action<LuaWrap> SetWrapper { get; }

        public AllowEntityCreationFilter<TRequestContainer> CreationIsApproved = delegate { return true; };
        public EntityEventHandler<TEntity> NewEntity = delegate { };

        public void SubscribeCallback()
        {
            Quik.Lua.RegisterCallback(OnNewData, QuikCallbackMethod);
        }
        public virtual void Initialize(ExecutionLoop entityNotificationLoop)
        {
            SetWrapper(Quik.Lua);
            _eventSignalizer = new EventSignalizer<TEntity>(entityNotificationLoop)
            {
                IsEnabled = true
            };
        }

        public virtual List<TEntity> GetAllEntities()
        {
            lock (_requestInProgressLock)
            {
                return TableWrapper.ReadWholeTable(AllEntitiesTable, Create); 
            }
        }
        public    abstract TEntity? Create(ref TRequestContainer request);
        protected abstract TEntity? Create(LuaWrap state);
        protected abstract TRequestContainer CreateRequestFrom(LuaWrap state);

        protected virtual int OnNewData(IntPtr state)
        {
            try
            {
                Debug.Print($"*** {this.GetType().Name}.OnNewData");

                lock (_callbackLock)
                {
                    var request = CreateRequestFrom(state);

                    if (CreationIsApproved(ref request) && Create(state) is TEntity entity)
                    {
                        _eventSignalizer.QueueEntity(NewEntity, entity);
                    }

                    return 1;
                }
            }
            catch (Exception e)
            {
                $"{e.Message}\n{e.StackTrace ?? "NO_STACKTRACE_PROVIDED"}".DebugPrintWarning();
                return -1;
            }
        }

        #region IDisposable
        protected virtual void DisposeInternal() { }
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    DisposeInternal();
                    _eventSignalizer.Dispose();
                    NewEntity = default;
                    CreationIsApproved = default;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposed = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~DataProvider()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        } 
        #endregion
    }
}
