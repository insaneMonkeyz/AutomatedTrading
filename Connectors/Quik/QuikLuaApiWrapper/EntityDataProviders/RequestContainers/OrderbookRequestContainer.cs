using Quik.Entities;

namespace Quik.EntityProviders.RequestContainers
{
    internal struct OrderbookRequestContainer : IRequestContainer<OrderBook>, IEquatable<OrderbookRequestContainer>
    {
        public SecurityRequestContainer SecurityRequest;

        public static OrderbookRequestContainer Create(string? classcode, string? ticker)
        {
            return new OrderbookRequestContainer {
                SecurityRequest = SecurityRequestContainer.Create(classcode, ticker)
            };
        }

        public bool HasData => SecurityRequest.HasData;
        public bool IsMatching(OrderBook? entity)
        {
            return entity != null && SecurityRequest.IsMatching(entity.Security);
        }

        public bool Equals(OrderbookRequestContainer other)
        {
            return SecurityRequest.Equals(other.SecurityRequest);
        }
        public override bool Equals(object? obj)
        {
            return obj is OrderbookRequestContainer other
                && SecurityRequest.Equals(other.SecurityRequest);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                return SecurityRequest.GetHashCode() * 808852;
            }
        }
    }
}
