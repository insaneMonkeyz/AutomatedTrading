namespace Core.Concepts.Entities
{
    public interface ISecurity
    {
        string Ticker { get; }
        string Code { get; }
        string Description { get; }
    }
}