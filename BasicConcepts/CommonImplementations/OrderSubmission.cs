namespace TradingConcepts.CommonImplementations
{
    public class OrderSubmission : IOrderSubmission
    {
        public required string AccountCode { get; init; }
        public required ISecurity Security { get; init; }
        public required IQuote Quote { get; init; }
        public required long TransactionId { get; init; }
        public bool IsLimit { get; init; }
        public OrderExecutionConditions ExecutionCondition { get; init; }
        public DateTimeOffset Expiry { get; init; }
        public TimeSpan TimeToExpiry => DateTime.UtcNow - Expiry.UtcDateTime;
    }
}
