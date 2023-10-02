using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingConcepts;

namespace DecisionMakingService.Strategies.PairTrading
{
    public class PairQuotingStrategyConfiguration : ITradingStrategyConfiguration
    {
        public Guid Id { get; }
        public Operations Operation { get; init; }
        public Decimal5 TargetPriceOffset { get; }
        public int OffsetRange { get; set; } 
        public long OrderSize { get; set; }
        public long PositionLimit { get; set; }
        public required ITradingAccount Account { get; init; }
        public required ISecurity QuotedSecurity { get; init; }
        public required ISecurity BaseSecurity { get; init; }
    }
}
