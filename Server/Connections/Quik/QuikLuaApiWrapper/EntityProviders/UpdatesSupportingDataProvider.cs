﻿using System.Diagnostics;
using TradingConcepts;
using Quik.Entities;
using Quik.EntityProviders.RequestContainers;
using Quik.Lua;
using Quik.EntityProviders.Resolvers;
using Quik.EntityProviders.Notification;

namespace Quik.EntityProviders
{
    internal abstract class UpdatableEntitiesProvider<TEntity, TRequestContainer>
        : DataProvider<TEntity, TRequestContainer>
            where TRequestContainer : struct, IRequestContainer<TEntity>
            where TEntity : class, INotifiableEntity
    {
        protected EntityResolver<TRequestContainer, TEntity>? _entityResolver;

        public EntityEventHandler<TEntity> EntityChanged = delegate { };

        public override void Initialize(ExecutionLoop entityNotificationLoop)
        {
#if TRACE
            this.Trace();
#endif
            _entityResolver = EntityResolvers.GetResolver<TRequestContainer, TEntity>();
            base.Initialize(entityNotificationLoop);
        }
        public virtual void Update(TEntity entity)
        {
#if TRACE
            this.Trace();
#endif
#if DEBUG
            EnsureInitialized(); 
#endif
        }
        protected abstract void Update(TEntity entity, LuaWrap state);

        protected override int OnNewData(IntPtr state)
        {
#if TRACE
            this.Trace();
#endif
            try
            {
                lock (_callbackLock)
                {
                    var request = CreateRequestFrom(state);
#if DEBUG && !UNITTESTING
                    _log.Debug(Helper.PrintQuikParameters(WrapperType));
#endif
                    var entity = _entityResolver.GetFromCache(ref request);

                    if (entity != null)
                    {
                        Update(entity, state);
                        _eventSignalizer.QueueEntity(EntityChanged, entity);

                        return 1;
                    }

                    if (CreationIsApproved(ref request) && (entity = Create(state)) != null)
                    {
                        _entityResolver.CacheEntity(ref request, entity);
                        _eventSignalizer.QueueEntity(NewEntity, entity);
                    }

                    return 1;
                }
            }
            catch (Exception e)
            {
                _log.Error(CALLBACK_EXCEPTION_MSG, e);
                return -1;
            }
        }

        protected override void DisposeManaged()
        {
            base.DisposeManaged();
            EntityChanged = delegate { };
        }
    }
}
