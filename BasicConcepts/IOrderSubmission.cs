using TradingConcepts.SecuritySpecifics;

namespace TradingConcepts
{
    public interface IOrderSubmission : IExpiring
    {
        string AccountCode { get; init; }
        long TransactionId { get; init; }
        ISecurity Security { get; init; }
        IQuote Quote { get; init; }
        bool IsMarket { get; init; }
        OrderExecutionConditions ExecutionCondition { get; init; }
    }
}
