using DecisionMakingService.Strategies;

namespace DecisionMakingService
{
    public interface IDecisionMakingService
    {
        IEnumerable<ITradingStrategy> RunningStrategies { get; }
        IEnumerable<ITradingStrategy> AvailableStrategies { get; }

        void EmployStrategy(ITradingStrategyConfiguration parameters);
        void RemoveStrategy(ITradingStrategy strategy);
    }
}
