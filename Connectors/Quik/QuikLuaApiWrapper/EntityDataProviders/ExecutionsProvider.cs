﻿using Quik.Entities;
using Quik.EntityProviders.Attributes;
using Quik.EntityProviders.RequestContainers;
using Quik.EntityProviders.QuikApiWrappers;

using static Quik.Quik;
using TradingConcepts;
using System.Runtime.CompilerServices;
using Quik.Lua;
using TradingConcepts.CommonImplementations;
using Quik.EntityProviders.Resolvers;

namespace Quik.EntityProviders
{
    internal class ExecutionsProvider : DataProvider<OrderExecution, OrderExecutionRequestContainer>
    {
        protected readonly EntityResolver<OrderRequestContainer, Order> _orderResolver;

        protected override string QuikCallbackMethod => ExecutionWrapper.CALLBACK_METHOD;
        protected override string AllEntitiesTable => ExecutionWrapper.NAME;
        protected override Action<LuaWrap> SetWrapper => ExecutionWrapper.Set;

        public override OrderExecution? Create(ref OrderExecutionRequestContainer request)
        {
#if TRACE
            this.Trace();
#endif
            throw new NotImplementedException("Need to implement 'SearchItems' from quik API in order " +
                "to be able to fetch random order executions from server.");
        }
        protected override OrderExecution? Create(LuaWrap state)
        {
#if TRACE
            this.Trace();
#endif
            lock (ExecutionWrapper.Lock)
            {
                ExecutionWrapper.Set(state);

                if (ResolveOrderOfExecution(state) is not Order order)
                {
                    $"Coudn't resolve order with id={ExecutionWrapper.ExchangeOrderId ?? "null"} to create an execution entity."
                        .DebugPrintWarning();

                    return null;
                }

                var result = new OrderExecution(order)
                {
                    TimeStamp = ExecutionWrapper.Timestamp,
                    TradeId = ExecutionWrapper.TradeId,
                    Quote = new Quote
                    {
                        Operation = ExecutionWrapper.Operation,
                        Price = ExecutionWrapper.Price,
                        Size = ExecutionWrapper.Size
                    }
                }; 

                order.Executions.Add(result);

                return result;
            }
        }
        protected override OrderExecutionRequestContainer CreateRequestFrom(LuaWrap state)
        {
#if TRACE
            this.Trace();
#endif
            return new OrderExecutionRequestContainer
            {
                TradeId = ExecutionWrapper.TradeId,
                ExchangeAssignedOrderId = ExecutionWrapper.ExchangeOrderId
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Order? ResolveOrderOfExecution(LuaWrap state)
        {
#if TRACE
            this.Trace();
#endif
            var request = new OrderRequestContainer
            {
                ClassCode = ExecutionWrapper.ClassCode,
                TransactionId = ExecutionWrapper.TransactionId,
                ExchangeAssignedId = ExecutionWrapper.ExchangeOrderId
            };
            return _orderResolver.Resolve(ref request);
        }

        #region Singleton
        [SingletonInstance(rank: -1)]
        public static ExecutionsProvider Instance { get; } = new();
        private ExecutionsProvider() 
        {
            _orderResolver = EntityResolvers.GetOrdersResolver();
        }
        #endregion
    }
}
