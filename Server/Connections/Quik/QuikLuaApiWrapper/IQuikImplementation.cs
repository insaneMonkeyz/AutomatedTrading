using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public event Action<DateTime> Connected = delegate { };
        public event Action<DateTime> ConnectionLost = delegate { };

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
        public event Action<ITrade> NewTrade;

        public Guid Id { get; } = new("310A16CB-F3A7-4317-A7CB-B4E4F9CFC5A2");

        public bool IsConnected
        {
            get
            {
                lock (SyncRoot)
                {
                    return Lua.ExecSimpleFunction(ISCONNECTED_FUNC);
                }
            }
        }

        public ITradingAccount? DerivativesAccount
        {
            get => AccountsProvider.Instance.GetAllEntities().FirstOrDefault(acc => acc.IsMoneyAccount);
        }

        public IEnumerable<IOrder> GetOrders()
        {
            return OrdersProvider.Instance.GetAllEntities();
        }

        public IEnumerable<IOrderExecution> GetOrderExecutions()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SecurityDescription> GetAvailableSecurities(Type securityType)
        {
            static IEnumerable<SecurityDescription> createDescriptions(Type t, Func<string, SecurityDescription> tickerToDescription)
            {
                return SecuritiesProvider.Instance.GetAvailableSecuritiesOfType(t).Select(tickerToDescription);
            }

            if (securityType.Equals(typeof(IFutures)))
            {
                return createDescriptions(securityType, Helper.InferFuturesFromTicker);
            }
            if (securityType.Equals(typeof(IOption)))
            {
                return createDescriptions(securityType, Helper.InferOptionFromTicker);
            }
            if (securityType.Equals(typeof(ICalendarSpread)))
            {
                return createDescriptions(securityType, Helper.InferSpreadFromTicker);
            }

            throw new NotSupportedException($"Security type {securityType.Name} is not supported by Quik");
        }

        public IEnumerable<SecurityDescription> GetAvailableSecurities<TSecurity>() where TSecurity : ISecurity
        {
            var type = typeof(TSecurity);

            return GetAvailableSecurities(type);
        }

        public TSecurity? GetSecurity<TSecurity>(string ticker) where TSecurity : ISecurity
        {
            var resolver = EntityResolvers.GetSecurityResolver();
            var sec = resolver.Resolve<TSecurity>(ticker);

            return (TSecurity?)sec;
        }

        public IOrderBook? GetOrderBook(Type securityType, string? ticker)
        {
            if (securityType is null)
            {
                throw new ArgumentNullException(nameof(securityType));
            }
            if (ticker is null)
            {
                throw new ArgumentNullException(nameof(ticker));
            }

            string classcode;

            try
            {
                classcode = MoexSpecifics.SecurityTypesToClassCodes[securityType];
            }
            catch (Exception e)
            {
                _log.Error("Unsupported security type", e);
                throw new ArgumentException($"Unsupported security type {securityType.Name}");
            }

            var request = OrderbookRequestContainer.Create(classcode, ticker);

            return EntityResolvers.GetOrderbooksResolver().Resolve(ref request);
        }

        public IOrder CreateOrder(ISecurity? security, string? accountCode, ref Quote quote, 
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
            return CreateOrder(security, DerivativesAccount.AccountCode, ref quote, executionCondition);
        }

        public void PlaceNewOrder(IOrder? order)
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

        public IOrder? ChangeOrder(IOrder? order, Decimal5 newPrice, long newSize)
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

        public void CancelOrder(IOrder? order)
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

        public IEnumerable<ITrade> GetTrades()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ISecurityBalance> GetSecuritiesBalances()
        {
            throw new NotImplementedException();
        }
    }
}
