using AppComponents.Delegates;
using AppComponents.Messaging.Results;
using Tools;
using TradingConcepts;
using TradingConcepts.CommonImplementations;

namespace MarketExecutionService
{
    internal partial class MarketExecutionService : IMarketExecutionService
    {
        private static readonly Dictionary<object, object> _subscriptions = new();

        public event FeedSubscriber<ISecurityBalance> NewSecurityBalance
        {
            add
            {
                var action = (ISecurityBalance b) => value(_quik.Id, b);
                _quik.NewSecurityBalance += action;
                _subscriptions[value] = action;
            }
            remove
            {
                if (_subscriptions.TryGetValue(value, out var action))
                {
                    _quik.NewSecurityBalance -= action as Action<ISecurityBalance>;
                    _subscriptions.Remove(value);
                }
            }
        }
        public event FeedSubscriber<ITradingAccount> NewAccount
        {
            add
            {
                var action = (ITradingAccount acc) => value(_quik.Id, acc);
                _quik.NewAccount += action;
                _subscriptions[value] = action;
            }
            remove
            {
                if (_subscriptions.TryGetValue(value, out var action))
                {
                    _quik.NewAccount -= action as Action<ITradingAccount>;
                    _subscriptions.Remove(value);
                }
            }
        }
        public event FeedSubscriber<IOrderExecution> NewExecution
        {
            add
            {
                var action = (IOrderExecution exec) => value(_quik.Id, exec);
                _quik.NewOrderExecution += action;
                _subscriptions[value] = action;
            }
            remove
            {
                if (_subscriptions.TryGetValue(value, out var action))
                {
                    _quik.NewOrderExecution -= action as Action<IOrderExecution>;
                    _subscriptions.Remove(value);
                }
            }
        }
        public event FeedSubscriber<IOrder> NewOrder
        {
            add
            {
                var action = (IOrder order) => value(_quik.Id, order);
                _quik.NewOrder += action;
                _subscriptions[value] = action;
            }
            remove
            {
                if (_subscriptions.TryGetValue(value, out var action))
                {
                    _quik.NewOrder -= action as Action<IOrder>;
                    _subscriptions.Remove(value);
                }
            }
        }

        public Result GetAccounts()
        {
            // if new accounts providers added, implement logic that aggregates accounts from all sources

            try
            {
                var result = _quik.DerivativesAccount.ToEnumerable();

                return Result.Success(result);
            }
            catch (Exception e)
            {
                return Result.Error(e.Message);
            }
        }
        public Result GetExecutions()
        {
            // if new execution providers added, implement logic that aggregates executions from all sources

            try
            {
                return Result.Success(_quik.GetOrderExecutions());
            }
            catch (Exception e)
            {
                return Result.Error(e.Message);
            }
        }
        public Result GetOrders()
        {
            // if new order providers added, implement logic that aggregates orders from all sources

            try
            {
                return Result.Success(_quik.GetOrders());
            }
            catch (Exception e)
            {
                return Result.Error(e.Message);
            }
        }
        public Result GetSecuritiesBalances()
        {
            try
            {
                return Result.Success(_quik.GetSecuritiesBalances());
            }
            catch (Exception e)
            {
                return Result.Error(e.Message);
            }
        }

        public Result Cancel(IOrder order)
        {
            try
            {
                _quik.CancelOrder(order);
                return Result.Success();
            }
            catch (Exception e)
            {
                return Result.Error(e.Message, order);
            }
        }
        public Result Change(IOrder? order, Decimal5 newPrice, long newSize)
        {
            try
            {
                var result = _quik.ChangeOrder(order, newPrice, newSize);

                return result is not null
                    ? Result.Success(result)
                    : Result.Error("Could not change the order", order);
            }
            catch (Exception e)
            {
                return Result.Error(e.Message, order);
            }
        }
        public Result PlaceNew(ISecurity? security, string? account, ref Quote quote, 
            OrderExecutionConditions? executionCondition = null, DateTime? expiry = null)
        {
            // as long as we only have Quik as an execution provider, no routing is needed
            // if new providers added, implement logic that routes the order to the corresponding market connection

            try
            {
                var order = _quik.CreateOrder(security, account, ref quote, executionCondition, expiry);

                _quik.PlaceNewOrder(order);

                return Result.Success(order);
            }
            catch (Exception e)
            {
                var args = new KeyValuePair<string, object?>[]
                {
                    new (nameof(security), security),
                    new (nameof(account), account),
                    new (nameof(quote), quote),
                    new (nameof(executionCondition), executionCondition),
                    new (nameof(expiry), expiry),
                };

                return Result.Error(e.Message, args);
            }
        }
    }
}
