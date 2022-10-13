namespace BasicConcepts
{
    public interface IOrderBook
    {
        long MarketDepth { get; set; }
        ISecurity Security { get; }
        /// <summary>
        /// Get a copy of bids array 
        /// </summary>
        Quote[] Bids { get; }
        /// <summary>
        /// Get a copy of asks array 
        /// </summary>
        Quote[] Asks { get; }
    }
}