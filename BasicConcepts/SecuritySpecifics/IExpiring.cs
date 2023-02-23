namespace TradingConcepts.SecuritySpecifics
{
    public interface IExpiring
    {
        DateTimeOffset Expiry { get; }
        TimeSpan TimeToExpiry { get; }
    }
}