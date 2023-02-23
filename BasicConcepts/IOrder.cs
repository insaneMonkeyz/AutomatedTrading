using TradingConcepts.SecuritySpecifics;

namespace TradingConcepts
{
    public interface IOrder : IOrderSubmission
    {
        DateTimeOffset Submitted { get; }
        long ExchangeAssignedId { get; }
        OrderStates State { get; }
        long RemainingSize { get; }
        long ExecutedSize { get; }
        IEnumerable<IOrderExecution> Executions { get; }
    }
}
