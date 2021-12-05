namespace Strategies
{
    public sealed class QuotingStrategy : Strategy
    {
        public QuotingOrderOperator SellOrder { get; }
        public QuotingOrderOperator BuyOrder { get; }
    }

    public sealed class QuotingOrderOperator : SingleOrderOperator
    {
        public int PriceRange { get; set; }
    }
}
