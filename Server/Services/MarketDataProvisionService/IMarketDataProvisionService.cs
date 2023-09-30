using AppComponents.Delegates;
using AppComponents.Messaging.Results;
using TradingConcepts;
using TradingConcepts.CommonImplementations;

namespace MarketDataProvisionService
{
    public interface IMarketDataProvisionService
    {
        event FeedSubscriber<ITrade> NewTrade;
        event FeedSubscriber<ISecurity> NewSecurity;

        Result GetTrades();
        Result GetOrderbook(ISecurity security);
        Result GetSecurities(SecurityTemplate filter);
    }
}