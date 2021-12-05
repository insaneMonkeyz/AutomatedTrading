using System;

namespace Strategies
{
    public sealed class PriceProvider
    {
        public Security Security { get; }
        public Quote Ask { get; }
        public Quote Bid { get; }
    }

    internal sealed class SpreadDiscoveryService
    {
        public event Action<decimal> NewData;

        public void RegisterSpreadRequest(Security baseSecurity, Security security)
        {
            throw new NotImplementedException();
        }
    }
}