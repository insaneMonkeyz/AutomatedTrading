namespace Core.Concepts.Entities
{
    public interface IOrderExecution
    {
        ISecurity Security { get; }
        IOrder Order { get; }
        IQuote Quote { get; }
        DateTimeOffset TimeStamp { get; }
    }
}
