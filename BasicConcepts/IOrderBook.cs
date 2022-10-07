namespace BasicConcepts
{
    public interface IOrderBook
    {
        long MarketDepth { get; }
        ISecurity Security { get; }
        IQuote[] Bids { get; }
        IQuote[] Asks { get; }
    }
}