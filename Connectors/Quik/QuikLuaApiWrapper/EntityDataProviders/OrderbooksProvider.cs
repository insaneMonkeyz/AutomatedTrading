using System.Runtime.CompilerServices;
using Quik.Entities;
using Quik.EntityProviders.Attributes;
using Quik.EntityProviders.QuikApiWrappers;
using Quik.EntityProviders.RequestContainers;

using static Quik.QuikProxy;

namespace Quik.EntityProviders
{
    internal sealed class OrderbooksProvider : UpdatableEntitiesProvider<OrderBook, OrderbookRequestContainer>
    {
        private readonly EntityResolver<SecurityRequestContainer, Security> _securitiesResolver;

        protected override string QuikCallbackMethod => OrderbookWrapper.CALLBACK_METHOD;
        protected override string AllEntitiesTable => string.Empty;

        private static readonly List<OrderBook> _emptyList = new(0);

        public override List<OrderBook> GetAllEntities()
        {
            return _emptyList;
        }

        public override OrderBook? Create(OrderbookRequestContainer request)
        {
            if (_securitiesResolver.GetEntity(request.SecurityContainer) is not Security security)
            {
                return null;
            }

            var orderbook = new OrderBook(security);

            Update(orderbook, State);

            return orderbook;
        }
        protected override OrderBook? Create(LuaState state)
        {
            throw new InvalidOperationException("Automatic creation of orderbooks is not allowed. " +
                "Must manually create one and then use Update() method to fill it with values.");
        }
        public override void Update(OrderBook entity)
        {
            Update(entity, State);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void Update(OrderBook book, LuaState state)
        {
            OrderbookWrapper.UpdateOrderBook(book);
        }
        protected override void ParseNewDataParams(LuaState state)
        {
            OrderbookWrapper.Set(state);

            _resolveEntityRequest.SecurityContainer.Ticker = OrderbookWrapper.Ticker;
            _resolveEntityRequest.SecurityContainer.ClassCode = OrderbookWrapper.ClassCode;
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
