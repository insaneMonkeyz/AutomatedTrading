using MarketDataProvisionService;
using MarketExecutionService;
using Tools;
using Tools.Logging;
using TradingConcepts;
using TradingConcepts.CommonImplementations;

namespace DecisionMakingService.Strategies
{
    internal partial class GrabStrategy : ITradingStrategyController, ITradingStrategy, IConfigurable<GrabStrategyConfiguration>
    {
        public GrabStrategyConfiguration? Configuration { get; private set; }
        public Guid Id { get; private set; }
        public string Name => throw new NotImplementedException();
        public string Description => throw new NotImplementedException();
        public State State { get; private set; }
        public bool IsEnabled => throw new NotImplementedException();
        public IEnumerable<ISecurityBalance> Portfolio => throw new NotImplementedException();
        public IEnumerable<IOrderExecution> Executions => throw new NotImplementedException();
        public IEnumerable<IOrder> Orders => throw new NotImplementedException();

        public void Configure(GrabStrategyConfiguration? config)
        {
            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (Id != Guid.Empty && config.Id != Id)
            {
                throw new InvalidOperationException("Configuration belongs to a different strategy. " +
                    $"Configuration target={config.Id}, configuring strategy={Id}");
            }

            if (Configuration?.Security is not null)
            {
                if (!Configuration.Security.Equals(config.Security))
                {
                    throw new InvalidOperationException("Cannot change security once it is set"); 
                }
            }
            else
            {
                _bookOperator = new(this, config.Security);
            }

            var prevstate = State;

            _log.Info($"Grab strategy {Id} disabled for configuration");

            State = State.Configuring;

            Configuration = config;

            State = prevstate;

            _log.Info($"Grab strategy {Id} configured");
        }
        public void Activate()
        {
            if (Configuration is null)
            {
                throw new InvalidOperationException("The strategy needs to be configured first");
            }
            if (State != State.Off)
            {
                return;
            }

            State = State.Running;

            _bookOperator.IsEnabled = true;
        }
        public void Deactivate()
        {
            throw new NotImplementedException();

            _bookOperator.IsEnabled = false;
        }
    }
}
