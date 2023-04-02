namespace TradingConcepts.SecuritySpecifics
{
    public class SecurityDescription<TSecurity> where TSecurity : ISecurity
    {
        public Type SecurityType { get; } = typeof(TSecurity);

        public required string Ticker { get; init; }
    }
}
