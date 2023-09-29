using TradingConcepts;
using TradingConcepts.SecuritySpecifics;

namespace Quik.Entities
{
    internal class CalendarSpread : MoexDerivativeBase, ICalendarSpread
    {
        public override string ClassCode => MoexSpecifics.CALENDAR_SPREADS_CLASS_CODE;
        public TimeSpan? ExpiryTimeDelta { get; }
        public IExpiring? NearTermLeg { get; }
        public IExpiring? LongTermLeg { get; }
        public ISecurity? Underlying { get; init; }

        public CalendarSpread(ref SecurityParamsContainer container, IExpiring neartermLeg, IExpiring longtermLeg) 
            : base(ref container)
        {
            NearTermLeg = neartermLeg;
            LongTermLeg = longtermLeg;
            Expiry = NearTermLeg.Expiry;
            ExpiryTimeDelta = LongTermLeg.Expiry - NearTermLeg.Expiry;
        }
        public CalendarSpread(ref SecurityParamsContainer container, DateTime expiry) 
            : base(ref container)
        {
            Expiry = expiry;
        }

        public override bool Equals(object? obj)
        {
            return base.Equals(obj)
                && obj is CalendarSpread spread
                && spread.ExpiryTimeDelta == ExpiryTimeDelta
                && (spread.NearTermLeg?.Equals(NearTermLeg) ?? NearTermLeg is null)
                && (spread.LongTermLeg?.Equals(LongTermLeg) ?? LongTermLeg is null)
                && (spread.Underlying?.Equals(Underlying) ?? Underlying is null);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(ExpiryTimeDelta, NearTermLeg, LongTermLeg);
        }
    }
}