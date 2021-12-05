using System;

namespace Strategies
{
    public sealed class PriceDifferenceTracker
    {
        public TimeSpan UpdateFrequency { get; set; }
        public Security First { get; }
        public Security Second { get; }
        public decimal? Difference { get; }
        public string Code { get; }

        public event Action<PriceDifferenceTracker> Changed;

        public static PriceDifferenceTracker Subscribe(Security first, Security second)
        {
            throw new NotImplementedException();
        }
        public static void Unsubscribe(PriceDifferenceTracker priceOffset)
        {
            throw new NotImplementedException();
        }
    }
}