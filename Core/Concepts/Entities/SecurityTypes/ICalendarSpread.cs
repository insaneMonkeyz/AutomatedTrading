namespace Core.Concepts.Entities.SecurityTypes
{
    public interface ICalendarSpread : ISecurity, IExpiringContract
    {
        ISecurity NearTermSecurity { get; }
        ISecurity LongTermSecurity { get; }
        TimeSpan TimeDelta { get; }
    }
}