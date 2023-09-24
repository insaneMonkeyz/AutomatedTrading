namespace TradingConcepts 
{ 
    public enum OrderExecutionConditions
    {
        Session,
        GoodTillCancelled,
        GoodTillDate,
        FillOrKill,
        CancelRest,
        Market
    }
}