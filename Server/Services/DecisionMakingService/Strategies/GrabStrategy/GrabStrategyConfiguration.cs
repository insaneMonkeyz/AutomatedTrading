using TradingConcepts;

namespace DecisionMakingService.Strategies
{
    public class GrabStrategyConfiguration : ITradingStrategyConfiguration
    {
        public Guid Id { get; }
        public Decimal5 TriggerPrice { get; }
        public Operations Operation { get; init; }
        public int MaxOrderSize { get; set; } = int.MaxValue;
        public int OrderPriceOffset { get; set; }
        public required ITradingAccount Account { get; init; }
        public required ISecurity Security { get; init; }

        public GrabStrategyConfiguration(ISecurity? security, ITradingAccount? account, Operations operation)
        {
            Operation = operation != Operations.Undefined
                ? operation
                : throw new ArgumentException("Please specify whether the strategy will be selling or buying");

            Security = security ?? throw new ArgumentNullException(nameof(security));
            Account = account ?? throw new ArgumentNullException(nameof(account));
        }
    }
}
