namespace BasicConcepts.SecuritySpecifics
{
    public interface ICalendarSpread : ISecurity, IExpiring
    {
        IExpiring? NearTermLeg { get; }
        IExpiring? LongTermLeg { get; }
        TimeSpan? ExpiryTimeDelta { get; }
    }
}