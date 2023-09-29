namespace TradingConcepts.SecuritySpecifics
{
    public interface ICalendarSpread : IDerivative, IExpiring
    {
        IExpiring? NearTermLeg { get; }
        IExpiring? LongTermLeg { get; }
        TimeSpan? ExpiryTimeDelta { get; }
    }
}