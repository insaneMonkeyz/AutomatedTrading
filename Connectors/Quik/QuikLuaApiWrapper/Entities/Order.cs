using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicConcepts;

namespace Quik.Entities
{
    internal class Order : IOrder, INotifyEntityUpdated
    {
        private const int DEFAULT_EXECUTIONS_LIST_SIZE = 10;

        private Security _security;
        private string? _exchangeAssignedIdString;

        public Security Security
        {
            get => _security;
            init => _security = value ?? throw new ArgumentNullException(nameof(value));
        }
        public Quote Quote { get; init; }
        public List<OrderExecution> Executions { get; } = new(DEFAULT_EXECUTIONS_LIST_SIZE);

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
        public OrderExecutionModes ExecutionMode { get; init; }
        public DateTimeOffset Expiry { get; init; }
        public TimeSpan TimeToExpiry { get; }

        IQuote IOrder.Quote => Quote;
        ISecurity IOrder.Security => _security;
        IEnumerable<IOrderExecution> IOrder.Executions => Executions;

        public event Action Updated = delegate { };

        public override string ToString()
        {
            return $"{State} {ExecutionMode} {Security} {Quote} Executed={ExecutedSize} Remainder={RemainingSize}";
        }
    }
}
