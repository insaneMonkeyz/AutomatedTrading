using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Concepts.Entities;

namespace Core.Concepts.AppComponents
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
