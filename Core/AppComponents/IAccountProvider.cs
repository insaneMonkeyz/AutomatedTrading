using System;
using System.Collections.Generic;
using BasicConcepts;

namespace Core.AppComponents
{
    public interface IAccountProvider
    {
        Guid Id { get; }
        string Description { get; }
        long TotalBalance { get; }
        long UnfixedProfit { get; }
        string AccountCurrency { get; }
        long CollateralMargin { get; }
        IReadOnlyDictionary<ISecurity, int> SecuritiesBalance { get; }
    }
}
