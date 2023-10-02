using TradingConcepts;

namespace DecisionMakingService.Strategies.PairTrading
{
    internal partial class PairQuotingStrategy : IConfigurable<PairQuotingStrategyConfiguration>, ITradingStrategy
    {
        public string Name => nameof(PairQuotingStrategy);
        public string Description => throw new NotImplementedException();
        public State State => throw new NotImplementedException();
        public IEnumerable<ISecurityBalance> Portfolio => throw new NotImplementedException();
        public IEnumerable<IOrderExecution> Executions => throw new NotImplementedException();
        public IEnumerable<IOrder> Orders => throw new NotImplementedException();
        public PairQuotingStrategyConfiguration? Configuration => throw new NotImplementedException();

        public void Configure(PairQuotingStrategyConfiguration? config)
        {
            throw new NotImplementedException();
        }

        public void Disable()
        {
            throw new NotImplementedException();
        }

        public void Enable()
        {
            throw new NotImplementedException();
        }
    }
}
