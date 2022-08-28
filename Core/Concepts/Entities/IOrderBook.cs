namespace Core.Concepts.Entities
{
    public interface IOrderBook
    {
        long MarketDepth { get; }
        ISecurity Security { get; }
        IQuote[] Bids { get; }
        IQuote[] Asks { get; }
    }
}