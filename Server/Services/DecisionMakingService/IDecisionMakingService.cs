using AppComponents.Messaging.Results;
using DecisionMakingService.Strategies;

namespace DecisionMakingService
{
    public interface IDecisionMakingService
    {
        IEnumerable<ITradingStrategy> Strategies { get; }

        Result AddStrategy(ITradingStrategyConfiguration parameters);
        Result RemoveStrategy(ITradingStrategy strategy);
        Result Configure(ITradingStrategyConfiguration configuration);
    }
}
