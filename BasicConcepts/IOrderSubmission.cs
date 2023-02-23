using TradingConcepts.SecuritySpecifics;

namespace TradingConcepts
{
    public interface IOrderSubmission : IExpiring
    {
        string AccountCode { get; }
        long TransactionId { get; }
        ISecurity Security { get; }
        IQuote Quote { get; }
        bool IsLimit { get; }
        OrderExecutionConditions ExecutionCondition { get; }
    }
}
