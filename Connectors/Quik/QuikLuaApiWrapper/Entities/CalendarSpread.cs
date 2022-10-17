using BasicConcepts;
using BasicConcepts.SecuritySpecifics;

namespace QuikLuaApi.Entities
{
    internal class CalendarSpread : SecurityBase, ICalendarSpread
    {
        public override string ClassCode => QuikApi.CALENDAR_SPREADS_CLASS_CODE;
        public TimeSpan ExpiryTimeDelta
        {
            get => LongTermLeg.Expiry - NearTermLeg.Expiry;
        }
        public DateTimeOffset Expiry { get; }
        public IExpiring NearTermLeg { get; }
        public IExpiring LongTermLeg { get; }
    
        public CalendarSpread(SecurityParamsContainer container, IExpiring neartermLeg, IExpiring longtermLeg) 
            : base(container)
        {
            NearTermLeg = neartermLeg;
            LongTermLeg = longtermLeg;
            Expiry = NearTermLeg.Expiry;
        }
        public CalendarSpread(SecurityParamsContainer container, DateTimeOffset expiry) 
            : base(container)
        {
            Expiry = expiry;
        }
    }
}