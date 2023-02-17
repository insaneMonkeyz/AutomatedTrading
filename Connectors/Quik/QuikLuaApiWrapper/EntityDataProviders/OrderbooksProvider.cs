using System.Diagnostics;
using System.Runtime.CompilerServices;
using Quik.Entities;
using Quik.EntityProviders.Attributes;
using Quik.EntityProviders.QuikApiWrappers;
using Quik.EntityProviders.RequestContainers;
using Quik.Lua;
using CallbackParameters = Quik.EntityProviders.QuikApiWrappers.FunctionsWrappers.ReadCallbackArgs<string?, string?, Quik.EntityProviders.RequestContainers.OrderbookRequestContainer>;

namespace Quik.EntityProviders
{
    internal sealed class OrderbooksProvider : IDisposable
    {
        public AllowEntityCreationFilter<OrderbookRequestContainer> CreationIsApproved = delegate { return true; };
        public EntityEventHandler<OrderBook> EntityChanged = delegate { };
        public EntityEventHandler<OrderBook> NewEntity = delegate { };

        private CallbackParameters _requestContainerCreationArgs = new()
        {
            Callback = OrderbookRequestContainer.Create
        };
        private EntityResolver<OrderbookRequestContainer, OrderBook> _bookResolver 
                  = NoResolver<OrderbookRequestContainer, OrderBook>.Instance;
        private EntityResolver<SecurityRequestContainer, Security> _securitiesResolver 
                  = NoResolver<SecurityRequestContainer, Security>.Instance;

        private readonly LuaFunction _onNewDataCallback;
        private IEntityEventSignalizer<OrderBook> _eventSignalizer = new DirectEntitySignalizer<OrderBook>();
        private readonly object _requestInProgressLock = new();
        private readonly object _callbackLock = new();

        private bool _initialized;
        private bool _disposed;

        public void Initialize(ExecutionLoop entityNotificationLoop)
        {
#if TRACE
            Extentions.Trace(nameof(OrderbooksProvider));
#endif
            lock (_callbackLock)
            {
                _bookResolver = EntityResolvers.GetOrderbooksResolver();
                _securitiesResolver = EntityResolvers.GetSecurityResolver();
                _eventSignalizer = new EventSignalizer<OrderBook>(entityNotificationLoop)
                {
                    IsEnabled = true
                };
                _initialized = true; 
            }
        }
        public void SubscribeCallback()
        {
#if TRACE
            Extentions.Trace(nameof(OrderbooksProvider));
#endif
            Quik.Lua.RegisterCallback(_onNewDataCallback, OrderbookWrapper.CALLBACK_METHOD);
        }

        public OrderBook? Create(ref OrderbookRequestContainer request)
        {
#if TRACE
            Extentions.Trace(nameof(OrderbooksProvider));
#endif
            EnsureInitialized();
            
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
            Extentions.Trace(nameof(OrderbooksProvider));
#endif
            EnsureInitialized();

            return OrderbookWrapper.UpdateOrderBook(book);
        }

        private int OnNewData(IntPtr state)
        {
#if TRACE
            Extentions.Trace(nameof(OrderbooksProvider));
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
                        _eventSignalizer.QueueEntity(EntityChanged, entity);

                        return 1;
                    }

                    if (CreationIsApproved(ref request) && (entity = Create(ref request)) != null)
                    {
                        _bookResolver.CacheEntity(ref request, entity);
                        _eventSignalizer.QueueEntity(NewEntity, entity);
                    }

                    return 1;
                }
                catch (Exception e)
                {
                    e.DebugPrintException();

                    return 0;
                }
            }
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
        private OrderbooksProvider()
        {
            _onNewDataCallback = OnNewData;
        }
        #endregion

        #region IDisposable
        private void Dispose(bool disposing)
        {
#if TRACE
            Extentions.Trace(nameof(OrderbooksProvider));
#endif
            if (!_disposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _eventSignalizer.Dispose();
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
