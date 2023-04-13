using Quik.Entities;
using Quik.EntityProviders.Attributes;
using Quik.EntityProviders.QuikApiWrappers;
using Quik.EntityProviders.RequestContainers;
using Quik.EntityProviders.Resolvers;
using TradingConcepts;
using static Quik.EntityProviders.QuikApiWrappers.TransactionWrapper;

namespace Quik.EntityProviders
{
    internal sealed class TransactionsProvider : QuikDataConsumer<Order>
    {
        protected override string QuikCallbackMethod => CALLBACK_METHOD;

        private EntityResolver<TransactionRequestContainer, Order>? _transactionsResolver;
        private EntityResolver<OrderRequestContainer, Order>? _ordersResolver;

        public EntityEventHandler<Order> OrderChanged = delegate { };

        public override void Initialize(ExecutionLoop entityNotificationLoop)
        {
#if TRACE
            this.Trace();
#endif
            _transactionsResolver = EntityResolvers.GetTransactionsResolver();
            _ordersResolver = EntityResolvers.GetOrdersResolver();
            base.Initialize(entityNotificationLoop);
        }

        public Order PlaceNew(MoexOrderSubmission submission)
        {
            if (submission.ClientCode is null)
            {
                throw new ArgumentException($"{nameof(submission.ClientCode)} of the order is not set");
            }

            var newOrderArgs = new NewOrderArgs()
            {
                OrderSubmission = submission,
                Expiry = string.Empty
            };

            newOrderArgs.Operation = submission.Quote.Operation == Operations.Buy
                ? BUY_OPERATION_PARAM
                : SELL_OPERATION_PARAM;

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
            var order = new Order(submission);

            if (error.HasNoValue())
            {
                order.AddIntermediateState(OrderStates.Registering);
            }
            else
            {
                order.SetSingleState(OrderStates.Rejected);
                _log.Warn($"Order {order} placement rejected\n{error}");
            }

            var transactionRequest = new TransactionRequestContainer
            {
                Submitted = order.Submitted.UtcDateTime,
                Ticker = order.Security.Ticker,
                Price = order.Quote.Price,
                Size = order.Quote.Size,
            };

            _transactionsResolver.CacheEntity(ref transactionRequest, order);

            return order;
        }
        public void Cancel(Order order)
        {
            if (order.State != OrderStates.Active)
            {
                _log.Warn($"Cannot cancel inactive order {order}");
                return;
            }

            var args = new CancelOrderArgs
            {
                Order = order
            };

            var error = CancelOrder(ref args);

            if (error != null)
            {
                _log.Warn($"Order {order} cancellation rejected\n  {error}");
            }
            else
            {
                order.AddIntermediateState(OrderStates.Cancelling);
            }
        }
        public void Change(Order order, Decimal5 newprice, long newsize)
        {
            if (order.State != OrderStates.Active)
            {
                _log.Warn($"Cannot change inactive order {order}");
                return;
            }

            if (order.ExchangeAssignedId == default || order.State.HasFlag(OrderStates.Registering))
            {
                _log.Warn($"Cannot change an order that is still registering {order}");
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
                _log.Warn($"Order {order} changing rejected\n{error}");
            }
            else
            {
                order.AddIntermediateState(OrderStates.Changing);
            }
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

                    var transactionLookup = new TransactionRequestContainer
                    {
                        Ticker = Ticker,
                        Price = Price,
                        Size = Size
                    };

                    var order = _transactionsResolver.GetFromCache(ref transactionLookup);

                    if (order != null)
                    {
                        order.ExchangeAssignedIdString = ExchangeAssignedOrderId;
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
#if DEBUG
                        LogEntityUpdated(order);
#endif
                        var orderLookup = new OrderRequestContainer
                        {
                            ExchangeAssignedId = order.ExchangeAssignedIdString,
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
        private void LogEntityUpdated(Order entity)
        {
            _log.Debug($@"Received updates for {nameof(Order)} {entity}
    {nameof(entity.ExchangeAssignedIdString)}={entity.ExchangeAssignedIdString}
    {nameof(entity.RemainingSize)}={entity.RemainingSize}
    {nameof(entity.State)}={entity.State}
    {nameof(TransactionWrapper.Uid)}={Uid}");
        }

        #region Singleton
        [SingletonInstance]
        public static TransactionsProvider Instance { get; } = new();
        private TransactionsProvider() { }
        #endregion
    }
}
