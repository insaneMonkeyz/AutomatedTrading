using BasicConcepts.SecuritySpecifics;

namespace Quik.Entities
{
    internal class Stock : Security, IStock
    {
        public override string ClassCode => MoexSpecifics.STOCK_CLASS_CODE;

        public Stock(ref SecurityParamsContainer container) : base(ref container)
        {

        }
    }
}