using BasicConcepts.SecuritySpecifics;

namespace Quik.Entities
{
    internal class Stock : SecurityBase, IStock
    {
        public override string ClassCode => QuikApi.QuikApi.STOCK_CLASS_CODE;

        public Stock(ref SecurityParamsContainer container) : base(ref container)
        {

        }
    }
}