using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicConcepts;

namespace Quik.Entities
{
    internal class OrderExecution : IOrderExecution
    {
        public long TradeId { get; init; }
        public long OrderId { get; init; }
        public string AccountId { get; init; }
        public string ClientCode { get; init; }

        Guid IOrderExecution.AccountId => throw new NotImplementedException();
        IQuote IOrderExecution.Quote => Quote;
        public IOrder Order { get; }
        public ISecurity Security { get; }
        public Quote Quote { get; init; }
        public DateTimeOffset TimeStamp { get; init; }

        public OrderExecution(IOrder order)
        {
            Order = order ?? throw new ArgumentNullException(nameof(order));
            Security = order.Security;
            OrderId = order.ExchangeAssignedId;
        }

        public override string ToString()
        {
            return $"{TimeStamp:O} {Security} {Quote} OrderId:{OrderId}";
        }
    }
}
