namespace Core.Concepts.Entities
{
    public interface IQuote
    {
        Decimal5 Price { get; }
        Side Side { get; }
        long Size { get; }
    }
}