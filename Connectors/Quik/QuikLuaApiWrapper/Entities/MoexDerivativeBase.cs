using TradingConcepts;
using TradingConcepts.SecuritySpecifics;

namespace Quik.Entities
{
    internal abstract class MoexDerivativeBase : Security, ILimitedPrice, IExpiring
    {
        public Decimal5? UpperPriceLimit { get; internal set; }
        public Decimal5? LowerPriceLimit { get; internal set; }
        public TimeSpan TimeToExpiry => Expiry - DateTimeOffset.Now;
        public DateTimeOffset Expiry { get; init; }

        protected MoexDerivativeBase(ref SecurityParamsContainer container) : base(ref container)
        {
        }

        public override bool Equals(object? obj)
        {
            return obj is MoexDerivativeBase derivative
                && derivative.Expiry == Expiry
                && derivative.UpperPriceLimit == UpperPriceLimit
                && derivative.LowerPriceLimit == LowerPriceLimit
                && base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(UpperPriceLimit, LowerPriceLimit, Expiry, 1001010);
        }
    }
}