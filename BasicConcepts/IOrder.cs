﻿using BasicConcepts.SecuritySpecifics;

namespace BasicConcepts
{
    public interface IOrder : IExpiring
    {
        long TransactionId { get; }
        long ExchangeAssignedId { get; }
        OrderExecutionModes ExecutionMode { get; }
        OrderStates State { get; }
        ISecurity Security { get; }
        IQuote Quote { get; }
        bool IsLimit { get; }
        long RemainingSize { get; }
        long ExecutedSize { get; }
        IEnumerable<IOrderExecution> Executions { get; }
    }
}
