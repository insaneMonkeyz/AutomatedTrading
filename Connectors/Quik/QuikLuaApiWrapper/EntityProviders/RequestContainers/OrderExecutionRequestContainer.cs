using Quik.Entities;

namespace Quik.EntityProviders.RequestContainers
{
    internal struct OrderExecutionRequestContainer : IRequestContainer<OrderExecution>, IEquatable<OrderExecutionRequestContainer>
    {
        public long TradeId;
        public string? ExchangeAssignedOrderId;

        public bool HasData 
        {
            get => TradeId != 0 && string.IsNullOrWhiteSpace(ExchangeAssignedOrderId);
        }

        public bool IsMatching(OrderExecution? entity)
        {
            return entity is OrderExecution orderExecution
                && TradeId == orderExecution.TradeId
                && ExchangeAssignedOrderId == orderExecution.Order.ExchangeAssignedIdString;
        }

        public bool Equals(OrderExecutionRequestContainer obj)
        {
            return TradeId == obj.TradeId
                && ExchangeAssignedOrderId == obj.ExchangeAssignedOrderId;
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

        public override string ToString()
        {
            return $"Order Execution Request: {{TradeId: {TradeId}, OrderId: {ExchangeAssignedOrderId ?? "null"}}}";
        }
    }
}
