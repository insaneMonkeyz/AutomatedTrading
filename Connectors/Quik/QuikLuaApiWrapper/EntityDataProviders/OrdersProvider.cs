using System.Runtime.CompilerServices;

using BasicConcepts;

using Quik.Entities;
using Quik.EntityProviders.Attributes;
using Quik.EntityProviders.QuikApiWrappers;
using Quik.EntityProviders.RequestContainers;
using Quik.Lua;

using CreateParams = Quik.EntityProviders.QuikApiWrappers.FunctionsWrappers.MultiGetMethod2Params<Quik.Lua.LuaWrap, Quik.Entities.Order?>;
using UpdateParams = Quik.EntityProviders.QuikApiWrappers.FunctionsWrappers.VoidMultiGetMethod2Params<Quik.Entities.Order, Quik.Lua.LuaWrap>;

namespace Quik.EntityProviders
{
    internal class OrdersProvider : UpdatableEntitiesProvider<Order, OrderRequestContainer>
    {
        private readonly SecurityResolver _securityResolver = EntityResolvers.GetSecurityResolver();
        private UpdateParams _updateParams = new()
        {
            Method = OrdersWrapper.GET_METOD,
            ReturnType1 = Api.TYPE_TABLE,
            ReturnType2 = Api.TYPE_NUMBER,
            Callback = new()
            {
                Arg1 = default,
                Invoke = default
            }
        };
        private CreateParams _createParams = new()
        {
            Method = OrdersWrapper.GET_METOD,
            ReturnType1 = Api.TYPE_TABLE,
            ReturnType2 = Api.TYPE_NUMBER,
            Callback = new()
            {
                Arg = default,
                Invoke = default,
                DefaultValue = null
            }
        };

        protected override string QuikCallbackMethod => OrdersWrapper.CALLBACK_METHOD;
        protected override string AllEntitiesTable => OrdersWrapper.NAME;
        protected override Action<LuaWrap> SetWrapper => OrdersWrapper.Set;

        public override void Initialize(ExecutionLoop entityNotificationLoop)
        {
            _updateParams.Callback.Invoke = Update;
            _createParams.Callback.Invoke = Create;
            _updateParams.Callback.Arg1 = Quik.Lua;
            _createParams.Callback.Arg = Quik.Lua;

            base.Initialize(entityNotificationLoop);
        }

        public override Order? Create(ref OrderRequestContainer request)
        {
            base.Create(ref request);

            lock (_requestInProgressLock)
            {
                _createParams.Arg0 = request.ClassCode;
                _createParams.Arg1 = request.ExchangeAssignedId;

                return FunctionsWrappers.ReadSpecificEntry(ref _createParams);
            }
        }
        protected override Order? Create(LuaWrap state)
        {
            lock (OrdersWrapper.Lock)
            {
                OrdersWrapper.Set(state);

                if (ResolveSecurityOfOrder(state) is not Security sec)
                {
                    $"Coudn't resolve security {OrdersWrapper.Ticker} to create an order."
                        .DebugPrintWarning();

                    return null;
                }

                var flags = OrdersWrapper.Flags;
                var order = new Order()
                {
                    Security = sec,
                    ExecutionCondition = FromMoexExecutionMode(OrdersWrapper.OrderExecutionMode),
                    Expiry = OrdersWrapper.Expiry ?? default,
                    IsLimit = flags.HasFlag(OrderFlags.IsLimitOrder),
                    TransactionId = OrdersWrapper.TransactionId,                    
                    Quote = new Quote
                    {
                        Price = OrdersWrapper.Price,
                        Size = OrdersWrapper.Size,
                        Operation = flags.HasFlag(OrderFlags.IsSellOrder)
                            ? Operations.Sell
                            : Operations.Buy
                    }
                };

                Update(order, state);

                return order;
            }
        }
        public override void Update(Order entity)
        {
            base.Update(entity);

            lock (_requestInProgressLock)
            {
                _updateParams.Arg0 = entity.Security.ClassCode;
                _updateParams.Arg1 = entity.ExchangeAssignedIdString;
                _updateParams.Callback.Arg0 = entity;

                FunctionsWrappers.ReadSpecificEntry(ref _updateParams);
            }
        }
        protected override void Update(Order entity, LuaWrap state)
        {
            lock (OrderbookWrapper.Lock)
            {
                OrdersWrapper.Set(state);

                if (OrdersWrapper.ExchangeOrderId is string orderId && !string.IsNullOrEmpty(orderId))
                {
                    entity.ExchangeAssignedIdString = orderId;
                }
                else
                {
                    $"Order id was not assigned to {entity} because it was not provided in the feed".DebugPrintWarning();
                }

                var flags = OrdersWrapper.Flags;

                entity.RemainingSize = OrdersWrapper.Rest;
                entity.State = flags.HasFlag(OrderFlags.IsAlive)
                    ? OrderStates.Active
                    : OrderStates.Done; 
            }
        }

        protected override OrderRequestContainer CreateRequestFrom(LuaWrap state)
        {
            lock (OrdersWrapper.Lock)
            {
                OrdersWrapper.Set(state);

                return new()
                {
                    ExchangeAssignedId = OrdersWrapper.ExchangeOrderId,
                    ClassCode = OrdersWrapper.ClassCode
                }; 
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Security? ResolveSecurityOfOrder(LuaWrap state)
        {
            var request = new SecurityRequestContainer
            {
                ClassCode = OrdersWrapper.ClassCode,
                Ticker = OrdersWrapper.Ticker,
            };
            return _securityResolver.Resolve(ref request);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static OrderExecutionConditions FromMoexExecutionMode(MoexOrderExecutionModes mode)
        {
            return mode switch
            {
                MoexOrderExecutionModes.FillOrKill => OrderExecutionConditions.FillOrKill,
                MoexOrderExecutionModes.CancelRest => OrderExecutionConditions.CancelRest,
                MoexOrderExecutionModes.GoodTillCanceled => OrderExecutionConditions.GoodTillCancelled,
                MoexOrderExecutionModes.GoodTillDate => OrderExecutionConditions.GoodTillDate,
                _ => OrderExecutionConditions.Session
            };
        }

        #region Singleton
        [SingletonInstance]
        public static OrdersProvider Instance { get; } = new();
        private OrdersProvider()
        {
        }
        #endregion
    }
}
