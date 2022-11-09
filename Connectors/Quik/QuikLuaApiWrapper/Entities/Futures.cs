using BasicConcepts;
using BasicConcepts.SecuritySpecifics;

namespace Quik.Entities
{
    internal class Futures : MoexDerivativeBase, IFutures
    {
        public override string ClassCode => QuikApi.QuikApi.FUTURES_CLASS_CODE;
        public ISecurity? Underlying { get; init; }

        public Futures(ref SecurityParamsContainer container) : base(ref container)
        {

        }
    }
}