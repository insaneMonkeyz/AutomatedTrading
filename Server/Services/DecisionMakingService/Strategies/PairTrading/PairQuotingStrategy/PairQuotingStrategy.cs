using TradingConcepts;

namespace DecisionMakingService.Strategies.PairTrading
{
    internal partial class PairQuotingStrategy : IConfigurable<PairQuotingStrategyConfiguration>, ITradingStrategyController, ITradingStrategy
    {
        public string Name => nameof(PairQuotingStrategy);
        public string Description => throw new NotImplementedException();
        public State State => throw new NotImplementedException();
        public IEnumerable<ISecurityBalance> Portfolio => throw new NotImplementedException();
        public IEnumerable<IOrderExecution> Executions => throw new NotImplementedException();
        public IEnumerable<IOrder> Orders => throw new NotImplementedException();
        public PairQuotingStrategyConfiguration? Configuration => throw new NotImplementedException();
        public bool IsEnabled => throw new NotImplementedException();

        public void Configure(PairQuotingStrategyConfiguration? config)
        {
            throw new NotImplementedException();
        }

        public void Deactivate()
        {
            throw new NotImplementedException();
        }

        public void Activate()
        {
            throw new NotImplementedException();
        }
    }
}
