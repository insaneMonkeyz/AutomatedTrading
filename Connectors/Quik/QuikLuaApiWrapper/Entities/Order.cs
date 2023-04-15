using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quik.EntityProviders.Notification;
using TradingConcepts;
using TradingConcepts.CommonImplementations;

namespace Quik.Entities
{
    internal class Order : MoexOrderSubmission, IOrder, INotifiableEntity
    {
        private const int DEFAULT_EXECUTIONS_LIST_SIZE = 10;

        private string? _exchangeAssignedIdString;

        public List<OrderExecution> Executions { get; } = new(DEFAULT_EXECUTIONS_LIST_SIZE);
        IEnumerable<IOrderExecution> IOrder.Executions => this.Executions;

        public OrderStates State { get; private set; }
        public string? ExchangeAssignedIdString 
        {
            get => _exchangeAssignedIdString ??= ExchangeAssignedId.ToString();
        }
        public long ExchangeAssignedId { get; set; }
        public long RemainingSize { get; set; }
        public long ExecutedSize => Quote.Size - RemainingSize;
        public DateTimeOffset Submitted { get; } = DateTimeOffset.Now;

        public event Action Updated = delegate { };

        [SetsRequiredMembers]
        public Order(MoexOrderSubmission submission) : base(submission.Security)
        {
            base.ExecutionCondition = submission.ExecutionCondition;
            base.TransactionId = submission.TransactionId;
            base.AccountCode = submission.AccountCode;
            base.ClientCode = submission.ClientCode;
            base.IsMarket = submission.IsMarket;
            base.Expiry = submission.Expiry;
            base.Quote = submission.Quote;
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

        public void NotifyUpdated() => Updated();

        public override string ToString()
        {
            return $"{Security} {Quote} {State} {ExchangeAssignedIdString} {ExecutionCondition} Executed={ExecutedSize} Remainder={RemainingSize}";
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
    }
}
