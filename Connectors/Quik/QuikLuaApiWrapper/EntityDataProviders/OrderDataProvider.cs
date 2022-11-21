using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicConcepts;
using Quik.Entities;
using Quik.EntityDataProviders.EntityDummies;
using Quik.EntityDataProviders.QuikApiWrappers;
using static Quik.QuikProxy;
using UpdateParams = Quik.QuikProxy.MultiGetMethod2ParamsNoReturn<Quik.Entities.Order, Quik.LuaState>;

namespace Quik.EntityDataProviders
{
    internal class OrderDataProvider : BaseDataProvider<Order, object>
    {
        private readonly SecurityDummy _securityDummy = new();
        private static UpdateParams _updateParams;

        protected override string QuikCallbackMethod => OrdersWrapper.CALLBACK_METHOD;

        public event GetSecurityHandler GetSecurity = delegate { return null; };

        public List<Order> GetAllOrders()
        {
            return ReadWholeTable(OrdersWrapper.NAME, Create);
        }
        public override void Update(Order entity)
        {
            lock (_userRequestLock)
            {
                _updateParams.Arg0 = entity.Security.ClassCode;
                _updateParams.Arg1 = entity.IdString;
                _updateParams.ActionParams.Arg0 = entity;

                ReadSpecificEntry(ref _updateParams);
            }
        }

        protected override Order? Create(LuaState state)
        {
            SetDummy(state);

            if (_securityDummy.HasData)
            {
                if (GetSecurity(_securityDummy) is SecurityBase sec)
                {
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
                else
                {
                    $"Coudn't resolve security {_securityDummy} to create an order."
                        .DebugPrintWarning();
                }
            }
            else
            {
                "Coudn't create an order. Nesessary information about it's security is not provided."
                    .DebugPrintWarning();
            }

            return null;
        }
        protected override void SetDummy(LuaState state)
        {
            OrdersWrapper.Set(state);

            _securityDummy.Ticker = OrdersWrapper.Ticker;
            _securityDummy.ClassCode = OrdersWrapper.ClassCode;
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
        public static OrderDataProvider Instance { get; } = new();
        private OrderDataProvider()
        {
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
