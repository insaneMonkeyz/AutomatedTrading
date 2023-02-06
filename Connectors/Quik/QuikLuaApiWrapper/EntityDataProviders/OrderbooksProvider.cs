using System.Runtime.CompilerServices;
using Quik.Entities;
using Quik.EntityProviders.Attributes;
using Quik.EntityProviders.QuikApiWrappers;
using Quik.EntityProviders.RequestContainers;
using Quik.Lua;
using static Quik.Quik;

namespace Quik.EntityProviders
{
    internal sealed class OrderbooksProvider : UpdatableEntitiesProvider<OrderBook, OrderbookRequestContainer>
    {
        private readonly EntityResolver<SecurityRequestContainer, Security> _securitiesResolver;

        protected override Action<LuaWrap> SetWrapper => OrderbookWrapper.Set;
        protected override string QuikCallbackMethod => OrderbookWrapper.CALLBACK_METHOD;
        protected override string AllEntitiesTable => string.Empty;

        private static readonly List<OrderBook> _emptyList = new(0);

        public override List<OrderBook> GetAllEntities()
        {
            return _emptyList;
        }

        public override OrderBook? Create(OrderbookRequestContainer request)
        {
            if (_securitiesResolver.Resolve(request.SecurityContainer) is not Security security)
            {
                return null;
            }

            var orderbook = new OrderBook(security);

            Update(orderbook, Quik.Lua);

            return orderbook;
        }
        protected override OrderBook? Create(LuaWrap state)
        {
            throw new InvalidOperationException("Automatic creation of orderbooks is not allowed. " +
                "Must manually create one and then use Update() method to fill it with values.");
        }
        public override void Update(OrderBook entity)
        {
            Update(entity, Quik.Lua);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void Update(OrderBook book, LuaWrap state)
        {
            OrderbookWrapper.UpdateOrderBook(book);
        }
        protected override OrderbookRequestContainer CreateRequestFrom(LuaWrap state)
        {
            OrderbookWrapper.Set(state);

            return new()
            {
                SecurityContainer = new()
                {
                    Ticker = OrderbookWrapper.Ticker,
                    ClassCode = OrderbookWrapper.ClassCode,
                }
            };
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
