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
            get => !string.IsNullOrEmpty(ClassCode)
                && (TransactionId != default || !string.IsNullOrEmpty(ExchangeAssignedId));
        }

        public bool IsMatching(Order? entity)
        {
            return entity != null
                && entity.Security.ClassCode == ClassCode
                && (entity.ExchangeAssignedIdString == ExchangeAssignedId
                            || entity.TransactionId == TransactionId);
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
            if (string.IsNullOrEmpty(ClassCode))
            {
                throw new InvalidOperationException($"Trying to generate hash of {nameof(OrderRequestContainer)} with empty {nameof(ClassCode)}");
            }

            if (!string.IsNullOrEmpty(ExchangeAssignedId))
            {
                return HashCode.Combine(ExchangeAssignedId, ClassCode);
            }

            if (TransactionId != default)
            {
                return HashCode.Combine(TransactionId, ClassCode);
            }

            throw new InvalidOperationException($"Generating hash for empty values {nameof(ExchangeAssignedId)} and {nameof(TransactionId)}");
        }

        public override string ToString()
        {
            return $"OrderRequest: {{TransactionId: {TransactionId}, OrderId: {ExchangeAssignedId ?? "null"}, ClassCode: {ClassCode ?? "null"}}}";
        }
    }
}
