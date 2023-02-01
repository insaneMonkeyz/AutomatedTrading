using Quik.Entities;
using Quik.EntityProviders.RequestContainers;

namespace Quik.EntityProviders
{
    internal abstract class UpdatableEntitiesProvider<TEntity, TRequestContainer>
        : DataProvider<TEntity, TRequestContainer>
            where TRequestContainer : IRequestContainer<TEntity>, new()
            where TEntity : class
    {
        protected readonly EntityResolver<TRequestContainer, TEntity> _entityResolver;

        public EntityEventHandler<TEntity> EntityChanged = delegate { };

        public abstract void Update(TEntity entity);
        protected abstract void Update(TEntity entity, LuaState state);
        protected abstract TRequestContainer CreateRequestFrom(LuaState state);

        protected override int OnNewData(IntPtr state)
        {
            // TODO: warning! this will definetely include dependencies resolving.
            // must find a way to do it asynchronously
            
            lock (_callbackLock)
            {
                var entity = _entityResolver.GetEntity(CreateRequestFrom(state));

                if (entity != null)
                {
                    Update(entity, state);

                    _eventSignalizer.QueueEntity<EntityEventHandler<TEntity>>(EntityChanged, entity);

                    return 1;
                }
                else
                {
                    return base.OnNewData(state);
                }
            }
        }

        public UpdatableEntitiesProvider()
        {
            _entityResolver = EntityResolvers.GetResolver<TRequestContainer, TEntity>();
        } 
    }
}
