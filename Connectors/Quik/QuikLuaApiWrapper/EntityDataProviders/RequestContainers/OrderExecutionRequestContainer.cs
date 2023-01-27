using Quik.Entities;

namespace Quik.EntityDataProviders.RequestContainers
{
    internal class OrderExecutionRequestContainer : IRequestContainer<OrderExecution>
    {
        public long TradeId;
        public long ExchangeAssignedOrderId;

        public bool HasData 
        {
            get => TradeId != 0 && ExchangeAssignedOrderId != 0;
        }

        public bool IsMatching(OrderExecution? entity)
        {
            return entity is OrderExecution orderExecution
                && TradeId == orderExecution.TradeId
                && ExchangeAssignedOrderId == orderExecution.Order.ExchangeAssignedId;
        }

        public override bool Equals(object? obj)
        {
            return obj is OrderExecutionRequestContainer container &&
                   TradeId == container.TradeId &&
                   ExchangeAssignedOrderId == container.ExchangeAssignedOrderId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TradeId, ExchangeAssignedOrderId);
        }
    }
}
