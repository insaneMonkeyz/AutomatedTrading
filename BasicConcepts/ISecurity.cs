namespace BasicConcepts
{
    public interface ISecurity
    {
        string Ticker { get; }
        string Code { get; }
        string Description { get; }
    }
}