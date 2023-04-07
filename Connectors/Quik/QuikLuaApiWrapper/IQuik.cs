using Quik.Entities;
using TradingConcepts;
using TradingConcepts.SecuritySpecifics;
using TradingConcepts.SecuritySpecifics.Options;

namespace Quik
{
    public interface IQuik
    {
        event Action<DateTimeOffset> Connected;
        event Action<DateTimeOffset> ConnectionLost;

        event Action<ISecurityBalance> NewSecurityBalance;
        event Action<IOrderExecution> NewOrderExecution;
        event Action<ITradingAccount> NewAccount;
        event Action<IOrderBook> NewOrderBook;
        event Action<ISecurity> NewSecurity;
        event Action<IOrder> NewOrder;

        event Action<ISecurityBalance> SecurityBalanceChanged;
        event Action<ITradingAccount> AccountChanged;
        event Action<IOrderBook> OrderBookChanged;
        event Action<ISecurity> SecurityChanged;
        event Action<IOrder> OrderChanged;

        IEnumerable<SecurityDescription> GetAvailableSecurities<TSecurity>() where TSecurity : ISecurity;
        TSecurity? GetSecurity<TSecurity>(string ticker) where TSecurity : ISecurity;

        IOrder PlaceNewOrder(MoexOrderSubmission submission);
        void ChangeOrder(IOrder order, Decimal5 newPrice, long newSize);
        void CancelOrder(IOrder order);
    }
}
