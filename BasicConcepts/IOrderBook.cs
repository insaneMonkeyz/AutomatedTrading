using TradingConcepts.CommonImplementations;

namespace TradingConcepts
{
    public interface IOrderBook
    {
        DateTime LastTimeUpdated { get; }
        long MarketDepth { get; set; }
        ISecurity Security { get; }
        Quote[] Bids { get; }
        Quote[] Asks { get; }
    }
}