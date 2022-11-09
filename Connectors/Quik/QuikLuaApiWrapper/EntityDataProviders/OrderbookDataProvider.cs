using BasicConcepts;
using Quik.EntityDataProviders.QuikApiWrappers;
using Quik;
using Quik.Entities;

using static Quik.QuikProxy;
using Quik.EntityDataProviders.EntityDummies;
using BasicConcepts.SecuritySpecifics;

namespace Quik.EntityDataProviders
{
    internal sealed class OrderbookDataProvider : BaseDataProvider<IOptimizedOrderBook , SecurityDummy>
    {
        public static OrderbookDataProvider Instance { get; } = new();
        protected override string QuikCallbackMethod => OrderbookWrapper.CALLBACK_METHOD;

        public IOptimizedOrderBook CreateOrderBook(SecurityBase sec)
        {
            var book = new OrderBook()
            {
                Security = sec
            };

            Update(book);

            return book;
        }
        public override void Update(IOptimizedOrderBook entity)
        {
            Update(entity, State);
        }
        protected override void Update(IOptimizedOrderBook book, LuaState state)
        {
            OrderbookWrapper.Set(state);
            OrderbookWrapper.UpdateOrderBook(_dummy.ClassCode, book);
        }

        protected override OrderBook Create(LuaState state)
        {
            throw new InvalidOperationException("Automatic creation of orderbooks is not allowed. " +
                "Must manually create one and then use Update() method to fill it with values.");
        }
        protected override void SetDummy(LuaState state)
        {
            OrderbookWrapper.Set(state);

            _dummy.Ticker = OrderbookWrapper.Ticker;
            _dummy.ClassCode = OrderbookWrapper.ClassCode;
        }
    }
}
