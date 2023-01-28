using Quik.Entities;
using Quik.EntityDataProviders.Attributes;
using Quik.EntityDataProviders.RequestContainers;
using Quik.EntityDataProviders.QuikApiWrappers;

using static Quik.QuikProxy;
using BasicConcepts;

namespace Quik.EntityDataProviders
{
    internal class ExecutionDataProvider : DataProvider<OrderExecution, OrderExecutionRequestContainer>
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

            _orderRequestContainer.ExchangeAssignedId = ExecutionWrapper.ExchangeOrderId;
        }

        #region Singleton
        [SingletonInstance]
        public static ExecutionDataProvider Instance { get; } = new();
        private ExecutionDataProvider() 
        {
            _orderResolver = EntityResolvers.GetOrdersResolver();
        }
        #endregion
    }
}
