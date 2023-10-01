using TradingConcepts;

namespace Strategies
{
    public class GrabStrategyParameters : ITradingStrategyParameters
    {
        public Guid Id { get; init; }
        public Operations Operation { get; set; }
        public Decimal5 TriggerPrice { get; set; }
        public long PositionLimit { get; set; }
    }
}
