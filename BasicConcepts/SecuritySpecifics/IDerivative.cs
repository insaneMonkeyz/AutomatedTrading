namespace BasicConcepts.SecuritySpecifics
{
    public interface IDerivative : ISecurity
    {
        ISecurity? Underlying { get; }
    }
}