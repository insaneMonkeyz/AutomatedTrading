namespace BasicConcepts.SecuritySpecifics.Options
{
    public interface IOption : IDerivative, IExpiring
    {
        Decimal5 Strike { get; }
        OptionTypes OptionType { get; }
    }
}
