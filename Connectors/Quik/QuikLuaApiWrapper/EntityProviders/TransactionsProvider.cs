using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Quik.Entities;
using Quik.EntityProviders.Attributes;
using Quik.EntityProviders.QuikApiWrappers;
using Quik.EntityProviders.RequestContainers;
using Quik.EntityProviders.Resolvers;
using Tools;
using TradingConcepts;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Quik.EntityProviders.QuikApiWrappers.TransactionWrapper;

namespace Quik.EntityProviders
{
    internal sealed class TransactionsProvider : QuikDataConsumer<Order>
    {
        protected override string QuikCallbackMethod => CALLBACK_METHOD;

        private readonly RandomAccessQueue<Order> _transactionsQueue = new();
        private IsSubjectToDequeue<Order, long> _isRelatedToTransaction;
        private EntityResolver<OrderRequestContainer, Order>? _ordersResolver;

        public EntityEventHandler<Order> OrderChanged = delegate { };
        public EntityEventHandler<Order> ChangeDenied = delegate { };
        public EntityEventHandler<Order> CancellationDenied = delegate { };

        public override void Initialize(ExecutionLoop entityNotificationLoop)
        {
#if TRACE
            this.Trace();
#endif
            _ordersResolver = EntityResolvers.GetOrdersResolver();
            _isRelatedToTransaction = IsRelatedToTransaction;
            base.Initialize(entityNotificationLoop);
        }

        public void PlaceNew(Order order)
        {
            var newOrderArgs = new NewOrderArgs
            {
                Order = order,
                Expiry = string.Empty,
                ExecutionType = ORDER_TYPE_LIMIT_PARAM,
                Operation = order.Quote.Operation == Operations.Buy
                    ? BUY_OPERATION_PARAM
                    : SELL_OPERATION_PARAM
            };
            
            switch (order.ExecutionCondition)
            {
                case OrderExecutionConditions.Session:
                    {
                        newOrderArgs.Price = order.Quote.Price.ToString((uint)order.Security.PricePrecisionScale);
                        newOrderArgs.ExecutionCondition = QUEUE_ORDER_PARAM;
                        newOrderArgs.Expiry = ORDER_TODAY_EXPIRY_PARAM;
                        break;
                    }
                case OrderExecutionConditions.Market:
                    {
                        newOrderArgs.ExecutionType = ORDER_TYPE_MARKET_PARAM;
                        break;
                    }
                case OrderExecutionConditions.GoodTillCancelled:
                    {
                        newOrderArgs.Price = order.Quote.Price.ToString((uint)order.Security.PricePrecisionScale);
                        newOrderArgs.ExecutionCondition = QUEUE_ORDER_PARAM;
                        newOrderArgs.Expiry = ORDER_GTC_EXPIRY_PARAM;
                        break;
                    }
                case OrderExecutionConditions.GoodTillDate:
                    {
                        newOrderArgs.Price = order.Quote.Price.ToString((uint)order.Security.PricePrecisionScale);
                        newOrderArgs.ExecutionCondition = QUEUE_ORDER_PARAM;
                        newOrderArgs.Expiry = order.Expiry.ToString("yyyyMMdd");
                        break;
                    }
                case OrderExecutionConditions.FillOrKill:
                    {
                        newOrderArgs.Price = order.Quote.Price.ToString((uint)order.Security.PricePrecisionScale);
                        newOrderArgs.ExecutionCondition = FILL_OR_KILL_ORDER_PARAM;
                        break;
                    }
                case OrderExecutionConditions.CancelRest:
                    {
                        newOrderArgs.Price = order.Quote.Price.ToString((uint)order.Security.PricePrecisionScale);
                        newOrderArgs.ExecutionCondition = CANCEL_BALANCE_ORDER_PARAM;
                        break;
                    }
                default:
                    throw new NotImplementedException(
                        $"Add support for {nameof(OrderExecutionConditions)}.{order.ExecutionCondition} case");
            }

            var error = PlaceNewOrder(ref newOrderArgs);
            order.SubmittedTime = DateTime.UtcNow;

            if (error.HasNoValue())
            {
                order.AddIntermediateState(OrderStates.Registering);
                _transactionsQueue.Enqueue(order);
            }
            else
            {
                order.SetSingleState(OrderStates.Rejected);
                _log.Warn($"Order {order} placement rejected\n{error}");
            }
        }
        public Order? Change(Order order, Decimal5 newprice, long newsize)
        {
            var neworder = new Order(order.Security)
            {
                TransactionId = TransactionIdGenerator.CreateId(),
                ExecutionCondition = order.ExecutionCondition,
                AccountCode = order.AccountCode,
                ParentOrder = order,
                Expiry = order.Expiry,
                Quote = new()
                {
                    Price = newprice,
                    Size = newsize,
                    Operation = order.Quote.Operation
                }
            };

            var error = ChangeOrder(neworder);
            neworder.SubmittedTime = DateTime.UtcNow;

            if (error != null)
            {
                OnChangeDenied(order, $"Order {order} changing rejected\n{error}");
                return null;
            }
            else
            {
                order.AddIntermediateState(OrderStates.Changing);
                neworder.AddIntermediateState(OrderStates.Registering);
                _eventSignalizer.QueueEntity(OrderChanged, order);
                return neworder;
            }
        }
        public void Cancel(Order order)
        {
            if (CancelOrder(order) is string error)
            {
                OnCancellationDenied(order, $"Order {order} cancellation rejected\n  {error}");
            }
            else
            {
                order.AddIntermediateState(OrderStates.Cancelling);
                _eventSignalizer.QueueEntity(OrderChanged, order);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnCancellationDenied(Order order, string errorMessage)
        {
            _log.Warn(errorMessage);
            _eventSignalizer.QueueEntity(CancellationDenied, order);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnChangeDenied(Order order, string errorMessage)
        {
            _log.Warn(errorMessage);
            _eventSignalizer.QueueEntity(ChangeDenied, order);
        }

        private bool IsRelatedToTransaction(Order order, long transactionId)
        {
            return order.TransactionId == transactionId;
        }
        protected override int OnNewData(nint state)
        {
            try
            {
#if TRACE
                this.Trace();
#endif
                lock (_callbackLock)
                {
                    SetContext(state);
#if DEBUG && !UNITTESTING
                    _log.Debug(Helper.PrintQuikParameters(typeof(TransactionWrapper)));
#endif

                    var status = Status;
                    if (status != TransactionStatus.Completed)
                    {
                        _log.Warn($"Transaction rejected by {ErrorSource}. {status}\n{ResultDescription}");
                    }

                    var transactionId = Id;
                    if (ClassCode is not string classcode)
                    {
                        _log.Warn($"Cannot process transaction {transactionId} callback. Class Code of security of the order is not set");
                        return 1;
                    }

                    if (_transactionsQueue.TryDequeueItem(_isRelatedToTransaction, transactionId, out Order order))
                    {
                        order.SubmissionReplyTime = DateTime.UtcNow;
                        order.ExchangeAssignedId = OrderId;
                        order.RemainingSize = order.Quote.Size - RejectedSize;

                        if (status != TransactionStatus.Completed)
                        {
                            order.SetSingleState(OrderStates.Rejected);
                        }
                        else
                        {
                            if (order.RemainingSize > 0)
                            {
                                order.SetSingleState(OrderStates.Active);
                            }
                            else
                            {
                                order.SetSingleState(OrderStates.Done);
                            }
                        }

                        var orderLookup = new OrderRequestContainer
                        {
                            ExchangeAssignedId = order.ExchangeAssignedId,
                            ClassCode = ClassCode,
                        };
                        _ordersResolver.CacheEntity(ref orderLookup, order);
                        _eventSignalizer.QueueEntity(OrderChanged, order);

                        return 1;
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error(CALLBACK_EXCEPTION_MSG, e);
            }

            return 1;
        }

        #region Singleton
        [SingletonInstance]
        public static TransactionsProvider Instance { get; } = new();
        private TransactionsProvider() { }
        #endregion
    }
}
