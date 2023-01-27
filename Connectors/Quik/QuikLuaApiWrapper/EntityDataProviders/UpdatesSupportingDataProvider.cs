using Quik.EntityDataProviders.RequestContainers;

namespace Quik.EntityDataProviders
{
    internal abstract class UpdatesSupportingDataProvider<TEntity, TRequestContainer>
        : DataProvider<TEntity, TRequestContainer>
            where TRequestContainer : IRequestContainer<TEntity>, new()
            where TEntity : class
    {
        protected readonly TRequestContainer _resolveEntityRequest = new();
        protected readonly EntityResolver<TRequestContainer, TEntity> _entityResolver;

        public EntityUpdatedHandler<TEntity> EntityChanged = delegate { };

        public abstract void Update(TEntity entity);

        protected abstract void Update(TEntity entity, LuaState state);
        protected abstract void BuildEntityResolveRequest(LuaState state);
        protected override int OnNewData(IntPtr state)
        {
            lock (_callbackLock)
            {
                BuildEntityResolveRequest(state);

                var entity = _entityResolver.GetEntity(_resolveEntityRequest);

                if (entity != null)
                {
                    Update(entity, state);

                    EntityChanged(entity);

                    return 1;
                }

                entity = Create(state);

                if (entity != null)
                {
                    NewEntity(entity);
                }

                return 1;
            }
        }

        public UpdatesSupportingDataProvider(EntityResolver<TRequestContainer, TEntity> resolver)
        {
            _entityResolver = resolver;
        } 
    }
}
