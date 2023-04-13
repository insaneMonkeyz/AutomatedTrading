using System.Runtime.CompilerServices;
using Quik.Entities;
using Quik.EntityProviders.Attributes;
using Quik.EntityProviders.QuikApiWrappers;
using Quik.EntityProviders.RequestContainers;
using Quik.EntityProviders.Resolvers;
using Tools.Logging;
using CallbackParameters = Quik.EntityProviders.QuikApiWrappers.FunctionsWrappers.ReadCallbackArgs<string?, string?, Quik.EntityProviders.RequestContainers.OrderbookRequestContainer>;

namespace Quik.EntityProviders
{
    internal sealed class OrderbooksProvider : QuikDataConsumer<OrderBook>
    {
        public AllowEntityCreationFilter<OrderbookRequestContainer> CreationIsApproved = delegate { return true; };
        public EntityEventHandler<OrderBook> EntityChanged = delegate { };
        public EntityEventHandler<OrderBook> NewEntity = delegate { };
        protected override string QuikCallbackMethod => OrderbookWrapper.CALLBACK_METHOD;

        private CallbackParameters _requestContainerCreationArgs = new()
        {
            Callback = OrderbookRequestContainer.Create
        };
        private EntityResolver<OrderbookRequestContainer, OrderBook> _bookResolver 
                  = NoResolver<OrderbookRequestContainer, OrderBook>.Instance;
        private EntityResolver<SecurityRequestContainer, Security> _securitiesResolver 
                  = NoResolver<SecurityRequestContainer, Security>.Instance;

        private readonly object _requestInProgressLock = new();

        private bool _initialized;

        public override void Initialize(ExecutionLoop entityNotificationLoop)
        {
#if TRACE
            this.Trace();
#endif
            lock (_callbackLock)
            {
                _bookResolver = EntityResolvers.GetOrderbooksResolver();
                _securitiesResolver = EntityResolvers.GetSecurityResolver();
                base.Initialize(entityNotificationLoop);
                _initialized = true; 
            }
        }

        public OrderBook? Create(ref OrderbookRequestContainer request)
        {
#if TRACE
            this.Trace();
#endif
#if DEBUG
            EnsureInitialized(); 
#endif

            if(_securitiesResolver.Resolve(ref request.SecurityRequest) is not Security security)
            {
                return null;
            }

            var book = new OrderBook(security);

            OrderbookWrapper.UpdateOrderBook(book);

            return book;
        }
        public bool Update(OrderBook book)
        {
#if TRACE
            this.Trace();
#endif
#if DEBUG
            EnsureInitialized();
#endif

            return OrderbookWrapper.UpdateOrderBook(book);
        }

        protected override int OnNewData(IntPtr state)
        {
#if TRACE
            //this.Trace();
#endif
            lock (_callbackLock)
            {
                try
                {
                    _requestContainerCreationArgs.LuaProvider = state;

                    var request = FunctionsWrappers.ReadCallbackArguments(ref _requestContainerCreationArgs);
                    var entity = _bookResolver.GetFromCache(ref request);

                    if (entity != null && Update(entity))
                    {
#if DEBUG
                        //LogEntityUpdated(entity); 
#endif
                        _eventSignalizer.QueueEntity(EntityChanged, entity);

                        return 1;
                    }

                    if (CreationIsApproved(ref request) && (entity = Create(ref request)) != null)
                    {
#if DEBUG
                        //LogEntityCreated(entity);
#endif
                        _bookResolver.CacheEntity(ref request, entity);
                        _eventSignalizer.QueueEntity(NewEntity, entity);
                    }

                    return 1;
                }
                catch (Exception e)
                {
                    _log.Error(CALLBACK_EXCEPTION_MSG, e);

                    return 0;
                }
            }
        }
        private void LogEntityCreated(OrderBook entity)
        {
            _log.Debug($"Received new {nameof(OrderBook)} for {entity.Security}\n{entity.Print()}");
        }
        private void LogEntityUpdated(OrderBook entity)
        {
            _log.Debug($"Received updates for {nameof(OrderBook)} {entity.Security}\n{entity.Print()}");
        }

        private void EnsureInitialized([CallerMemberName] string? method = null)
        {
            if (!_initialized)
            {
                throw new InvalidOperationException($"Calling {method ?? "METHOD_NOT_PROVIDED"} from {this.GetType().Name} before it was initialized.");
            }
        }

        #region Singleton
        [SingletonInstance]
        public static OrderbooksProvider Instance { get; } = new();
        private OrderbooksProvider() { }
        #endregion
    }
}
