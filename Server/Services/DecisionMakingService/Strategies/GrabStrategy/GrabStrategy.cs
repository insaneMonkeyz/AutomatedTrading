using MarketDataProvisionService;
using MarketExecutionService;
using Tools;
using Tools.Logging;
using TradingConcepts;
using TradingConcepts.CommonImplementations;

namespace DecisionMakingService.Strategies
{
    internal partial class GrabStrategy : ITradingStrategy, IConfigurable<GrabStrategyConfiguration>
    {
        public GrabStrategyConfiguration? Configuration { get; private set; }
        public Guid Id => throw new NotImplementedException();
        public string Name => throw new NotImplementedException();
        public string Description => throw new NotImplementedException();
        public State State { get; private set; }
        public IEnumerable<ISecurityBalance> Portfolio => throw new NotImplementedException();
        public IEnumerable<IOrderExecution> Executions => throw new NotImplementedException();
        public IEnumerable<IOrder> Orders => throw new NotImplementedException();

        public void Configure(GrabStrategyConfiguration? config)
        {
            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            var prevstate = State;
            var prevsecurity = Configuration.Security;

            _log.Info($"Grab strategy {Id} disabled for configuration");

            State = State.Configuring;

            Configuration = config;

            if (prevsecurity != config.Security)
            {
                ResolveBookReader(); 
            }

            State = prevstate;

            _log.Info($"Grab strategy {Id} configured");
        }
        public void Enable()
        {
            if (Configuration is null)
            {
                throw new InvalidOperationException("The strategy needs to be configured first");
            }
            if (State != State.Disabled)
            {
                return;
            }

            State = State.Enabled;
        }
        public void Disable()
        {
            throw new NotImplementedException();
        }
    }
}
