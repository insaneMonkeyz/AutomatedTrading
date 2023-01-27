﻿using BasicConcepts;
using Quik.Entities;
using Quik.EntityDataProviders.Attributes;
using Quik.EntityDataProviders.RequestContainers;
using Quik.EntityDataProviders.QuikApiWrappers;

using static Quik.QuikProxy;

using UpdateParams = Quik.QuikProxy.MultiGetMethod2ParamsNoReturn<Quik.Entities.Order, Quik.LuaState>;
using SecurityResolver = Quik.EntityDataProviders.EntityResolver<Quik.EntityDataProviders.RequestContainers.SecurityRequestContainer, Quik.Entities.Security>;

namespace Quik.EntityDataProviders
{
    internal class OrderDataProvider : DataProvider<Order, OrderRequestContainer>
    {
        private readonly SecurityResolver _securityResolver;
        private readonly SecurityRequestContainer _securityRequest = new();
        private UpdateParams _updateParams;

        protected override string QuikCallbackMethod => OrdersWrapper.CALLBACK_METHOD;

        public List<Order> GetAllOrders()
        {
            return ReadWholeTable(OrdersWrapper.NAME, Create);
        }
        public override void Update(Order entity)
        {
            lock (_userRequestLock)
            {
                _updateParams.Arg0 = entity.Security.ClassCode;
                _updateParams.Arg1 = entity.ExchangeAssignedIdString;
                _updateParams.ActionParams.Arg0 = entity;

                ReadSpecificEntry(ref _updateParams);
            }
        }

        protected override Order? Create(LuaState state)
        {
            BuildSecurityResolveRequest(state);

            if (_securityResolver.GetEntity(_securityRequest) is not Security sec)
            {
                $"Coudn't resolve security {_securityRequest} to create an order."
                    .DebugPrintWarning();

                return null;
            }

            OrdersWrapper.Set(state);

            var flags = OrdersWrapper.Flags;
            var order = new Order()
            {
                Security = sec,
                ExecutionMode = FromMoexExecutionMode(OrdersWrapper.OrderExecutionMode),
                Expiry = OrdersWrapper.Expiry ?? default,
                IsLimit = flags.HasFlag(OrderFlags.IsLimitOrder),
                Quote = new OrderQuote
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
        protected void BuildSecurityResolveRequest(LuaState state)
        {
            OrdersWrapper.Set(state);

            _securityRequest.Ticker = OrdersWrapper.Ticker;
            _securityRequest.ClassCode = OrdersWrapper.ClassCode;
        }
        protected override void BuildEntityResolveRequest(LuaState state)
        {
            OrdersWrapper.Set(state);

            _resolveEntityRequest.ExchangeAssignedId = OrdersWrapper.ExchangeOrderId;
        }
        protected override void Update(Order entity, LuaState state)
        {
            OrdersWrapper.Set(state);

            var flags = OrdersWrapper.Flags;

            entity.RemainingSize = OrdersWrapper.Rest;
            entity.State = flags.HasFlag(OrderFlags.IsAlive)
                ? OrderStates.Active
                : OrderStates.Done;
        }

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
        public static OrderDataProvider Instance { get; } = new();
        private OrderDataProvider() : base(EntityResolversFactory.GetOrdersResolver())
        {
            _securityResolver = EntityResolversFactory.GetSecurityResolver();
            _updateParams = new()
            {
                Method = OrdersWrapper.GET_METOD,
                ReturnType1 = LuaApi.TYPE_TABLE,
                ReturnType2 = LuaApi.TYPE_NUMBER,
                Action = Update,
                ActionParams = new()
                {
                    Arg1 = State
                }
            };
        }
        #endregion
    }
}
