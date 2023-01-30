using Quik.Entities;

namespace Quik.EntityProviders.RequestContainers
{
    internal class OrderbookRequestContainer : SecurityBasedRequestContainer<OrderBook>
    {
        public override bool IsMatching(OrderBook? entity)
        {
            return entity != null && SecuritiesMatch(entity.Security);
        }
    }
}
