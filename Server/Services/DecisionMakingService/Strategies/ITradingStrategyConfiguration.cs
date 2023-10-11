namespace DecisionMakingService.Strategies
{
    public interface ITradingStrategyConfiguration
    {
        Guid Id { get; }
        bool IsEnabled { get; }
    }
}
