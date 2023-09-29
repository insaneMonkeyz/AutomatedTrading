namespace TradingConcepts.SecuritySpecifics.Options
{
    public interface IOption : IDerivative, IExpiring
    {
        Decimal5 Strike { get; }
        OptionTypes OptionType { get; }
        double IV { get; }
        double Delta { get; }
        double Vega { get; }
        double Gamma { get; }
        double Theta { get; }
    }
}
