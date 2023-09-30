namespace TradingConcepts.SecuritySpecifics.Options
{
    public class OptionDescription : SecurityDescription, IExpiring
    {
        public override Type SecurityType => typeof(IOption);
        public OptionTypes OptionType { get; init; }
        public DateTime Expiry { get; init; }
        public Decimal5 Strike { get; init; }
        public bool IsShortTermExpiry { get; init; }
        public TimeSpan TimeToExpiry => Expiry - DateTime.UtcNow;
    }
}
