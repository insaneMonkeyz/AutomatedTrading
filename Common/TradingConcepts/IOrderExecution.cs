namespace TradingConcepts
{
    public interface IOrderExecution
    {
        Guid AccountId { get; }
        ISecurity Security { get; }
        IOrder Order { get; }
        IQuote Quote { get; }
        DateTimeOffset TimeStamp { get; }
    }
}
