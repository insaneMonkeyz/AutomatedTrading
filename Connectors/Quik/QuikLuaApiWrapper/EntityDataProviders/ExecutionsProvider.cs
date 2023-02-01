using Quik.Entities;
using Quik.EntityProviders.Attributes;
using Quik.EntityProviders.RequestContainers;
using Quik.EntityProviders.QuikApiWrappers;

using static Quik.QuikProxy;
using BasicConcepts;

namespace Quik.EntityProviders
{
    internal class ExecutionsProvider : DataProvider<OrderExecution, OrderExecutionRequestContainer>
    {
        protected readonly OrderRequestContainer _orderRequestContainer = new();
        protected readonly EntityResolver<OrderRequestContainer, Order> _orderResolver;

        protected override string QuikCallbackMethod => ExecutionWrapper.CALLBACK_METHOD;
        protected override string AllEntitiesTable => ExecutionWrapper.NAME;

        public override OrderExecution? Create(OrderExecutionRequestContainer request)
        {
            throw new NotImplementedException("Need to implement 'SearchItems' from quik API in order " +
                "to be able to fetch random order executions from server.");
        }
        protected override OrderExecution? Create(LuaState state)
        {
            BuildOrderResolveRequest(state);

            if (_orderResolver.GetEntity(_orderRequestContainer) is not Order order)
            {
                $"Coudn't resolve order with id={_orderRequestContainer} to create an execution entity."
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
        protected void BuildOrderResolveRequest(LuaState state)
        {
            ExecutionWrapper.Set(state);

            _orderRequestContainer.ExchangeAssignedId = ExecutionWrapper.ExchangeOrderId.ToString();
        }

        #region Singleton
        [SingletonInstance]
        public static ExecutionsProvider Instance { get; } = new();
        private ExecutionsProvider() 
        {
            _orderResolver = EntityResolvers.GetOrdersResolver();
        }
        #endregion
    }
}
