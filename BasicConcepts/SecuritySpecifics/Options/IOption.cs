namespace BasicConcepts.SecuritySpecifics.Options
{
    public interface IOption : ISecurity, IExpiring
    {
        Decimal5 Strike { get; }
        ISecurity Underlying { get; }
        OptionTypes OptionType { get; }
    }
}
