using Quik.Entities;
using Quik.EntityDataProviders.Attributes;
using Quik.EntityDataProviders.RequestContainers;

namespace Quik.EntityDataProviders
{
    internal delegate void EntityStateChangedHandler<T>(T entity);
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

        protected readonly object _callbackLock = new();
        protected readonly object _userRequestLock = new();
        protected readonly TRequestContainer _resolveEntityRequest = new();
        protected readonly EntityResolver<TRequestContainer, TEntity> _entityResolver;

        public EntityStateChangedHandler<TEntity> EntityChanged = delegate { };

        public DataProvider(EntityResolver<TRequestContainer, TEntity> resolver)
        {
            _entityResolver = resolver;
        }

        public void SubscribeCallback(CallbackSubscriber subscribe)
        {
            subscribe(OnEntityChanged, QuikCallbackMethod);
        }
        public abstract void Update(TEntity entity);

        protected abstract void Update(TEntity entity, LuaState state);
        protected abstract void BuildEntityResolveRequest(LuaState state);
        protected abstract TEntity? Create(LuaState state);

        private int OnEntityChanged(IntPtr state)
        {
            lock (_callbackLock)
            {
                BuildEntityResolveRequest(state);
                // ваще то этот коллбек может вызываться и при появлении новых сущностей.
                // разберись с этим
                if (_entityResolver.GetEntity(_resolveEntityRequest) is TEntity entity)
                {
                    Update(entity, state);

                    EntityChanged(entity);
                } 
            }

            return 1;
        }
    }
}
