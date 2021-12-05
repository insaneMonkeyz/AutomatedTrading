namespace Strategies
{
    public sealed class GrabStrategy : Strategy
    {
        public GrabOrderOperator HighPriceAsk { get; }
        public GrabOrderOperator LowPriceAsk { get; }
        public GrabOrderOperator HighPriceBid { get; }
        public GrabOrderOperator LowPriceBid { get; }
    }

    public sealed class GrabOrderOperator : SingleOrderOperator
    {
        public decimal GrabPriceOffset
        {
            get => _priceOffset;
            set => _priceOffset = value;
        }
    }
}
