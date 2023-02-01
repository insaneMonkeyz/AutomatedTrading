using Quik.Entities;
using Quik.EntityProviders.Attributes;
using Quik.EntityProviders.QuikApiWrappers;
using Quik.EntityProviders.RequestContainers;

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
    internal abstract class DataProvider<TEntity, TRequestContainer> : IQuikDataSubscriber, IDisposable
        where TRequestContainer : IRequestContainer<TEntity>, new()
        where TEntity : class
    {
        protected abstract string QuikCallbackMethod { get; }
        protected abstract string AllEntitiesTable { get; }

        protected readonly object _callbackLock = new();
        protected readonly object _requestInProgressLock = new();

        private bool _disposed;

        protected readonly EventSignalizer<TEntity> _eventSignalizer = new();

        public EntityEventHandler<TEntity> NewEntity = delegate { };

        public void SubscribeCallback(LuaState state)
        {
            _eventSignalizer.Start();
            state.RegisterCallback(OnNewData, QuikCallbackMethod);
        }
        public virtual List<TEntity> GetAllEntities()
        {
            lock (_requestInProgressLock)
            {
                return QuikProxy.ReadWholeTable(AllEntitiesTable, Create); 
            }
        }
        public    abstract TEntity? Create(TRequestContainer request);
        protected abstract TEntity? Create(LuaState state);

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
