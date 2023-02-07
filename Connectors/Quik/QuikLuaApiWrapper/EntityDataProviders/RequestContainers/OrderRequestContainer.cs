using Quik.Entities;

namespace Quik.EntityProviders.RequestContainers
{
    internal struct OrderRequestContainer : IRequestContainer<Order>, IEquatable<OrderRequestContainer>
    {
        public string ExchangeAssignedId;
        public string ClassCode;

        public bool HasData => !string.IsNullOrEmpty(ExchangeAssignedId)
            && !string.IsNullOrEmpty(ClassCode);

        public bool IsMatching(Order? entity)
        {
            return entity != null
                && entity.ExchangeAssignedIdString == ExchangeAssignedId
                && entity.Security.ClassCode == ClassCode;
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
            unchecked
            {
                return HashCode.Combine(ExchangeAssignedId, ClassCode) * 2281488; 
            }
        }

        public override string ToString()
        {
            return HasData 
                ? ExchangeAssignedId
                : "N/A";
        }
    }
}
