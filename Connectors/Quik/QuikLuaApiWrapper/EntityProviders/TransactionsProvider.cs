using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Quik.Entities;
using Quik.EntityProviders.Attributes;
using Quik.EntityProviders.QuikApiWrappers;
using Quik.EntityProviders.RequestContainers;
using Quik.EntityProviders.Resolvers;
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

        public Order PlaceNew(MoexOrderSubmission submission)
        {
            var newOrderArgs = new NewOrderArgs
            {
                OrderSubmission = submission,
                Expiry = string.Empty,
                Operation = submission.Quote.Operation == Operations.Buy
                    ? BUY_OPERATION_PARAM
                    : SELL_OPERATION_PARAM
            };

            if (submission.IsMarket)
            {
                newOrderArgs.ExecutionType = ORDER_TYPE_MARKET_PARAM;
            }
            else
            {
                newOrderArgs.ExecutionType = ORDER_TYPE_LIMIT_PARAM;
                newOrderArgs.Price = submission.Quote.Price.ToString((uint)submission.Security.PricePrecisionScale);
            }

            switch (submission.ExecutionCondition)
            {
                case OrderExecutionConditions.FillOrKill:
                    {
                        newOrderArgs.ExecutionCondition = FILL_OR_KILL_ORDER_PARAM;
                        break;
                    }
                case OrderExecutionConditions.CancelRest:
                    {
                        newOrderArgs.ExecutionCondition = CANCEL_BALANCE_ORDER_PARAM;
                        break;
                    }
                case OrderExecutionConditions.Session:
                    {
                        newOrderArgs.ExecutionCondition = QUEUE_ORDER_PARAM;
                        newOrderArgs.Expiry = ORDER_TODAY_EXPIRY_PARAM;
                        break;
                    }
                case OrderExecutionConditions.GoodTillCancelled:
                    {
                        newOrderArgs.ExecutionCondition = QUEUE_ORDER_PARAM;
                        newOrderArgs.Expiry = ORDER_GTC_EXPIRY_PARAM;
                        break;
                    }
                case OrderExecutionConditions.GoodTillDate:
                    {
                        newOrderArgs.ExecutionCondition = QUEUE_ORDER_PARAM;
                        newOrderArgs.Expiry = submission.Expiry.ToString("yyyyMMdd");
                        break;
                    }
                default:
                    throw new NotImplementedException(
                        $"Add support for {nameof(OrderExecutionConditions)}.{submission.ExecutionCondition} case");
            }

            var error = PlaceNewOrder(ref newOrderArgs);
            var order = new Order(submission)
            {
                SubmittedTime = DateTime.UtcNow
            };

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

            return order;
        }
        public void Cancel(Order order)
        {
            if (order.State != OrderStates.Active)
            {
                OnCancellationDenied(order, $"Cannot cancel inactive order {order}");
                return;
            }

            var args = new CancelOrderArgs
            {
                Order = order
            };

            var error = CancelOrder(ref args);

            if (error != null)
            {
                OnCancellationDenied(order, $"Order {order} cancellation rejected\n  {error}");
            }
            else
            {
                order.AddIntermediateState(OrderStates.Cancelling);
                _eventSignalizer.QueueEntity(OrderChanged, order);
            }
        }
        public void Change(Order order, Decimal5 newprice, long newsize)
        {
            if (order.State != OrderStates.Active)
            {
                OnChangeDenied(order, $"Cannot change inactive order {order}");
                return;
            }

            if (order.ExchangeAssignedId == default || order.State.HasFlag(OrderStates.Registering))
            {
                OnChangeDenied(order, $"Cannot change an order that is still registering {order}");
                return;
            }

            var changeArgs = new ChangeOrderArgs
            {
                Order = order,
                NewPrice = newprice,
                NewSize = newsize
            };

            var error = ChangeOrder(ref changeArgs);

            if (error != null)
            {
                OnChangeDenied(order, $"Order {order} changing rejected\n{error}");
            }
            else
            {
                order.AddIntermediateState(OrderStates.Changing);
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
