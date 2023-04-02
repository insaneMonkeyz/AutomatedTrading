namespace TradingConcepts.SecuritySpecifics.Options
{
    public class OptionDescription : SecurityDescription<IOption>
    {
        public OptionTypes OptionType { get; init; }
        public DateTime ExpiryDate { get; init; }
        public Decimal5 Strike { get; init; }
    }
}
