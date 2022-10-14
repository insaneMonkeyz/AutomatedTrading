using BasicConcepts;
using BasicConcepts.SecuritySpecifics;

namespace QuikLuaApi.Entities
{
    internal class CalendarSpread : SecurityBase, ICalendarSpread
    {
        public TimeSpan ExpiryTimeDelta
        {
            get => LongTermLeg.Expiry - NearTermLeg.Expiry;
        }
        public DateTimeOffset Expiry
        {
            get => NearTermLeg.Expiry;
        }
        public IExpiring NearTermLeg { get; init; }
        public IExpiring LongTermLeg { get; init; }
    }
}