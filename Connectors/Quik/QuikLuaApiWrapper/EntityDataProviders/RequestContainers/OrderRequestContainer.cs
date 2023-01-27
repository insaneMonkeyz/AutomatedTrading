using Quik.Entities;

namespace Quik.EntityDataProviders.RequestContainers
{
    internal class OrderRequestContainer : IRequestContainer<Order>
    {
        public long ExchangeAssignedId;

        public bool HasData => ExchangeAssignedId != default;

        public bool IsMatching(Order? entity)
        {
            return entity?.ExchangeAssignedId == ExchangeAssignedId;
        }

        public override bool Equals(object? obj)
        {
            return obj is OrderRequestContainer container &&
                ExchangeAssignedId == container.ExchangeAssignedId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ExchangeAssignedId, 31544785);
        }

        public override string ToString()
        {
            return HasData 
                ? ExchangeAssignedId.ToString()
                : "N/A";
        }
    }
}
