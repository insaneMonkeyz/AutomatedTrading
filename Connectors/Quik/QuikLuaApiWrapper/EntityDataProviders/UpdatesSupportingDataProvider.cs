using Quik.Entities;
using Quik.EntityProviders.RequestContainers;

namespace Quik.EntityProviders
{
    internal abstract class UpdatableEntitiesProvider<TEntity, TRequestContainer>
        : DataProvider<TEntity, TRequestContainer>
            where TRequestContainer : IRequestContainer<TEntity>, new()
            where TEntity : class
    {
        protected EntityResolver<TRequestContainer, TEntity> _entityResolver;

        public EntityEventHandler<TEntity> EntityChanged = delegate { };

        public override void Initialize()
        {
            _entityResolver = EntityResolvers.GetResolver<TRequestContainer, TEntity>();
            base.Initialize();
        }
        public abstract void Update(TEntity entity);
        protected abstract void Update(TEntity entity, LuaState state);
        protected abstract TRequestContainer CreateRequestFrom(LuaState state);

        protected override int OnNewData(IntPtr state)
        {
            // TODO: warning! this will definetely include dependencies resolving.
            // must find a way to do it asynchronously in order not to block quik's main thread
            
            lock (_callbackLock)
            {
                var request = CreateRequestFrom(state);
                var entity = _entityResolver.GetFromCache(request);

                if (entity != null)
                {
                    Update(entity, state);

                    _eventSignalizer.QueueEntity<EntityEventHandler<TEntity>>(EntityChanged, entity);

                    return 1;
                }

                entity = Create(state);

                if (entity != null)
                {
                    _entityResolver.CacheEntity(request, entity);
                    _eventSignalizer.QueueEntity<EntityEventHandler<TEntity>>(NewEntity, entity);
                }

                return 1;
            }
        }
    }
}
