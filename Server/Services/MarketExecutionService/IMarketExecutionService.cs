using AppComponents;
using AppComponents.Delegates;
using AppComponents.Messaging.Results;
using TradingConcepts;
using TradingConcepts.CommonImplementations;

namespace MarketExecutionService
{
    public interface IMarketExecutionService
    {
        event FeedSubscriber<ISecurityBalance> NewSecurityBalance;
        event FeedSubscriber<ITradingAccount> NewAccount;
        event FeedSubscriber<IOrderExecution> NewExecution;
        event FeedSubscriber<IOrder> NewOrder;

        Result GetSecuritiesBalances();
        Result GetAccounts();
        Result GetExecutions();
        Result GetOrders();

        Result PlaceNew(ISecurity security, string account, ref Quote quote, OrderExecutionConditions? executionCondition = null, DateTime? expiry = null);
        Result Change(IOrder order, Decimal5 newPrice, long newSize);
        Result Cancel(IOrder order);
    }
}