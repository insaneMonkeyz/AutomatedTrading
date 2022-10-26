using BasicConcepts;
using BasicConcepts.SecuritySpecifics;

namespace QuikLuaApi.Entities
{
    internal class Futures : SecurityBase, IFutures
    {
        public override string ClassCode => QuikApi.QuikApi.FUTURES_CLASS_CODE;
        public ISecurity? Underlying { get; init; }
        public DateTimeOffset Expiry { get; init; }

        public Futures(ref SecurityParamsContainer container) : base(ref container)
        {

        }
    }
}