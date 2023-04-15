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
using TradingConcepts;
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
        ITradingAccount? IQuik.Account
        {
            get => AccountsProvider.Instance.GetAllEntities().FirstOrDefault(acc => acc.IsMoneyAccount);
        }

        IEnumerable<IOrder> IQuik.GetOrders()
        {
            return OrdersProvider.Instance.GetAllEntities();
        }

        IEnumerable<SecurityDescription> IQuik.GetAvailableSecurities<TSecurity>() 
        {
            var type = typeof(TSecurity);

            static IEnumerable<SecurityDescription> createDescriptions(Type t, Func<string, SecurityDescription> tickerToDescription)
            {
                return SecuritiesProvider.Instance.GetAvailableSecuritiesOfType(t).Select(tickerToDescription);
            }

            return type switch
            {
                       IFutures => createDescriptions(type, Helper.InferFuturesFromTicker),
                        IOption => createDescriptions(type, Helper.InferOptionFromTicker),
                ICalendarSpread => createDescriptions(type, Helper.InferSpreadFromTicker),
                              _ => throw new NotSupportedException($"Descriptions for type {type} cannot be created. Type not supported")
            };
        }
        TSecurity? IQuik.GetSecurity<TSecurity>(string ticker) where TSecurity : default
        {
            return 
                (TSecurity?)EntityResolvers
                    .GetSecurityResolver()
                    .Resolve<TSecurity>(ticker);
        }

        IOrder IQuik.PlaceNewOrder(MoexOrderSubmission submission)
        {
#if TRACE
            this.Trace();
#endif
            return TransactionsProvider.Instance.PlaceNew(submission);
        }
        void IQuik.ChangeOrder(IOrder order, Decimal5 newPrice, long newSize)
        {
#if TRACE
            this.Trace();
#endif
            if (order is not Order moexOrder)
            {
                throw new InvalidOperationException("Requesting to change an order that does not belong to MOEX");
            }

            TransactionsProvider.Instance.Change(moexOrder, newPrice, newSize);
        }
        void IQuik.CancelOrder(IOrder order)
        {
#if TRACE
            this.Trace(); 
#endif
            if (order is not Order moexOrder)
            {
                throw new InvalidOperationException("Requesting to change an order that does not belong to MOEX");
            }

            TransactionsProvider.Instance.Cancel(moexOrder);
        }
    }
}
