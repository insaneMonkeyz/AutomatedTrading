using Quik.Entities;
using Quik.EntityDataProviders.QuikApiWrappers;
using Quik.EntityProviders;
using Quik.EntityProviders.RequestContainers;
using Quik.Lua;
using TradingConcepts;

namespace Quik.EntityDataProviders
{
    internal class TransactionsProvider
    {
        protected readonly object _callbackLock = new();
        private readonly LuaFunction _onNewDataCallback;

        private EntityResolver<OrderRequestContainer, Order>? _ordersResolver;
        private IEntityEventSignalizer<Order> _eventSignalizer = new DirectEntitySignalizer<Order>();

        public EntityEventHandler<Order> OrderChanged = delegate { };

        public void SubscribeCallback()
        {
#if TRACE
            this.Trace();
#endif
            Quik.Lua.RegisterCallback(_onNewDataCallback, TransactionWrapper.CALLBACK_METHOD);
        }
        public virtual void Initialize(ExecutionLoop entityNotificationLoop)
        {
#if TRACE
            this.Trace();
#endif
            _ordersResolver = EntityResolvers.GetOrdersResolver();
            _eventSignalizer = new EventSignalizer<Order>(entityNotificationLoop)
            {
                IsEnabled = true
            };
        }

        public Order PlaceNew(MoexOrderSubmission submission)
        {
            var error = TransactionWrapper.PlaceNewOrder(submission);
            var order = new Order(submission);

            if (error != null)
            {
                order.SetSingleState(OrderStates.Rejected);
                $"Order {order} placement rejected\n{error}".DebugPrintWarning();
            }
            else
            {
                order.AddIntermediateState(OrderStates.Registering);
            }

            var orderRequest = new OrderRequestContainer
            {
                ClassCode = order.Security.ClassCode,
                TransactionId = order.TransactionId,
                ExchangeAssignedId = order.ExchangeAssignedIdString
            };

            _ordersResolver.CacheEntity(ref orderRequest, order);

            return order;
        }
        public void Cancel(Order order)
        {
            var error = TransactionWrapper.CancelOrder(order);

            if (error != null)
            {
                $"Order {order} cancellation rejected\n{error}".DebugPrintWarning();
            }
            else
            {
                order.AddIntermediateState(OrderStates.Cancelling);
            }
        }
        public void Change(Order order, Decimal5 newprice, int newsize)
        {
            var error = TransactionWrapper.ChangeOrder(order, newprice, newsize);

            if (error != null)
            {
                $"Order {order} changing rejected\n{error}".DebugPrintWarning();
            }
            else
            {
                order.AddIntermediateState(OrderStates.Changing);
            }
        }

        private void Update(Order order, LuaWrap lua)
        {
            order.ExchangeAssignedIdString = TransactionWrapper.ExchangeAssignedOrderId;
            order.RemainingSize = TransactionWrapper.RemainingSize;
            
            if(order.RemainingSize > 0)
            {
                order.SetSingleState(OrderStates.Active);
            }
            else
            {
                order.SetSingleState(OrderStates.Done);
            }
        }

        private int OnTransactionReply(nint state)
        {
            try
            {
#if TRACE
                this.Trace();
#endif
                lock (_callbackLock)
                {
                    TransactionWrapper.SetContext(state);

                    var status = TransactionWrapper.Status;

                    if (status != TransactionWrapper.TransactionStatus.Completed)
                    {
                        $"Transaction rejected by {TransactionWrapper.ErrorSource}. {status}\n{TransactionWrapper.ResultDescription}".DebugPrintWarning();
                        return 1;
                    }

                    if (TransactionWrapper.ClassCode is not string classcode)
                    {
                        "Class Code of security of the order is not set".DebugPrintWarning();
                        return 1;
                    }

                    var orderLookup = new OrderRequestContainer
                    {
                        ClassCode = classcode,
                        TransactionId = TransactionWrapper.Id,

                        // do not set the exchange assigned id
                        // this is a transaction reply, so when the order was created, it was cached without such id
                        // order wont be found!
                    };

                    var order = _ordersResolver.GetFromCache(ref orderLookup);

                    if (order != null)
                    {
                        Update(order, state);

                        orderLookup.ExchangeAssignedId = order.ExchangeAssignedIdString;

                        _ordersResolver.CacheEntity(ref orderLookup, order);
                        _eventSignalizer.QueueEntity(OrderChanged, order);

                        return 1;
                    }
                }
            }
            catch (Exception e)
            {
                e.ToString().DebugPrintWarning();
            }

            return 1;
        }

        #region Singleton
        public static TransactionsProvider Instance { get; } = new();
        private TransactionsProvider()
        {
            _onNewDataCallback = OnTransactionReply;
        } 
        #endregion
    }
}
