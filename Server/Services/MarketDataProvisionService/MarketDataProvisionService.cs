using AppComponents.Delegates;
using AppComponents.Messaging.Results;
using Microsoft.AspNetCore.Mvc.Filters;
using Quik;
using Tools;
using TradingConcepts;
using TradingConcepts.CommonImplementations;
using TradingConcepts.SecuritySpecifics;
using TradingConcepts.SecuritySpecifics.Options;

namespace MarketDataProvisionService
{
    internal partial class MarketDataProvisionService : IMarketDataProvisionService
    {
        public event FeedSubscriber<ISecurity> NewSecurity
        {
            add
            {
                var action = (ISecurity sec) => value(_quik.Id, sec);
                _quik.NewSecurity += action;
                _subscriptions[value] = action;
            }
            remove
            {
                if (_subscriptions.TryGetValue(value, out var action))
                {
                    _quik.NewSecurity -= action as Action<ISecurity>;
                    _subscriptions.Remove(value);
                }
            }
        }
        public event FeedSubscriber<ITrade> NewTrade
        {
            add
            {
                var action = (ITrade trade) => value(_quik.Id, trade);
                _quik.NewTrade += action;
                _subscriptions[value] = action;
            }
            remove
            {
                if (_subscriptions.TryGetValue(value, out var action))
                {
                    _quik.NewTrade -= action as Action<ITrade>;
                    _subscriptions.Remove(value);
                }
            }
        }

        public Result GetTrades()
        {
            // if new market data feed providers added, implement logic that aggregates trades from all sources

            try
            {
                return Result.Success(_quik.GetTrades());
            }
            catch (Exception e)
            {
                return Result.Error(e.Message);
            }
        }

        public Result GetOrderbook(ISecurity security)
        {
            try
            {
                var orderbook = _quik.GetOrderBook(security.GetType(), security.Ticker);
                return Result.Success(orderbook);
            }
            catch (Exception e)
            {
                return Result.Error(e.Message);
            }
        }

        public Result GetSecurities(SecurityTemplate filter)
        {
            if (filter.SecurityType is null)
            {
                return Result.Error("Must define security type");
            }

            //                                         dont forget to extend when adding new sources

            if (filter.ExchangeId is Guid exchangeId && exchangeId != MoexSpecifics.MoexExchangeId)
            {
                return Result.Error("Exchange is not supported");
            }

            try
            {
                var availableSecurities = _quik.GetAvailableSecurities(filter.SecurityType);
                var sampleInstance = availableSecurities.FirstOrDefault();

                if (sampleInstance is not IExpiring)
                {
                    return Result.Success(Enumerable.Empty<ISecurity>());
                }

                if (filter.TickerTemplate is not null)
                {
                    availableSecurities = availableSecurities
                        .Where(s => s.Ticker.Contains(filter.TickerTemplate, StringComparison.InvariantCultureIgnoreCase));
                }

                var expirings = availableSecurities.OfType<IExpiring>();
                
                if (filter.HasExpirationConstraints())
                {
                    filter.ExpiringFrom ??= DateTime.MinValue;
                    filter.ExpiringUntil ??= DateTime.MaxValue;

                    expirings = expirings.Where(e => e.Expiry >= filter.ExpiringFrom &&
                                                     e.Expiry <= filter.ExpiringUntil);
                }

                var result = default(ISecurity[]);

                if (sampleInstance is OptionDescription)
                {
                    var options = expirings.OfType<OptionDescription>();

                    if (filter.HasStrikeConstraints())
                    {
                        filter.MinStrike ??= Decimal5.MIN_VALUE;
                        filter.MaxStrike ??= Decimal5.MAX_VALUE;

                        options = options.Where(o => o.Strike >= filter.MinStrike &&
                                                     o.Strike <= filter.MaxStrike);
                    }

                    result = options.Select(o => _quik.GetSecurity<IOption>(o.Ticker))
                                    .OfType<ISecurity>()
                                    .ToArray();
                }
                else if (sampleInstance is CalendarSpreadDescription)
                {
                    result = expirings.OfType<SecurityDescription>()
                                      .Select(c => _quik.GetSecurity<ICalendarSpread>(c.Ticker))
                                      .OfType<ISecurity>()
                                      .ToArray();
                }
                else if (sampleInstance is FuturesDescription)
                {
                    result = expirings.OfType<SecurityDescription>()
                                      .Select(c => _quik.GetSecurity<IFutures>(c.Ticker))
                                      .OfType<ISecurity>()
                                      .ToArray();
                }
                else
                {
                    throw new NotSupportedException($"This type '{sampleInstance.GetType().Name}' of security description is not supported");
                }

                return Result.Success(result);
            }
            catch (Exception e) when (e is not NotSupportedException)
            {
                return Result.Error(e.Message);
            }
        }

        private static readonly Dictionary<object, object> _subscriptions = new();
    }
}