using Quik.Entities;
using Quik.EntityDataProviders.Attributes;
using Quik.EntityDataProviders.QuikApiWrappers;
using Quik.EntityDataProviders.RequestContainers;

namespace Quik.EntityDataProviders
{
    internal delegate void EntityUpdatedHandler<T>(T entity);
    internal delegate void EntityCreatedHandler<T>(T entity);
    internal delegate TEntity? GetChangingEntityHandler<TEntity, TRequestContainer>(TRequestContainer dummy);

    /// <summary>
    /// Fetches business entity from quik
    /// </summary>
    /// <typeparam name="TEntity">Type of the entity to fetch</typeparam>
    /// <typeparam name="TRequestContainer">
    /// Type of container used to request dependent entities, 
    /// such as an Order for OrderExecution
    /// </typeparam>
    internal abstract class DataProvider<TEntity, TRequestContainer> : IDataProvider 
        where TRequestContainer : IRequestContainer<TEntity>, new()
        where TEntity : class
    {
        protected abstract string QuikCallbackMethod { get; }
        protected abstract string AllEntitiesTable { get; }

        protected readonly object _callbackLock = new();
        protected readonly object _userRequestLock = new();

        public EntityCreatedHandler<TEntity> NewEntity = delegate { };

        public void SubscribeCallback(CallbackSubscriber subscribe)
        {
            subscribe(OnNewData, QuikCallbackMethod);
        }
        public virtual List<TEntity> GetAllEntities()
        {
            return QuikProxy.ReadWholeTable(AllEntitiesTable, Create);
        }
        protected abstract TEntity? Create(LuaState state);

        protected virtual int OnNewData(IntPtr state)
        {
            if (Create(state) is TEntity entity)
            {
                NewEntity(entity);
            }

            return 1;
        }
    }
}
