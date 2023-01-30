using Quik.Entities;
using Quik.EntityProviders.QuikApiWrappers;

namespace Quik.EntityProviders.RequestContainers
{
    internal class DerivativeRequestContainer : SecurityRequestContainer
    {
        public override bool HasData
        {
            get => !string.IsNullOrEmpty(Ticker);
        }
        public override bool IsMatching(Security? security)
        {
            return security?.Ticker == Ticker;
        }

        public override bool Equals(object? obj)
        {
            return obj is SecurityRequestContainer other
                && Ticker == other.Ticker;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                return (Ticker ?? string.Empty).GetHashCode() * 2272345; 
            }
        }
        public override string? ToString()
        {
            return string.IsNullOrEmpty(Ticker) 
                ? "Empty Security Request" 
                : Ticker;
        }
    }
}
