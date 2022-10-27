using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicConcepts
{
    public interface ITradingAccount
    {
        string AccountCode { get; }
        string? Description { get; }
        Decimal5 TotalFunds { get; }
        Decimal5 UnusedFunds { get; }
        Decimal5 FloatingIncome { get; }
        Currencies AccountCurrency { get; }
        Decimal5 CollateralMargin { get; }
        IEnumerable<ISecurityBalance> SecuritiesBalance { get; }
        Decimal5 GetBuyMarginRequirements(ISecurity security);
        Decimal5 GetSellMarginRequirements(ISecurity security);
    }
}
