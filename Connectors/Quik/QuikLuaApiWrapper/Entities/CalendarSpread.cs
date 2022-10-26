using BasicConcepts.SecuritySpecifics;

namespace QuikLuaApi.Entities
{
    internal class CalendarSpread : SecurityBase, ICalendarSpread
    {
        public override string ClassCode => QuikApi.QuikApi.CALENDAR_SPREADS_CLASS_CODE;
        public TimeSpan ExpiryTimeDelta
        {
            get => LongTermLeg.Expiry - NearTermLeg.Expiry;
        }
        public DateTimeOffset Expiry { get; }
        public IExpiring? NearTermLeg { get; }
        public IExpiring? LongTermLeg { get; }
    
        public CalendarSpread(ref SecurityParamsContainer container, IExpiring neartermLeg, IExpiring longtermLeg) 
            : base(ref container)
        {
            NearTermLeg = neartermLeg;
            LongTermLeg = longtermLeg;
            Expiry = NearTermLeg.Expiry;
        }
        public CalendarSpread(ref SecurityParamsContainer container, DateTimeOffset expiry) 
            : base(ref container)
        {
            Expiry = expiry;
        }
    }
}