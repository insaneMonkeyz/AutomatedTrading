namespace BasicConcepts
{

    public delegate void QuotesReader(Quote[] bids, Quote[] asks, long marketDepth);
    public delegate void OneSideQuotesReader(Quote[] quotes, Operations operation, long marketDepth);

    public interface IOptimizedOrderBook : IOrderBook
    {
        /// <summary>
        /// Provides an efficient way to operate on quotes arrays that avoids copying. <para/>
        /// Original quotes, not copies, are passed to the reader method.
        /// </summary>
        /// <param name="reader">A method that operates on quotes</param>
        void UseQuotes(QuotesReader reader, long marketDepth);
        void UseBids(OneSideQuotesReader reader);
        void UseAsks(OneSideQuotesReader reader);
    }
}