using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingConcepts;
using TradingConcepts.CommonImplementations;
using TradingConcepts.SecuritySpecifics;

namespace Quik.Entities
{
    public class MoexOrderSubmission : IOrderSubmission
    {
#pragma warning disable CS8618
        private readonly Security _security;
        private string _accountCode;
#pragma warning restore CS8618
        internal Security Security => _security;
        public string? ClientCode { get; init; }
        public required string AccountCode 
        {
            get => _accountCode; 
            init => _accountCode = value ?? throw new ArgumentNullException(nameof(value)); 
        }
        public required IQuote Quote { get; init; }
        public required long TransactionId { get; init; }

        public bool IsMarket { get; init; }
        public OrderExecutionConditions ExecutionCondition { get; init; }
        public DateTimeOffset Expiry { get; init; }
        public TimeSpan TimeToExpiry => DateTime.UtcNow - Expiry.UtcDateTime;

        ISecurity IOrderSubmission.Security
        {
            get => _security;
            init
            {
                _security = Helper.CastToMoexSecurity(value);
            }
        }

        public MoexOrderSubmission(ISecurity moexSecurity)
        {
            _security = Helper.CastToMoexSecurity(moexSecurity);
        }
    }
}
