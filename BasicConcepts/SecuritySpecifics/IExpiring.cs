namespace TradingConcepts.SecuritySpecifics
{
    public interface IExpiring
    {
        DateTime Expiry { get; }
        TimeSpan TimeToExpiry { get; }
    }
}