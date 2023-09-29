using AppComponents;
using AppComponents.Delegates;
using AppComponents.Messaging.Results;
using TradingConcepts;
using TradingConcepts.CommonImplementations;

namespace MarketExecutionService
{
    public interface IMarketExecutionService
    {
        event FeedSubscriber<ITradingAccount> NewAccounts;
        event FeedSubscriber<IOrderExecution> NewExecutions;
        event FeedSubscriber<IOrder> NewOrders;

        Result GetAccounts();
        Result GetExecutions();
        Result GetOrders();

        Result PlaceNew(ISecurity security, string account, ref Quote quote, OrderExecutionConditions? executionCondition = null, DateTime? expiry = null);
        Result Change(IOrder order, Decimal5 newPrice, long newSize);
        Result Cancel(IOrder order);
    }
}