using AppComponents.Messaging.Results;
using DecisionMakingService.Strategies;

namespace DecisionMakingService
{
    public interface IDecisionMakingService
    {
        IEnumerable<ITradingStrategy> RunningStrategies { get; }
        IEnumerable<ITradingStrategy> AvailableStrategies { get; }

        Result AddStrategy(ITradingStrategyConfiguration parameters);
        Result RemoveStrategy(ITradingStrategy strategy);
    }
}
