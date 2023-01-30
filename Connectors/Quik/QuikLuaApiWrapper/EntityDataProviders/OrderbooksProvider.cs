using BasicConcepts;
using Quik.EntityProviders.QuikApiWrappers;
using Quik;
using Quik.Entities;

using static Quik.QuikProxy;
using Quik.EntityProviders.RequestContainers;
using BasicConcepts.SecuritySpecifics;
using Quik.EntityProviders.Attributes;

namespace Quik.EntityProviders
{
    internal sealed class OrderbooksProvider : UpdatableEntitiesProvider<OrderBook, OrderbookRequestContainer>
    {
        private readonly object _securityRequestLock = new();
        private readonly SecurityRequestContainer _securityRequest = new();
        private readonly EntityResolver<SecurityRequestContainer, Security> _securitiesResolver;

        protected override string QuikCallbackMethod => OrderbookWrapper.CALLBACK_METHOD;
        protected override string AllEntitiesTable => string.Empty;

        private static readonly List<OrderBook> _emptyList= new(0);

        public override List<OrderBook> GetAllEntities()
        {
            return _emptyList;
        }

        public override OrderBook? Create(OrderbookRequestContainer request)
        {
            if (ResolveSecurity(request) is not Security security)
            {
                return null;
            }

            var orderbook = new OrderBook(security);
            
            Update(orderbook);

            return orderbook;
        }
        protected override OrderBook? Create(LuaState state)
        {
            throw new InvalidOperationException("Automatic creation of orderbooks is not allowed. " +
                "Must manually create one and then use Update() method to fill it with values.");
        }
        public override void Update(OrderBook entity)
        {
            BuildEntityResolveRequest(State);
            Update(entity, State);
        }
        protected override void Update(OrderBook book, LuaState state)
        {
            OrderbookWrapper.UpdateOrderBook(_resolveEntityRequest.ClassCode, book);
        }
        protected override void BuildEntityResolveRequest(LuaState state)
        {
            OrderbookWrapper.Set(state);

            _resolveEntityRequest.Ticker = OrderbookWrapper.Ticker;
            _resolveEntityRequest.ClassCode = OrderbookWrapper.ClassCode;
        }

        private Security? ResolveSecurity(OrderbookRequestContainer request)
        {
            lock (_securityRequestLock)
            {
                _securityRequest.Ticker = request.Ticker;
                _securityRequest.ClassCode = request.ClassCode;
                return _securitiesResolver.GetEntity(_securityRequest);
            }
        }

        #region Singleton
        [SingletonInstance]
        public static OrderbooksProvider Instance { get; } = new();
        private OrderbooksProvider()
        {
            _securitiesResolver = EntityResolvers.GetSecurityResolver();
        }
        #endregion
    }
}
