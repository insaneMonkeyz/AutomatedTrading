using BasicConcepts;
using Quik.Entities;
using Quik.EntityProviders.Attributes;
using Quik.EntityProviders.RequestContainers;
using Quik.EntityProviders.QuikApiWrappers;

using static Quik.Quik;

using UpdateParams = Quik.EntityProviders.QuikApiWrappers.FunctionsWrappers.VoidMultiGetMethod2Params<Quik.Entities.Order, Quik.Lua.LuaWrap>;
using CreateParams = Quik.EntityProviders.QuikApiWrappers.FunctionsWrappers.MultiGetMethod2Params<Quik.Lua.LuaWrap, Quik.Entities.Order?>;
using SecurityResolver = Quik.EntityProviders.EntityResolver<Quik.EntityProviders.RequestContainers.SecurityRequestContainer, Quik.Entities.Security>;
using System.Runtime.CompilerServices;
using Quik.Lua;
using Quik.EntityProviders.QuikApiWrappers;

namespace Quik.EntityProviders
{
    internal class OrdersProvider : UpdatableEntitiesProvider<Order, OrderRequestContainer>
    {
        private readonly SecurityResolver _securityResolver;
        private UpdateParams _updateParams;
        private CreateParams _createParams;

        protected override string QuikCallbackMethod => OrdersWrapper.CALLBACK_METHOD;
        protected override string AllEntitiesTable => OrdersWrapper.NAME;

        public override Order? Create(OrderRequestContainer request)
        {
            if (!request.HasData)
            {
                throw new ArgumentException($"{nameof(OrderRequestContainer)} request is missing essential parameters");
            }

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
                    ExecutionMode = FromMoexExecutionMode(OrdersWrapper.OrderExecutionMode),
                    Expiry = OrdersWrapper.Expiry ?? default,
                    IsLimit = flags.HasFlag(OrderFlags.IsLimitOrder),
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
            return _securityResolver.Resolve(new SecurityRequestContainer
            {
                ClassCode = OrdersWrapper.ClassCode,
                Ticker = OrdersWrapper.Ticker,
            });
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static OrderExecutionModes FromMoexExecutionMode(MoexOrderExecutionModes mode)
        {
            return mode switch
            {
                MoexOrderExecutionModes.FillOrKill => OrderExecutionModes.FillOrKill,
                MoexOrderExecutionModes.CancelRest => OrderExecutionModes.CancelRest,
                MoexOrderExecutionModes.GoodTillCanceled => OrderExecutionModes.GoodTillCancelled,
                MoexOrderExecutionModes.GoodTillDate => OrderExecutionModes.GoodTillDate,
                _ => OrderExecutionModes.Undefined
            };
        }

        #region Singleton
        [SingletonInstance]
        public static OrdersProvider Instance { get; } = new();
        private OrdersProvider()
        {
            _securityResolver = EntityResolvers.GetSecurityResolver();
            _updateParams = new()
            {
                Method = OrdersWrapper.GET_METOD,
                ReturnType1 = Api.TYPE_TABLE,
                ReturnType2 = Api.TYPE_NUMBER,
                Callback = new()
                {                    
                    Arg1 = Quik.Lua,
                    Invoke = Update
                }
            };
            _createParams = new()
            {
                Method = OrdersWrapper.GET_METOD,
                ReturnType1 = Api.TYPE_TABLE,
                ReturnType2 = Api.TYPE_NUMBER,
                Callback = new()
                {                    
                    Arg = Quik.Lua,
                    Invoke = Create,
                    DefaultValue = null
                }
            };
        }
        #endregion
    }
}
