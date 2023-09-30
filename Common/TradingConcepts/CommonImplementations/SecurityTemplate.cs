using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingConcepts.CommonImplementations
{
    public struct SecurityTemplate
    {
        public Guid? ExchangeId;
        public Type? SecurityType;
        public string? TickerTemplate;
        public DateTime? ExpiringFrom;
        public DateTime? ExpiringUntil;
        public Decimal5? MinStrike;
        public Decimal5? MaxStrike;

        public bool HasStrikeConstraints() => MinStrike != null || MaxStrike != null;
        public bool HasExpirationConstraints() => ExpiringUntil != null || ExpiringFrom != null;
    }
}
