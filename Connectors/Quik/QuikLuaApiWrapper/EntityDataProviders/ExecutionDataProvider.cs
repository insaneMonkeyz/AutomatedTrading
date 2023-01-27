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

        public List<OrderExecution> GetAllEntities()
        {
            return ReadWholeTable(ExecutionWrapper.NAME, Create);
        }

        public override void Update(OrderExecution entity)
        {
            throw new NotSupportedException("Updating order executions is not supported.");
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

        protected override void BuildEntityResolveRequest(LuaState state)
        {
            ExecutionWrapper.Set(state);

            _resolveEntityRequest.TradeId = ExecutionWrapper.TradeId;
            _resolveEntityRequest.ExchangeAssignedOrderId = ExecutionWrapper.ExchangeOrderId;
        }
        protected void BuildOrderResolveRequest(LuaState state)
        {
            ExecutionWrapper.Set(state);

            _orderRequestContainer.ExchangeAssignedId = ExecutionWrapper.ExchangeOrderId;
        }

        protected override void Update(OrderExecution entity, LuaState state)
        {
            throw new NotImplementedException();
        }

        #region Singleton
        [SingletonInstance]
        public static ExecutionDataProvider Instance { get; } = new();
        private ExecutionDataProvider() : base(EntityResolversFactory.GetOrderExecutionsResolver())
        {
            throw new NotImplementedException();

            _orderResolver = EntityResolversFactory.GetOrdersResolver();

            //_updateParams = new()
            //{
            //    Arg3 = DerivativesPositionsWrapper.LIMIT_TYPE,
            //    Method = DerivativesPositionsWrapper.GET_METOD,
            //    ReturnType = LuaApi.TYPE_TABLE,
            //    ActionParams = new() { Arg1 = State },
            //    Action = Update,
            //};
        }
        #endregion
    }
}
