﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Quik.Entities;
using Quik.EntityProviders;
using Quik.EntityProviders.RequestContainers;
using Quik.EntityProviders.Resolvers;
using Quik.Lua;
using Tools;
using TradingConcepts;
using TradingConcepts.CommonImplementations;
using TradingConcepts.SecuritySpecifics;
using TradingConcepts.SecuritySpecifics.Options;

namespace Quik
{
    internal partial class Quik : IQuik
    {
        public event Action<DateTimeOffset> Connected = delegate { };
        public event Action<DateTimeOffset> ConnectionLost = delegate { };

        public event Action<ISecurityBalance> NewSecurityBalance = delegate { };
        public event Action<IOrderExecution> NewOrderExecution = delegate { };
        public event Action<ITradingAccount> NewAccount = delegate { };
        public event Action<IOrderBook> NewOrderBook = delegate { };
        public event Action<ISecurity> NewSecurity = delegate { };
        public event Action<IOrder> NewOrder = delegate { };

        public event Action<ISecurityBalance> SecurityBalanceChanged = delegate { };
        public event Action<ITradingAccount> AccountChanged = delegate { };
        public event Action<IOrderBook> OrderBookChanged = delegate { };
        public event Action<ISecurity> SecurityChanged = delegate { };
        public event Action<IOrder> OrderChanged = delegate { };

        public event Action<IOrder> OrderChangeDenied = delegate { };
        public event Action<IOrder> OrderCancellationDenied = delegate { };

        Guid IQuik.Id { get; } = new("310A16CB-F3A7-4317-A7CB-B4E4F9CFC5A2");

        bool IQuik.IsConnected
        {
            get
            {
                lock (SyncRoot)
                {
                    return Lua.ExecSimpleFunction(ISCONNECTED_FUNC);
                }
            }
        }
        ITradingAccount? IQuik.DerivativesAccount
        {
            get => AccountsProvider.Instance.GetAllEntities().FirstOrDefault(acc => acc.IsMoneyAccount);
        }

        IEnumerable<IOrder> IQuik.GetOrders()
        {
            return OrdersProvider.Instance.GetAllEntities();
        }
        IEnumerable<IOrderExecution> IQuik.GetOrderExecutions()
        {
            throw new NotImplementedException();
        }
        IEnumerable<SecurityDescription> IQuik.GetAvailableSecurities<TSecurity>() 
        {
            var type = typeof(TSecurity);

            static IEnumerable<SecurityDescription> createDescriptions(Type t, Func<string, SecurityDescription> tickerToDescription)
            {
                return SecuritiesProvider.Instance.GetAvailableSecuritiesOfType(t).Select(tickerToDescription);
            }

            if (type.Equals(typeof(IFutures)))
            {
                return createDescriptions(type, Helper.InferFuturesFromTicker);
            }
            if (type.Equals(typeof(IOption)))
            {
                return createDescriptions(type, Helper.InferOptionFromTicker);
            }
            if (type.Equals(typeof(ICalendarSpread)))
            {
                return createDescriptions(type, Helper.InferSpreadFromTicker);
            }

            throw new NotSupportedException($"Descriptions for type {type} cannot be created. Type not supported");
        }
        TSecurity? IQuik.GetSecurity<TSecurity>(string ticker) where TSecurity : default
        {
            var resolver = EntityResolvers.GetSecurityResolver();
            var sec = resolver.Resolve<TSecurity>(ticker);

            return (TSecurity?)sec;
        }
        IOrderBook? IQuik.GetOrderBook<TSecurity>(string ticker)
        {
            string classcode;

            try
            {
                classcode = MoexSpecifics.SecurityTypesToClassCodes[typeof(TSecurity)];
            }
            catch (Exception e)
            {
                _log.Error("Unsupported security type", e);
                throw new ArgumentException($"Unsupported security type {typeof(TSecurity)}");
            }

            var request = OrderbookRequestContainer.Create(classcode, ticker);

            return EntityResolvers.GetOrderbooksResolver().Resolve(ref request);
        }

        IOrder IQuik.CreateOrder(ISecurity? security, string? accountCode, ref Quote quote, 
            OrderExecutionConditions? executionCondition = null, DateTime? expiry = null)
        {
            if (security is null)
            {
                throw new ArgumentNullException(nameof(security));
            }
            if (accountCode is null)
            {
                throw new ArgumentNullException(nameof(accountCode));
            }

            return new Order(security)
            {
                TransactionId = TransactionIdGenerator.CreateId(),
                ExecutionCondition = executionCondition.GetValueOrDefault(),
                Expiry = expiry.GetValueOrDefault(),
                AccountCode = accountCode ?? throw new ArgumentNullException(nameof(accountCode)),
                Quote = quote,
            };
        }
        public IOrder CreateDerivativeOrder(IDerivative security, ref Quote quote, 
            OrderExecutionConditions executionCondition = OrderExecutionConditions.Session)
        {
            var iquik = this as IQuik;
            return iquik.CreateOrder(security, iquik.DerivativesAccount.AccountCode, ref quote, executionCondition);
        }

        void IQuik.PlaceNewOrder(IOrder? order)
        {
#if TRACE
            this.Trace();
#endif
            if (order is not Order moexOrder)
            {
                throw new ArgumentException("The order is not a MOEX order");
            }
            if (order.Quote.Operation == Operations.Undefined)
            {
                throw new InvalidOperationException("Order operation is not defined");
            }

            TransactionsProvider.Instance.PlaceNew(moexOrder);
        }
        IOrder? IQuik.ChangeOrder(IOrder? order, Decimal5 newPrice, long newSize)
        {
#if TRACE
            this.Trace();
#endif
            if (order is not Order moexOrder)
            {
                throw new ArgumentException("The order is not a MOEX order");
            }

            if (order.State != OrderStates.Active)
            {
                throw new InvalidOperationException($"Cannot change inactive order {order}");
            }

            if (order.ExchangeAssignedId == default || order.State.HasFlag(OrderStates.Registering))
            {
                throw new InvalidOperationException($"Cannot change an order that is still registering {order}");
            }

            return TransactionsProvider.Instance.Change(moexOrder, newPrice, newSize);
        }
        void IQuik.CancelOrder(IOrder? order)
        {
#if TRACE
            this.Trace(); 
#endif
            if (order is not Order moexOrder)
            {
                throw new InvalidOperationException("The order is not a MOEX order");
            }

            if (order.State != OrderStates.Active)
            {
                throw new InvalidOperationException($"Cannot cancel inactive order {order}");
            }

            TransactionsProvider.Instance.Cancel(moexOrder);
        }

    }
}
