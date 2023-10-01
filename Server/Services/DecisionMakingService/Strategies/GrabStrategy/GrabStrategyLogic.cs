using MarketDataProvisionService;
using MarketExecutionService;
using Tools;
using Tools.Logging;
using TradingConcepts;
using TradingConcepts.CommonImplementations;

namespace DecisionMakingService.Strategies
{
    internal partial class GrabStrategy : IDisposable
    {
        public GrabStrategy()
        {
            _execution = DI.Resolve<IMarketExecutionService>() ?? throw new Exception("MarketExecutionService is not initialized");
            _dataFeed = DI.Resolve<IMarketDataProvisionService>() ?? throw new Exception("MarketDataProvisionService is not initialized"); 
            _quotesReader = new OneSideQuotesReader(ReadQuote);
            _executionThread = new Thread(WorkCycle);
            _executionThread.Start();
        }

        private void ResolveBookReader()
        {
            var commandResult = _dataFeed.GetOrderbook(Configuration.Security);

            if (!commandResult.IsSuccess || commandResult.Data is not IOrderbookReader reader)
            {
                throw new Exception("Cannot continue with GrabStrategy configuration because the orderbook can't get resolved");
            }

            if (reader == _bookReader)
            {
                return;
            }

            if (reader is not INotifyEntityUpdated newbook)
            {
                throw new NotSupportedException($"Orderbook must implement {nameof(INotifyEntityUpdated)} " +
                    $"in order to be used in the {GetType().Name}");
            }

            if (_bookReader is INotifyEntityUpdated book)
            {
                book.Updated -= OnNewData;
            }

            newbook.Updated += OnNewData;
            _bookReader = reader;
        }
        private void WorkCycle()
        {
            while (!_disposed)
            {
                try
                {
                    _processingAlarm.WaitOne();
                    ProcessQuote();
                }
                catch (Exception e)
                {
                    _log.Error($"Error in {nameof(GrabStrategy)} {Id} while processing the quote", e);
                }
            }
        }
        private void ProcessQuote()
        {
            var quote = _processingQuote;

            if (State != State.Enabled || quote.Size == default)
            {
                return;
            }

            var orderQuote = new Quote()
            {
                Price = quote.Price
            };

            bool priceIsJuicy;

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
                            _bookReader.Security,
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
        private void ReadQuote(Quote[] quotes, Operations operation, long depth)
        {
            // we are blocking the reader, dump the data into a variable
            // so that _executionThread picks it up and process
            _processingQuote = quotes[0];
            _processingAlarm.Set();
        }
        private void OnNewData()
        {
            if (State != State.Enabled)
            {
                return;
            }

            if (Configuration.Operation == Operations.Buy)
            {
                _bookReader.UseAsks(_quotesReader);
            }
            else
            {
                _bookReader.UseBids(_quotesReader);
            }
        }

        private readonly IMarketExecutionService _execution;
        private readonly IMarketDataProvisionService _dataFeed;
        private readonly Log _log = LogManagement.GetLogger(typeof(GrabStrategy));
        private readonly AutoResetEvent _processingAlarm = new(false);
        private readonly Thread _executionThread;
        private readonly OneSideQuotesReader _quotesReader;
        private IOrderbookReader? _bookReader;
        private Quote _processingQuote;

        #region IDisposable

        private bool _disposed;
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;

                // to release the _executionThread that is stuck on the wait handle
                _processingAlarm.Set();
                _processingAlarm.Dispose();
            }
        }

        #endregion
    }
}
