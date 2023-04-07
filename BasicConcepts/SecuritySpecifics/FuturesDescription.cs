namespace TradingConcepts.SecuritySpecifics
{
    public class FuturesDescription : SecurityDescription
    {
        public override Type SecurityType => typeof(IFutures);
        public DateTime ExpiryDate { get; init; }

    }
}
