using TradingConcepts;
using TradingConcepts.SecuritySpecifics;

namespace Quik.Entities
{
    internal class Futures : MoexDerivativeBase, IFutures
    {
        public override string ClassCode => MoexSpecifics.FUTURES_CLASS_CODE;
        public ISecurity? Underlying { get; init; }

        public Futures(ref SecurityParamsContainer container) : base(ref container)
        {

        }

        public override bool Equals(object? obj)
        {
            return obj is Futures fut
                && (fut.Underlying?.Equals(Underlying) ?? Underlying is null)
                && base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), 3847511);
        }
    }
}