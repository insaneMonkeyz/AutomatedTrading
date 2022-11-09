using BasicConcepts;
using BasicConcepts.SecuritySpecifics;

namespace Quik.Entities
{
    internal abstract class MoexDerivativeBase : SecurityBase, ILimitedPrice, IExpiring
    {
        public Decimal5? UpperPriceLimit { get; internal set; }
        public Decimal5? LowerPriceLimit { get; internal set; }
        public TimeSpan TimeToExpiry => Expiry - DateTimeOffset.Now;
        public DateTimeOffset Expiry { get; init; }

        protected MoexDerivativeBase(ref SecurityParamsContainer container) : base(ref container)
        {
        }

    }
}