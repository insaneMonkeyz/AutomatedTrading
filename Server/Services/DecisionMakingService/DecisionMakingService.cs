using AppComponents.Messaging.Results;
using DecisionMakingService.Strategies;

namespace DecisionMakingService
{
    internal partial class DecisionMakingService : IDecisionMakingService
    {
        IEnumerable<ITradingStrategy> IDecisionMakingService.Strategies => _strategies;

        public Result AddStrategy(ITradingStrategyConfiguration parameters)
        {
            try
            {
                var strategy = StrategiesFactory.CreateStrategy(parameters);

                _strategies.Add(strategy);

                (strategy as ITradingStrategyController).Activate();

                return Result.Success(strategy);
            }
            catch (Exception e)
            {
                return Result.Error("Could not add a strategy", e.Message);
            }
        }

        public Result RemoveStrategy(Guid id)
        {
            var strategy = _strategies.FirstOrDefault(s => s.Id == id);

            if (strategy is null)
            {
                return Result.Error($"Strategy with id '{id}' not found");
            }

            try
            {
                (strategy as ITradingStrategyController).Deactivate();
            }
            catch (Exception e)
            {
                return Result.Error($"Cannot deactivate the strategy '{strategy}' before removing. Try again later");
            }

            if (!_strategies.Remove(strategy))
            {
                return Result.Error($"Strategy '{strategy}' is found, deactivated, but for some reason cannot be deleted");
            }

            return Result.Success();
        }

        public Result ConfigureStrategy<T>(T configuration) where T : ITradingStrategyConfiguration
        {
            var strategy = _strategies.FirstOrDefault(s => s.Id == configuration.Id);

            if (strategy is null)
            {
                return Result.Error($"Strategy for this configuration is not found. Id '{configuration.Id}'");
            }

            if (strategy is not IConfigurable<T> configurableStrategy)
            {
                var expectedType = strategy
                    .GetType()
                    .GetInterfaces()
                    .FirstOrDefault(
                        i => i.GenericTypeArguments.Contains(typeof(ITradingStrategyConfiguration)) && 
                             i.Name.StartsWith("IConfigurable"))?
                    .GenericTypeArguments[0];

                return Result.Error($"The strategy with id '{configuration.Id}' expects configuration of type {expectedType.Name} while" +
                    $"ptovided configuration is of type {typeof(T).Name}");
            }

            configurableStrategy.Configure(configuration);

            return Result.Success();
        }

        private readonly List<ITradingStrategy> _strategies = new(10);
    }
}
