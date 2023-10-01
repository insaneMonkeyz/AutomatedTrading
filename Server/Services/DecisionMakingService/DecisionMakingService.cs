using DecisionMakingService.Strategies;

namespace DecisionMakingService
{
    internal partial class DecisionMakingService : IDecisionMakingService
    {
        IEnumerable<ITradingStrategy> IDecisionMakingService.AvailableStrategies { get; }
        IEnumerable<ITradingStrategy> IDecisionMakingService.RunningStrategies => _runningStrategies;

        public void EmployStrategy(ITradingStrategyConfiguration parameters)
        {
        }

        public void RemoveStrategy(ITradingStrategy strategy)
        {
            throw new NotImplementedException();
        }

        private readonly List<ITradingStrategy> _runningStrategies = new(10);
    }
}
