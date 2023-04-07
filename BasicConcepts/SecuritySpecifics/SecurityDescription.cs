namespace TradingConcepts.SecuritySpecifics
{
    public class SecurityDescription
    {
        public virtual Type SecurityType => typeof(ISecurity);

        public required string Ticker { get; init; }
    }
}
