namespace TradingConcepts.SecuritySpecifics.Options
{
    public class OptionDescription : SecurityDescription
    {
        public override Type SecurityType => typeof(IOption);
        public OptionTypes OptionType { get; init; }
        public DateTime ExpiryDate { get; init; }
        public Decimal5 Strike { get; init; }
        public bool IsShortTermExpiry { get; init; }
    }
}
