using AppComponents.Messaging.Results;
using DecisionMakingService.Strategies;

namespace DecisionMakingService
{
    internal partial class DecisionMakingService : IDecisionMakingService
    {
        IEnumerable<ITradingStrategy> IDecisionMakingService.Strategies => _runningStrategies;

        public Result AddStrategy(ITradingStrategyConfiguration parameters)
        {
            try
            {
                var strategy = StrategiesFactory.CreateStrategy(parameters);

                _runningStrategies.Add(strategy);

                return Result.Success(strategy);
            }
            catch (Exception e)
            {
                return Result.Error("Could not add a strategy", e.Message);
            }
        }

        public Result RemoveStrategy(ITradingStrategy strategy)
        {
            throw new NotImplementedException();
        }

        public Result Configure(ITradingStrategyConfiguration configuration)
        {
            throw new NotImplementedException();
        }

        private readonly List<ITradingStrategy> _runningStrategies = new(10);
    }
}
