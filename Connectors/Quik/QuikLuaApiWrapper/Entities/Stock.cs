namespace QuikLuaApi.Entities
{
    internal class Stock : SecurityBase
    {
        public override string ClassCode => QuikApi.QuikApi.STOCK_CLASS_CODE;

        public Stock(SecurityParamsContainer container) : base(container)
        {

        }
    }
}