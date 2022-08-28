namespace Core.Concepts.Entities.SecurityTypes
{
    public interface ICalendarSpread : ISecurity, IExpiring
    {
        ISecurity NearTermSecurity { get; }
        ISecurity LongTermSecurity { get; }
        TimeSpan TimeDelta { get; }
    }
}