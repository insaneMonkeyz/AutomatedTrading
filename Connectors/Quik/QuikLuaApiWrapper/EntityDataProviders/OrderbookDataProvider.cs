using BasicConcepts;
using Quik.EntityDataProviders.QuikApiWrappers;
using Quik;
using Quik.Entities;

using static Quik.QuikProxy;
using Quik.EntityDataProviders.RequestContainers;
using BasicConcepts.SecuritySpecifics;
using Quik.EntityDataProviders.Attributes;

namespace Quik.EntityDataProviders
{
    internal sealed class OrderbookDataProvider : DataProvider<OrderBook, OrderbookRequestContainer>
    {
        protected override string QuikCallbackMethod => OrderbookWrapper.CALLBACK_METHOD;

        public IOptimizedOrderBook CreateOrderBook(Security sec)
        {
            var book = new OrderBook()
            {
                Security = sec
            };

            Update(book);

            return book;
        }
        public override void Update(OrderBook entity)
        {
            Update(entity, State);
        }
        protected override void Update(OrderBook book, LuaState state)
        {
            OrderbookWrapper.Set(state);
            OrderbookWrapper.UpdateOrderBook(_resolveEntityRequest.ClassCode, book);
        }

        protected override OrderBook? Create(LuaState state)
        {
            throw new InvalidOperationException("Automatic creation of orderbooks is not allowed. " +
                "Must manually create one and then use Update() method to fill it with values.");
        }
        protected override void BuildEntityResolveRequest(LuaState state)
        {
            OrderbookWrapper.Set(state);

            _resolveEntityRequest.Ticker = OrderbookWrapper.Ticker;
            _resolveEntityRequest.ClassCode = OrderbookWrapper.ClassCode;
        }

        #region Singleton
        [SingletonInstance]
        public static OrderbookDataProvider Instance { get; } = new();
        private OrderbookDataProvider() : base(EntityResolversFactory.GetOrderbooksResolver())
        { }
        #endregion
    }
}
