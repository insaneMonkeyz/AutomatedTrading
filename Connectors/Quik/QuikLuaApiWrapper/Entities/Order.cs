using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicConcepts;

namespace Quik.Entities
{
    internal class Order : IOrder
    {
        private SecurityBase _security;
        private string _orderId;

        public SecurityBase Security
        {
            get => _security;
            init => _security = value ?? throw new ArgumentNullException(nameof(value));
        }
        public OrderQuote Quote { get; init; }
        public List<IOrderExecution> Executions { get; }

        public OrderStates State { get; set; }
        public string IdString
        {
            get => _orderId ??= Id.ToString();
        }
        public long Id { get; init; }
        public bool IsLimit { get; init; }
        public long RemainingSize { get; set; }
        public long ExecutedSize => Quote.Size - RemainingSize;
        public OrderExecutionModes ExecutionMode { get; init; }
        public DateTimeOffset Expiry { get; init; }
        public TimeSpan TimeToExpiry { get; }

        IQuote IOrder.Quote => Quote;
        ISecurity IOrder.Security => _security;
        IEnumerable<IOrderExecution> IOrder.Executions => Executions;
    }
}
