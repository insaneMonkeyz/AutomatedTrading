namespace TradingConcepts.SecuritySpecifics
{
    public class FuturesDescription : SecurityDescription<IFutures> 
    { 
        public DateTime ExpiryDate { get; init; }
    }
}
