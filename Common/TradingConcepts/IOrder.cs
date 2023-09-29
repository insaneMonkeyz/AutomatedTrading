using TradingConcepts.CommonImplementations;
using TradingConcepts.SecuritySpecifics;

namespace TradingConcepts
{
    public interface IOrder : IExpiring
    {
        string AccountCode { get; }
        long TransactionId { get; }
        ISecurity Security { get; }
        Quote Quote { get; }
        OrderExecutionConditions ExecutionCondition { get; }
        DateTime ServerCompletionTime { get; }
        DateTime CompletionReplyTime { get; }
        DateTime SubmittedTime { get; }
        DateTime SubmissionReplyTime { get; }
        long ExchangeAssignedId { get; }
        OrderStates State { get; }
        long RemainingSize { get; }
        long ExecutedSize { get; }
        IEnumerable<IOrderExecution> Executions { get; }
        IOrder? ParentOrder { get; }
    }
}
