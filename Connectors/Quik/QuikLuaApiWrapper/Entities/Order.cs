using Quik.EntityProviders.Notification;
using Tools;
using TradingConcepts;
using TradingConcepts.CommonImplementations;

namespace Quik.Entities
{
    internal class Order : IOrder, INotifiableEntity
    {
        private const int DEFAULT_EXECUTIONS_LIST_SIZE = 10;

        private readonly Security _security;
        private string? _exchangeAssignedIdString;
        private string _accountCode;

        #region IOrder
        IEnumerable<IOrderExecution> IOrder.Executions => this.Executions;
        ISecurity IOrder.Security => _security;
        public required string AccountCode
        {
            get => _accountCode;
            init => _accountCode = value ?? throw new ArgumentNullException(nameof(value));
        }
        public required Quote Quote { get; init; }
        public required long TransactionId { get; init; }
        public OrderExecutionConditions ExecutionCondition { get; init; }
        public DateTime Expiry { get; init; }
        public TimeSpan TimeToExpiry => Expiry - DateTime.UtcNow;
        public IOrder? ParentOrder { get; set; }
        public OrderStates State { get; private set; }
        public long ExchangeAssignedId { get; set; }
        public long RemainingSize { get; set; }
        public long ExecutedSize => Quote.Size - RemainingSize;
        public DateTime SubmittedTime { get; set; }
        public DateTime SubmissionReplyTime { get; set; }
        public DateTime ServerCompletionTime { get; set; }
        public DateTime CompletionReplyTime { get; set; }
        #endregion

        #region INotifyEntityUpdated
        public event Action Updated = delegate { };
        public void NotifyUpdated() => Updated();
        #endregion

        internal Security Security => _security;
        public string ExchangeAssignedIdString
        {
            get
            {
                if (_exchangeAssignedIdString == null)
                {
                    if (ExchangeAssignedId == 0)
                    {
                        return string.Empty;
                    }

                    _exchangeAssignedIdString = ExchangeAssignedId.ToString();
                }

                return _exchangeAssignedIdString;
            }
        }
        public List<OrderExecution> Executions { get; } = new(DEFAULT_EXECUTIONS_LIST_SIZE);

        public Order(ISecurity security)
        {
            _security = Helper.CastToMoexSecurity(security);
        }

        public void SetSingleState(OrderStates state)
        {
            lock (this)
            {
#if DEBUG
                switch (state)
                {
                    case OrderStates.Registering:
                    case OrderStates.Changing:
                    case OrderStates.Cancelling:
                        throw new InvalidOperationException($"State {state} is not single");
                }

                EnsureStateCanBeSet(state);
#endif
                State = state; 
            }
        }
        public void AddIntermediateState(OrderStates state)
        {
            lock (this)
            {
#if DEBUG
                switch (state)
                {
                    case OrderStates.None:
                    case OrderStates.Rejected:
                    case OrderStates.Done:
                    case OrderStates.OnHold:
                    case OrderStates.Active:
                        throw new InvalidOperationException($"State {state} is not intermediate");
                }

                EnsureStateCanBeSet(state);
#endif

                State |= state; 
            }
        }
        private void EnsureStateCanBeSet(OrderStates newState)
        {
            var state = State;

            bool isInvalid = newState == OrderStates.Registering && !state.HasFlag(OrderStates.None)
                          || newState == OrderStates.Cancelling && !state.HasFlag(OrderStates.Active)
                          || newState == OrderStates.Changing && !state.HasFlag(OrderStates.Active)
                          || newState == OrderStates.Active && !(state.HasFlag(OrderStates.None) || 
                                                                 state.HasFlag(OrderStates.Registering) || 
                                                                 state.HasFlag(OrderStates.OnHold))
                          || newState == OrderStates.OnHold && !state.HasFlag(OrderStates.Active)
                          || newState == OrderStates.Rejected && !(state.HasFlag(OrderStates.Active) || state.HasFlag(OrderStates.None));


            if (isInvalid)
            {
                throw new InvalidOperationException($"Cannot set {newState} order state when base state is {state}");
            }
        }

        public override string ToString()
        {
            return $"{Security} {Quote} {State}\tExchangeId={ExchangeAssignedIdString} TransactionId={TransactionId}\tCondition={ExecutionCondition} Executed={ExecutedSize} Remainder={RemainingSize}";
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(ExchangeAssignedId, TransactionId);
        }
        public override bool Equals(object? obj)
        {
            return obj is Order another
                && another.ExecutionCondition == ExecutionCondition
                && another.TransactionId == TransactionId
                && another.RemainingSize == RemainingSize
                && another.Expiry == Expiry
                && another.State == State
                && another.Quote == Quote
                && another.SubmittedTime == SubmittedTime
                && another.SubmissionReplyTime == SubmissionReplyTime
                && another.ServerCompletionTime == ServerCompletionTime
                && another.CompletionReplyTime == CompletionReplyTime
                && another._accountCode == _accountCode
                && another._security.Equals(_security)
                && another.Executions.SequenceEqual(Executions)
                && (another.ParentOrder?.Equals(ParentOrder) ?? ParentOrder is null);
        }
    }
}
