using Quik.Entities;

namespace Quik.EntityDataProviders.RequestContainers
{
    internal class OrderRequestContainer : IRequestContainer<Order>
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

        public override bool Equals(object? obj)
        {
            return obj is OrderRequestContainer container
                && ExchangeAssignedId == container.ExchangeAssignedId
                && ClassCode == container.ClassCode;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ExchangeAssignedId, ClassCode);
        }

        public override string ToString()
        {
            return HasData 
                ? ExchangeAssignedId.ToString()
                : "N/A";
        }
    }
}
