using BasicConcepts;
using BasicConcepts.SecuritySpecifics;

namespace QuikLuaApi.Entities
{
    internal class CalendarSpread : MoexDerivativeBase, ICalendarSpread
    {
        public override string ClassCode => QuikApi.QuikApi.CALENDAR_SPREADS_CLASS_CODE;
        public TimeSpan? ExpiryTimeDelta { get; }
        public IExpiring? NearTermLeg { get; }
        public IExpiring? LongTermLeg { get; }

        public CalendarSpread(ref SecurityParamsContainer container, IExpiring neartermLeg, IExpiring longtermLeg) 
            : base(ref container)
        {
            NearTermLeg = neartermLeg;
            LongTermLeg = longtermLeg;
            Expiry = NearTermLeg.Expiry;
            ExpiryTimeDelta = LongTermLeg.Expiry - NearTermLeg.Expiry;
        }
        public CalendarSpread(ref SecurityParamsContainer container, DateTimeOffset expiry) 
            : base(ref container)
        {
            Expiry = expiry;
        }
    }
}