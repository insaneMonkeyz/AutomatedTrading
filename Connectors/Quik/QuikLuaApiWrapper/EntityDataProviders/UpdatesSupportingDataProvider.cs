using BasicConcepts;
using Quik.Entities;
using Quik.EntityProviders.RequestContainers;
using Quik.Lua;

namespace Quik.EntityProviders
{
    internal abstract class UpdatableEntitiesProvider<TEntity, TRequestContainer>
        : DataProvider<TEntity, TRequestContainer>
            where TRequestContainer : struct, IRequestContainer<TEntity>
            where TEntity : class, INotifyEntityUpdated
    {
        protected EntityResolver<TRequestContainer, TEntity>? _entityResolver;

        public EntityEventHandler<TEntity> EntityChanged = delegate { };

        public override void Initialize()
        {
            _entityResolver = EntityResolvers.GetResolver<TRequestContainer, TEntity>();
            base.Initialize();
        }
        public abstract void Update(TEntity entity);
        protected abstract void Update(TEntity entity, LuaWrap state);

        protected override int OnNewData(IntPtr state)
        {
            // TODO: warning! this will definetely include dependencies resolving.
            // must find a way to do it asynchronously in order not to block quik's main thread
            
            lock (_callbackLock)
            {
                var request = CreateRequestFrom(state);
                var entity = _entityResolver.GetFromCache(ref request);

                if (entity != null)
                {
                    Update(entity, state);

                    _eventSignalizer.QueueEntity<EntityEventHandler<TEntity>>(EntityChanged, entity);

                    return 1;
                }

                if (CreationIsApproved(ref request) && (entity = Create(state)) != null)
                {
                    _entityResolver.CacheEntity(ref request, entity);
                    _eventSignalizer.QueueEntity<EntityEventHandler<TEntity>>(NewEntity, entity);
                }

                return 1;
            }
        }

        protected override void DisposeInternal()
        {
            EntityChanged = delegate { };
        }
    }
}
