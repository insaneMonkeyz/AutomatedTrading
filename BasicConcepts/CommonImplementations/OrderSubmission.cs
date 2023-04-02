namespace TradingConcepts.CommonImplementations
{
    public class OrderSubmission : IOrderSubmission
    {
#pragma warning disable CS8618
        private ISecurity _security;
#pragma warning restore CS8618
        public required string AccountCode { get; init; }
        public required virtual ISecurity Security
        {
            get => _security;
            init => _security = value ?? throw new ArgumentNullException(nameof(value));
        }
        public required IQuote Quote { get; init; }
        public required long TransactionId { get; init; }

        public bool IsMarket { get; init; }
        public OrderExecutionConditions ExecutionCondition { get; init; }
        public DateTimeOffset Expiry { get; init; }
        public TimeSpan TimeToExpiry => DateTime.UtcNow - Expiry.UtcDateTime;
    }
}
