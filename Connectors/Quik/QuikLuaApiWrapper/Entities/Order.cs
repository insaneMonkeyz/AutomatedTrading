using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingConcepts;
using TradingConcepts.CommonImplementations;

namespace Quik.Entities
{
    internal class Order : IOrder, INotifyEntityUpdated
    {
        private const int DEFAULT_EXECUTIONS_LIST_SIZE = 10;

        private Security _security;
        private string? _exchangeAssignedIdString;

        public required Security Security
        {
            get => _security;
            init => _security = value ?? throw new ArgumentNullException(nameof(value));
        }
        public Quote Quote { get; init; }
        public List<OrderExecution> Executions { get; } = new(DEFAULT_EXECUTIONS_LIST_SIZE);

        public string AccountCode { get; init; }
        public string ClientCode { get; init; }
        public OrderStates State { get; set; }
        public string? ExchangeAssignedIdString 
        { 
            get => _exchangeAssignedIdString; 
            internal set
            {
                _exchangeAssignedIdString = value ?? throw new ArgumentNullException(nameof(value));
                ExchangeAssignedId = long.Parse(value);
            }
        }
        public long ExchangeAssignedId { get; private set; }
        public long TransactionId { get; init; }
        public bool IsLimit { get; init; }
        public long RemainingSize { get; set; }
        public long ExecutedSize => Quote.Size - RemainingSize;
        public OrderExecutionConditions ExecutionCondition { get; init; }
        public DateTimeOffset Expiry { get; init; }
        public TimeSpan TimeToExpiry { get; }
        public DateTimeOffset Submitted { get; set; }

        IEnumerable<IOrderExecution> IOrder.Executions => Executions;
        ISecurity IOrderSubmission.Security => Security;
        IQuote IOrderSubmission.Quote => Quote;

        public event Action Updated = delegate { };

        public override string ToString()
        {
            return $"{State} {ExecutionCondition} {Security} {Quote} Executed={ExecutedSize} Remainder={RemainingSize}";
        }
    }
}
