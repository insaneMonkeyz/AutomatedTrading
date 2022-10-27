using BasicConcepts;
using BasicConcepts.SecuritySpecifics;

namespace QuikLuaApi.Entities
{
    internal abstract class MoexDerivativeBase : SecurityBase, ILimitedPrice, IExpiring
    {
        public Decimal5 UpperPriceLimit { get; internal set; }
        public Decimal5 LowerPriceLimit { get; internal set; }
        public TimeSpan TimeToExpiry => DateTimeOffset.Now - Expiry;
        public DateTimeOffset Expiry { get; init; }

        protected MoexDerivativeBase(ref SecurityParamsContainer container) : base(ref container)
        {
        }

    }
}