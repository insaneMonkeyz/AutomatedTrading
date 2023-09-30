namespace TradingConcepts.SecuritySpecifics
{
    public class FuturesDescription : SecurityDescription, IExpiring
    {
        public override Type SecurityType => typeof(IFutures);
        public DateTime Expiry { get; init; }
        public TimeSpan TimeToExpiry => Expiry - DateTime.UtcNow;
    }
}
