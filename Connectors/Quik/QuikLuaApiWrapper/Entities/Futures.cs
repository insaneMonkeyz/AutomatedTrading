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
    }
}