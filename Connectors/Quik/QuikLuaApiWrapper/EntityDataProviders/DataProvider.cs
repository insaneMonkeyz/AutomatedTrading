using Quik.Entities;
using Quik.EntityProviders.Attributes;
using Quik.EntityProviders.QuikApiWrappers;
using Quik.EntityProviders.RequestContainers;

namespace Quik.EntityProviders
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
    internal abstract class DataProvider<TEntity, TRequestContainer> : IQuikDataSubscriber 
        where TRequestContainer : IRequestContainer<TEntity>, new()
        where TEntity : class
    {
        protected abstract string QuikCallbackMethod { get; }
        protected abstract string AllEntitiesTable { get; }

        protected readonly object _callbackLock = new();
        protected readonly object _userRequestLock = new();

        public EntityCreatedHandler<TEntity> NewEntity = delegate { };

        public void SubscribeCallback(LuaState state)
        {
            state.RegisterCallback(OnNewData, QuikCallbackMethod);
        }
        public virtual List<TEntity> GetAllEntities()
        {
            return QuikProxy.ReadWholeTable(AllEntitiesTable, Create);
        }
        public    abstract TEntity? Create(TRequestContainer request);
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
