using MarketExecutionService;
using Microsoft.Extensions.Configuration;
using Tools;
using Tools.Logging;
using TradingConcepts;
using TradingConcepts.CommonImplementations;

namespace DecisionMakingService.Strategies
{
    internal partial class GrabStrategy : IDisposable
    {
        public GrabStrategy() : base()
        {
            _execution = DI.Resolve<IMarketExecutionService>() ?? throw new Exception("MarketExecutionService is not initialized");
            _quotesReader = new OneSideQuotesReader(OnReadingQuote);
        }

        private void OnProcessing()
        {
            var quote = _processingQuote;

            var orderQuote = new Quote()
            {
                Price = quote.Price
            };

            bool priceIsJuicy;

            // TODO: Check price limits. ISecurity at the moment does not contain any information on that

            if (quote.Operation == Operations.Buy)
            {
                orderQuote.Operation = Operations.Sell;
                priceIsJuicy = quote.Price >= Configuration.TriggerPrice;
            }
            else
            {
                orderQuote.Operation = Operations.Buy;
                priceIsJuicy = quote.Price <= Configuration.TriggerPrice;
            }

            if (priceIsJuicy)
            {
                var remainder = quote.Size;

                while (remainder > 0)
                {
                    orderQuote.Size = Math.Min(remainder, Configuration.MaxOrderSize);

                    var commandResult =
                        _execution.PlaceNew(
                            Configuration.Security,
                                        Configuration.Account.AccountCode,
                                            ref orderQuote,
                                                OrderExecutionConditions.CancelRest);

                    if (!commandResult.IsSuccess)
                    {
                        _log.Warn($"Failed to place the order. " +
                            $"{nameof(GrabStrategy)} {Id} {Configuration.Security} {quote}\n{commandResult.Description}");
                    }

                    remainder -= orderQuote.Size;
                }
            }
        }
        private void OnReadingQuote(Quote[] quotes, Operations operation, long depth)
        {
            // in order not to block the notification thread
            // we'll read and dump the data into a variable
            // then awake the _executionThread to let him pick the data up and process it

            _processingQuote = quotes[0];

            if (State == State.Enabled && _processingQuote.Size != default)
            {
                _bookOperator.ProcessAsync();
            }
        }

        private readonly Log _log = LogManagement.GetLogger(typeof(GrabStrategy));
        private readonly IMarketExecutionService _execution;
        private readonly OneSideQuotesReader _quotesReader;
        private OrderbookOperator? _bookOperator;
        private Quote _processingQuote;

        #region IDisposable

        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            _bookOperator?.Dispose();
        }

        #endregion
    }
}
