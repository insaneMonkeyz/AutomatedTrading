using System;
using System.Collections.Generic;
using BasicConcepts;

namespace Core.AppComponents.BusinessLogicConcepts
{
    public interface IAccountProvider
    {
        Guid Id { get; }
        IEnumerable<ITradingAccount> GetAvailableAccounts();
        Decimal5 GetBuyMarginRequirements(ISecurity security);
        Decimal5 GetSellMarginRequirements(ISecurity security);
    }
}
