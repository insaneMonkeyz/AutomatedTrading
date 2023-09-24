using System.Runtime.CompilerServices;

using TradingConcepts;

using Quik.Entities;
using Quik.EntityProviders.Attributes;
using Quik.EntityProviders.QuikApiWrappers;
using Quik.EntityProviders.RequestContainers;
using Quik.Lua;

using CreateParams = Quik.EntityProviders.QuikApiWrappers.FunctionsWrappers.MultiGetMethod2Params<Quik.Lua.LuaWrap, Quik.Entities.Order?>;
using UpdateParams = Quik.EntityProviders.QuikApiWrappers.FunctionsWrappers.VoidMultiGetMethod2Params<Quik.Entities.Order, Quik.Lua.LuaWrap>;
using TradingConcepts.CommonImplementations;
using Quik.EntityProviders.Resolvers;

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

        protected override Type WrapperType => typeof(OrdersWrapper);
        protected override string QuikCallbackMethod => OrdersWrapper.CALLBACK_METHOD;
        protected override string AllEntitiesTable => OrdersWrapper.NAME;
        protected override Action<LuaWrap> SetWrapper => OrdersWrapper.Set;

        public override void Initialize(ExecutionLoop entityNotificationLoop)
        {
#if TRACE
            this.Trace();
#endif
            _updateParams.Callback.Invoke = Update;
            _createParams.Callback.Invoke = Create;
            _updateParams.Callback.Arg1 = Quik.Lua;
            _createParams.Callback.Arg = Quik.Lua;

            base.Initialize(entityNotificationLoop);
        }

        public override Order? Create(ref OrderRequestContainer request)
        {
#if TRACE
            this.Trace();
#endif
            if (request.ExchangeAssignedId == default)
            {
                return null;
            }

            base.Create(ref request);

            lock (_requestInProgressLock)
            {
                _createParams.Arg0 = request.ClassCode;
                _createParams.Arg1 = request.ExchangeAssignedId.ToString();

                return FunctionsWrappers.ReadSpecificEntry(ref _createParams);
            }
        }
        protected override Order? Create(LuaWrap state)
        {
#if TRACE
            this.Trace();
#endif
            lock (OrdersWrapper.Lock)
            {
                OrdersWrapper.Set(state);

                if (ResolveSecurityOfOrder(state) is not Security sec)
                {
                    _log.Error($"Coudn't resolve security {OrdersWrapper.Ticker} to create an order.");

                    return null;
                }

                var flags = OrdersWrapper.Flags;

                var order = new Order(sec)
                {
                    AccountCode = OrdersWrapper.Account,
                    ExecutionCondition = GetExecutionMode(flags, OrdersWrapper.OrderExecutionMode),
                    Expiry = OrdersWrapper.Expiry ?? default,
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
#if TRACE
            this.Trace();
#endif
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
#if TRACE
            this.Trace();
#endif
            lock (OrderbookWrapper.Lock)
            {
                OrdersWrapper.Set(state);

                var orderId = OrdersWrapper.ExchangeOrderId;
                if (orderId != default)
                {
                    entity.ExchangeAssignedId = orderId;
                }
                else
                {
                    _log.Warn($"Order id was not assigned to {entity} because it was not provided in the feed");
                }

                var flags = OrdersWrapper.Flags;

                entity.RemainingSize = OrdersWrapper.Rest;

                if (flags.HasFlag(OrderFlags.IsAlive))
                {
                    entity.SetSingleState(OrderStates.Active);
                }
                else
                {
                    entity.SetSingleState(OrderStates.Done);
                }
            }
        }

        protected override OrderRequestContainer CreateRequestFrom(LuaWrap state)
        {
#if TRACE
            this.Trace();
#endif
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
#if TRACE
            this.Trace();
#endif
            var request = new SecurityRequestContainer
            {
                ClassCode = OrdersWrapper.ClassCode,
                Ticker = OrdersWrapper.Ticker,
            };
            return _securityResolver.Resolve(ref request);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static OrderExecutionConditions GetExecutionMode(OrderFlags flags, MoexOrderExecutionModes mode)
        {
            if (!flags.HasFlag(OrderFlags.IsLimitOrder))
            {
                return OrderExecutionConditions.Market;
            }

            return mode switch
            {
                MoexOrderExecutionModes.FillOrKill => OrderExecutionConditions.FillOrKill,
                MoexOrderExecutionModes.CancelRest => OrderExecutionConditions.CancelRest,
                MoexOrderExecutionModes.GoodTillDate => OrderExecutionConditions.GoodTillDate,
                MoexOrderExecutionModes.GoodTillCanceled => OrderExecutionConditions.GoodTillCancelled,
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
