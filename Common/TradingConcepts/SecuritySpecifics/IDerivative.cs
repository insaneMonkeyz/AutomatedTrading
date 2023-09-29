namespace TradingConcepts.SecuritySpecifics
{
    public interface IDerivative : ISecurity
    {
        ISecurity? Underlying { get; }
    }
}