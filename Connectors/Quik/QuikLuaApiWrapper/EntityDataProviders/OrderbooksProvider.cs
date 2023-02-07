using Quik.Entities;
using Quik.EntityProviders.Attributes;
using Quik.EntityProviders.QuikApiWrappers;
using Quik.EntityProviders.RequestContainers;

using CallbackParameters = Quik.EntityProviders.QuikApiWrappers.FunctionsWrappers.ReadCallbackArgs<string, string, Quik.EntityProviders.RequestContainers.OrderbookRequestContainer>;

namespace Quik.EntityProviders
{
    internal sealed class OrderbooksProvider : IDisposable
    {
        public AllowEntityCreationFilter<OrderbookRequestContainer> CreationIsApproved = delegate { return true; };
        public EntityEventHandler<OrderBook> EntityChanged = delegate { };
        public EntityEventHandler<OrderBook> NewEntity = delegate { };

        private CallbackParameters _updatedArgs;
        private EntityResolver<OrderbookRequestContainer, OrderBook> _bookResolver;
        private EntityResolver<SecurityRequestContainer, Security> _securitiesResolver;

        private readonly EventSignalizer<OrderBook> _eventSignalizer = new();
        private readonly object _requestInProgressLock = new();
        private readonly object _callbackLock = new();

        private bool _disposed;

        public void Initialize()
        {
            _updatedArgs.Callback = OrderbookRequestContainer.Create;
            _bookResolver = EntityResolvers.GetOrderbooksResolver();
            _securitiesResolver = EntityResolvers.GetSecurityResolver();
        }
        public void SubscribeCallback()
        {
            Quik.Lua.RegisterCallback(OnNewData, OrderbookWrapper.CALLBACK_METHOD);
        }

        public OrderBook? Create(ref OrderbookRequestContainer request)
        {
            if (_securitiesResolver.Resolve(ref request.SecurityRequest) is not Security security)
            {
                return null;
            }

            var book = new OrderBook(security);

            OrderbookWrapper.UpdateOrderBook(book);

            return book;
        }
        public void Update(OrderBook book)
        {
            OrderbookWrapper.UpdateOrderBook(book);
        }

        private int OnNewData(IntPtr state)
        {
            lock (_callbackLock)
            {
                _updatedArgs.LuaProvider = state;

                var request = FunctionsWrappers.ReadCallbackArguments(ref _updatedArgs);
                var entity = _bookResolver.GetFromCache(ref request);

                if (entity != null)
                {
                    Update(entity);

                    _eventSignalizer.QueueEntity<EntityEventHandler<OrderBook>>(EntityChanged, entity);

                    return 1;
                }

                if (CreationIsApproved(ref request) && (entity = Create(ref request)) != null)
                {
                    _bookResolver.CacheEntity(ref request, entity);
                    _eventSignalizer.QueueEntity<EntityEventHandler<OrderBook>>(NewEntity, entity);
                }

                return 1;
            }
        }

        #region Singleton
        [SingletonInstance]
        public static OrderbooksProvider Instance { get; } = new();
        private OrderbooksProvider()
        {
        }
        #endregion

        #region IDisposable
        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _eventSignalizer.Stop();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposed = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~OrderbooksProvider()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        } 
        #endregion
    }
}
