using System.Runtime.CompilerServices;
using Quik.EntityProviders.QuikApiWrappers;
using Quik.EntityProviders.RequestContainers;
using Quik.Lua;
using Tools.Logging;

namespace Quik.EntityProviders
{
    internal delegate void EntityEventHandler<T>(T entity);
    internal delegate TEntity? GetChangingEntityHandler<TEntity, TRequestContainer>(TRequestContainer dummy);
    internal delegate bool AllowEntityCreationFilter<TRequestContainer>(ref TRequestContainer request);
     
    /// <summary>
    /// Fetches business entity from quik
    /// </summary>
    /// <typeparam name="TEntity">Type of the entity to fetch</typeparam>
    /// <typeparam name="TRequestContainer">
    /// Type of container used to request dependent entities, 
    /// such as an Order for OrderExecution
    /// </typeparam>
    internal abstract class DataProvider<TEntity, TRequestContainer> : QuikDataConsumer<TEntity>
        where TRequestContainer : struct, IRequestContainer<TEntity>
        where TEntity : class
    {
        protected readonly object _requestInProgressLock = new();
        protected bool _initialized;

        protected abstract string AllEntitiesTable { get; }
        protected abstract Action<LuaWrap> SetWrapper { get; }

        public AllowEntityCreationFilter<TRequestContainer> CreationIsApproved = delegate { return true; };
        public EntityEventHandler<TEntity> NewEntity = delegate { };

        public override void Initialize(ExecutionLoop entityNotificationLoop)
        {
#if TRACE
            this.Trace();
#endif
            SetWrapper(Quik.Lua);
            base.Initialize(entityNotificationLoop);
            _initialized = true;
        }

        public virtual List<TEntity> GetAllEntities()
        {
#if TRACE
            this.Trace();
#endif
#if DEBUG
            EnsureInitialized();
#endif
            lock (_requestInProgressLock)
            {
                return TableWrapper.ReadWholeTable(AllEntitiesTable, Create); 
            }
        }
        public virtual TEntity? Create(ref TRequestContainer request)
        {
#if TRACE
            this.Trace();
#endif
#if DEBUG
            EnsureInitialized();
#endif

            if (!request.HasData)
            {
                _log.Error($"{request.GetType().Name} request is missing essential parameters");
            }

            return null;
        }
        protected abstract TEntity? Create(LuaWrap state);
        protected abstract TRequestContainer CreateRequestFrom(LuaWrap state);

        protected override int OnNewData(IntPtr state)
        {
            try
            {
#if TRACE
                this.Trace();
#endif
                lock (_callbackLock)
                {
                    var request = CreateRequestFrom(state);

                    if (CreationIsApproved(ref request) && Create(state) is TEntity entity)
                    {
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

        protected void EnsureInitialized([CallerMemberName] string? method = "METHOD_NOT_PROVIDED")
        {
            if (!_initialized)
            {
                throw new InvalidOperationException($"Calling {method} from {this.GetType().Name} before it was initialized.");
            }
        }

        #region IDisposable
        protected override void DisposeManaged()
        {
            NewEntity = default;
            CreationIsApproved = default;
        }
        protected override void DisposeUnmanaged() { }
        #endregion
    }
}
