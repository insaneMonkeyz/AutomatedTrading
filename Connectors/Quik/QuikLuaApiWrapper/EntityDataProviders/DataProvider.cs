using BasicConcepts;
using Quik.Entities;
using Quik.EntityProviders.QuikApiWrappers;
using Quik.EntityProviders.Attributes;
using Quik.EntityProviders.QuikApiWrappers;
using Quik.EntityProviders.RequestContainers;
using Quik.Lua;

using static Quik.Quik;

namespace Quik.EntityProviders
{
    internal delegate void EntityEventHandler<T>(T entity);
    internal delegate TEntity? GetChangingEntityHandler<TEntity, TRequestContainer>(TRequestContainer dummy);
     
    /// <summary>
    /// Fetches business entity from quik
    /// </summary>
    /// <typeparam name="TEntity">Type of the entity to fetch</typeparam>
    /// <typeparam name="TRequestContainer">
    /// Type of container used to request dependent entities, 
    /// such as an Order for OrderExecution
    /// </typeparam>
    internal abstract class DataProvider<TEntity, TRequestContainer> : IDisposable
        where TRequestContainer : IRequestContainer<TEntity>, new()
        where TEntity : class
    {
        protected abstract string QuikCallbackMethod { get; }
        protected abstract string AllEntitiesTable { get; }
        protected abstract Action<LuaWrap> SetWrapper { get; }

        protected readonly object _callbackLock = new();
        protected readonly object _requestInProgressLock = new();

        private bool _disposed;

        protected readonly EventSignalizer<TEntity> _eventSignalizer = new();

        public EntityEventHandler<TEntity> NewEntity = delegate { };

        public void SubscribeCallback()
        {
            Quik.Lua.RegisterCallback(OnNewData, QuikCallbackMethod);
        }
        public virtual void Initialize()
        {
            SetWrapper(Quik.Lua);
            _eventSignalizer.Start();
        }

        public virtual List<TEntity> GetAllEntities()
        {
            lock (_requestInProgressLock)
            {
                return TableWrapper.ReadWholeTable(AllEntitiesTable, Create); 
            }
        }
        public    abstract TEntity? Create(TRequestContainer request);
        protected abstract TEntity? Create(LuaWrap state);

        protected virtual int OnNewData(IntPtr state)
        {
            if (Create(state) is TEntity entity)
            {
                _eventSignalizer.QueueEntity<EntityEventHandler<TEntity>>(NewEntity, entity);
            }

            return 1;
        }

        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _eventSignalizer.Stop();
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
