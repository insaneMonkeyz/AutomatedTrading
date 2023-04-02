using Quik.Entities;

namespace Quik.EntityProviders.RequestContainers
{
    internal struct OrderRequestContainer : IRequestContainer<Order>, IEquatable<OrderRequestContainer>
    {
        public long TransactionId;
        public string? ExchangeAssignedId;
        public string? ClassCode;

        public bool HasData
        {
            get => TransactionId != default && !string.IsNullOrEmpty(ClassCode);
        }

        public bool IsMatching(Order? entity)
        {
            return entity != null
                && entity.ExchangeAssignedIdString == ExchangeAssignedId
                && entity.TransactionId == TransactionId
                && entity.Security.ClassCode == ClassCode;
        }

        public bool Equals(OrderRequestContainer other)
        {
            return ClassCode == other.ClassCode
                && TransactionId == other.TransactionId
                && ExchangeAssignedId == other.ExchangeAssignedId;
        }

        public override bool Equals(object? obj)
        {
            return obj is OrderRequestContainer container
                && ExchangeAssignedId == container.ExchangeAssignedId
                && TransactionId == container.TransactionId
                && ClassCode == container.ClassCode;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ExchangeAssignedId, ClassCode, TransactionId);
        }

        public override string ToString()
        {
            return HasData
                ? string.IsNullOrEmpty(ExchangeAssignedId)
                    ? $"OrderId: null; TransactionId: {TransactionId}"
                    : $"OrderId: {ExchangeAssignedId}; TransactionId: {TransactionId}"
                : "N/A";
        }
    }
}
