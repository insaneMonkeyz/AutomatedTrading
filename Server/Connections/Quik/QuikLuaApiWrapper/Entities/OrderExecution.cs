﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quik.EntityProviders.Notification;
using TradingConcepts;
using TradingConcepts.CommonImplementations;

namespace Quik.Entities
{
    internal class OrderExecution : IOrderExecution, INotifiableEntity
    {
        public long TradeId { get; init; }
        public long OrderId { get; init; }
        public string AccountId { get; init; }
        public string ClientCode { get; init; }
        public Order Order { get; }
        public Security Security { get; }
        public Quote Quote { get; init; }
        public DateTimeOffset TimeStamp { get; init; }
        public event Action Updated = delegate { };

        Guid IOrderExecution.AccountId => throw new NotImplementedException();
        IQuote IOrderExecution.Quote => Quote;
        IOrder IOrderExecution.Order => Order;
        ISecurity IOrderExecution.Security => Security;

        public OrderExecution(Order order)
        {
            Order = order ?? throw new ArgumentNullException(nameof(order));
            Security = order.Security;
            OrderId = order.ExchangeAssignedId;
        }

        public void NotifyUpdated() => Updated();

        public override string ToString()
        {
            return $"{TimeStamp:O} {Security} {Quote} OrderId:{OrderId}";
        }
    }
}
