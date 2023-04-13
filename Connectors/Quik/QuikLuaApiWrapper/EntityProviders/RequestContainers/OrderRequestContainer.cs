using Quik.Entities;

namespace Quik.EntityProviders.RequestContainers
{
    internal struct OrderRequestContainer : IRequestContainer<Order>, IEquatable<OrderRequestContainer>
    {
        public string? ExchangeAssignedId;
        public string? ClassCode;

        public bool HasData
        {
            get => !(string.IsNullOrEmpty(ClassCode) || string.IsNullOrEmpty(ExchangeAssignedId));
        }

        public bool IsMatching(Order? entity)
        {
            return entity != null
                && entity.Security.ClassCode == ClassCode
                && entity.ExchangeAssignedIdString == ExchangeAssignedId;
        }

        public bool Equals(OrderRequestContainer other)
        {
            return ClassCode == other.ClassCode
                && ExchangeAssignedId == other.ExchangeAssignedId;
        }

        public override bool Equals(object? obj)
        {
            return obj is OrderRequestContainer container
                && ExchangeAssignedId == container.ExchangeAssignedId
                && ClassCode == container.ClassCode;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ExchangeAssignedId, ClassCode, 88002000600);
        }

        public override string ToString()
        {
            return $"OrderRequest: {{OrderId: {ExchangeAssignedId ?? "null"}, ClassCode: {ClassCode ?? "null"}}}";
        }
    }
}
