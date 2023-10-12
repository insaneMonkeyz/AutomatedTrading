using AppComponents.Messaging.Results;
using DecisionMakingService.Strategies;

namespace DecisionMakingService
{
    public interface IDecisionMakingService
    {
        IEnumerable<ITradingStrategy> Strategies { get; }

        Result AddStrategy(ITradingStrategyConfiguration parameters);
        Result RemoveStrategy(Guid strategyId);
        Result ConfigureStrategy<T>(T configuration) where T : ITradingStrategyConfiguration;
    }
}
