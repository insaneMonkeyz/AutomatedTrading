using MarketDataProvisionService;
using Tools;
using TradingConcepts;

namespace DecisionMakingService.Strategies
{
    internal class OrderbookOperator : IDisposable
    {
        public object? Owner { get; }

        public ISecurity? Security
        {
            get => _bookReader?.Security;
        }

        public Action ProcessData
        {
            get => _processData;
            set => _processData = value ?? throw new ArgumentNullException(nameof(value));
        }
        public OneSideQuotesReader? AsksReader
        {
            get => _asksReader;
            set => _asksReader = value;
        }
        public OneSideQuotesReader? BidsReader
        {
            get => _bidsReader;
            set => _bidsReader = value;
        }
        public bool IsEnabled
        {
            get => _isEnabled; 
            set
            {
                _isEnabled = value;

                if (value)
                {
                    StartProcessingThread();
                }
            }
        }

        public event Action<Exception>? Error;

        public void ProcessAsync()
        {
            _processingAlarm.Set();
        }

        private void StartProcessingThread()
        {
            if (!_executionThread.IsAlive && !_disposed)
            {
                _executionThread.Start();
            }
        }
        private void ResolveBookReader(ISecurity security)
        {
            var commandResult = _dataFeed.GetOrderbook(security);

            var ownerPrefix = Owner is null
                    ? string.Empty
                    : $"{Owner?.GetType().Name}'s ";

            if (!commandResult.IsSuccess || commandResult.Data is not IOrderbookReader reader)
            {
                throw new Exception($"{ownerPrefix}{nameof(OrderbookOperator)} failed to resolve the orderbook for {security}");
            }

            if (reader is not INotifyEntityUpdated newbook)
            {
                throw new NotSupportedException($"Orderbook must implement {nameof(INotifyEntityUpdated)} " +
                    $"in order to be used in the {ownerPrefix}{nameof(OrderbookOperator)}." +
                    $"Type: {reader.GetType().FullName}");
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
                    ProcessData();
                }
                catch (Exception e)
                {
                    Error?.Invoke(e);
                }
            }
        }
        private void OnNewData()
        {
            if (!_isEnabled)
            {
                return;
            }

            if (_asksReader != null)
            {
                _bookReader.UseAsks(_asksReader);
            }
            if (_bidsReader != null)
            {
                _bookReader.UseBids(_bidsReader);
            }
        }

        public OrderbookOperator(object? owner, ISecurity? security)
        {
            if (security is null)
            {
                throw new ArgumentNullException(nameof(security));
            }

            _dataFeed = DI.Resolve<IMarketDataProvisionService>() ?? throw new Exception("MarketDataProvisionService is not initialized");
            _executionThread = new Thread(WorkCycle);
            Owner = owner;
            ResolveBookReader(security);
        }

        private bool _isEnabled;
        private IOrderbookReader? _bookReader;
        private readonly Thread _executionThread;
        private readonly IMarketDataProvisionService _dataFeed;
        private readonly AutoResetEvent _processingAlarm = new(false);

        private Action _processData = delegate { };
        private OneSideQuotesReader? _asksReader = delegate { };
        private OneSideQuotesReader? _bidsReader = delegate { };

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
                (_bookReader as INotifyEntityUpdated).Updated -= OnNewData;
            }
        }

        #endregion
    }
}
