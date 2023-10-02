using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarketExecutionService;
using Tools;
using Tools.Logging;
using TradingConcepts;
using TradingConcepts.CommonImplementations;

namespace DecisionMakingService.Strategies.PairTrading
{
    internal partial class PairQuotingStrategy : Strategy, IDisposable
    {
        public PairQuotingStrategy()
        {
            _execution = DI.Resolve<IMarketExecutionService>() ?? throw new Exception("MarketExecutionService is not initialized");
            _quotesReader = new OneSideQuotesReader(OnReadingQuote);
        }
        public override string ToString()
        {
            return $"{nameof(PairQuotingStrategy)} {Id} {Configuration.QuotedSecurity} {Configuration.BaseSecurity}";
        }

        // TODO: Implement logic to calculate the share of the own position of the strategy

        private long CalculatedOrderSize
        {
            get
            {
                var available = Math.Max(Configuration.PositionLimit - _position.Amount, 0);
                return Math.Min(Configuration.OrderSize, available);
            }
        }
        private Decimal5 CalculatedPrice
        {
            get => _processingQuote.Price + Configuration.TargetPriceOffset;
        }
        private Decimal5 LowerPriceLimit
        {
            get => _order.Quote.Price - _priceRange;
        }
        private Decimal5 UpperPriceLimit
        {
            get => _order.Quote.Price + _priceRange;
        }

        private bool IsValid(ref Quote quote)
        {
            // TODO: Implement security price limits check
            return quote.Size > 0;
        }
        private void PlaceNewOrder(ref Quote quote)
        {
            if (!IsValid(ref quote))
            {
                _log.Warn($"{this} is trying to place an order that cannot be executed: {quote}");
                return;
            }

            var commandResult =
                _execution.PlaceNew(
                    Configuration.QuotedSecurity,
                                Configuration.Account.AccountCode,
                                    ref quote);

            if (commandResult.IsError)
            {
                _log.Warn($"{this} failed to place the order {quote}\n{commandResult.Description}");
            }
            else
            {
                _log.Info($"{this} requested to place order {_order}");
                _order = commandResult.Data as IOrder;
            }
        }
        private void ChangeOrder(ref Quote quote)
        {
            if (!IsValid(ref quote))
            {
                _log.Warn($"{this} cannot replace its order {_order} with new parameters {quote}. Order will be canceled.");
                CancelOrder();
                return;
            }

            var commandResult = _execution.Change(_order, quote.Price, quote.Size);

            if (commandResult.IsError)
            {
                _log.Warn($"{this} failed to replace order {_order} with parameters {quote}.\n{commandResult.Description}");
            }
            else
            {
                var neworder = commandResult.Data as IOrder;
                _log.Info($"{this} requested to change order {_order} to {neworder}");
                _order = neworder;
            }
        }
        private void CancelOrder()
        {
            var commandResult = _execution.Cancel(_order);
            
            if (commandResult.IsError)
            {
                _log.Warn($"Could not cancel {_order}. {this}\n{commandResult.Description}");
            }
            else
            {
                _log.Info($"{this} requested to cancel order {_order}");
            }
        }
        private void OnProcessing()
        {
            var quote = _processingQuote;

            var orderQuote = new Quote()
            {
                Operation = quote.Operation,
                Price = CalculatedPrice,
                Size = CalculatedOrderSize
            };

            if (_order is null)
            {
                PlaceNewOrder(ref orderQuote);
                return;
            }

            var needsUpdate = 
                   _order.State != OrderStates.Active
                || _order.RemainingSize != orderQuote.Size
                || _order.Quote.Price < LowerPriceLimit
                || _order.Quote.Price > UpperPriceLimit;

            if (needsUpdate)
            {
                ChangeOrder(ref orderQuote);
            }
        }
        private void OnReadingQuote(Quote[] quotes, Operations operation, long depth)
        {
            // in order to not block the notification thread
            // we'll read and dump the data into a variable
            // then awake the _executionThread to let him pick up and processes the data

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
        private ISecurityBalance? _position;
        private Quote _processingQuote;
        private Decimal5 _priceRange;
        private IOrder? _order;

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
