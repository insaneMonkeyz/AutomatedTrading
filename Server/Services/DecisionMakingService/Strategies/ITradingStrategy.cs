using TradingConcepts;

namespace DecisionMakingService.Strategies
{
    public interface ITradingStrategy
    {
        Guid Id { get; }
        string Name { get; }
        string Description { get; }
        State State { get; }
        IEnumerable<ISecurityBalance> Portfolio { get; }
        IEnumerable<IOrderExecution> Executions { get; }
        IEnumerable<IOrder> Orders { get; }
    }
}
