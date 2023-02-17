namespace BasicConcepts 
{ 
    public enum OrderExecutionConditions
    {
        Session,
        GoodTillCancelled,
        GoodTillDate,
        FillOrKill,
        CancelRest
    }
}