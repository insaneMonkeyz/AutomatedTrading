namespace BasicConcepts
{
    public interface IDerivative : ISecurity
    {
        ISecurity Underlying { get; }
    }
}