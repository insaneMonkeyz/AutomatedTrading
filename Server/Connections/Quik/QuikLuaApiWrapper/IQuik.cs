using Quik.Entities;
using TradingConcepts;
using TradingConcepts.CommonImplementations;
using TradingConcepts.SecuritySpecifics;
using TradingConcepts.SecuritySpecifics.Options;

namespace Quik
{
    public interface IQuik : IDisposable
    {
        event Action<DateTime> Connected;
        event Action<DateTime> ConnectionLost;

        event Action<ISecurityBalance> NewSecurityBalance;
        event Action<IOrderExecution> NewOrderExecution;
        event Action<ITradingAccount> NewAccount;
        event Action<IOrderBook> NewOrderBook;
        event Action<ISecurity> NewSecurity;
        event Action<IOrder> NewOrder;
        event Action<ITrade> NewTrade;

        event Action<ISecurityBalance> SecurityBalanceChanged;
        event Action<ITradingAccount> AccountChanged;
        event Action<IOrderBook> OrderBookChanged;
        event Action<ISecurity> SecurityChanged;
        event Action<IOrder> OrderChanged;

        event Action<IOrder> OrderChangeDenied;
        event Action<IOrder> OrderCancellationDenied;

        Guid Id { get; }
        bool IsConnected { get; }
        ITradingAccount? DerivativesAccount { get; }

        int Initialize(IntPtr luaContext);

        IEnumerable<ITrade> GetTrades();
        IEnumerable<IOrder> GetOrders();
        IEnumerable<IOrderExecution> GetOrderExecutions();
        IEnumerable<ISecurityBalance> GetSecuritiesBalances();
        IEnumerable<SecurityDescription> GetAvailableSecurities<TSecurity>() where TSecurity : ISecurity;
        IEnumerable<SecurityDescription> GetAvailableSecurities(Type securityType);
        TSecurity? GetSecurity<TSecurity>(string ticker) where TSecurity : ISecurity;
        IOrderBook? GetOrderBook(Type? securityType, string? ticker);

        IOrder CreateOrder(ISecurity? security, string? accountCode, ref Quote quote, OrderExecutionConditions? ExecutionCondition = default, DateTime? expiry = default);
        IOrder CreateDerivativeOrder(IDerivative security, ref Quote quote, OrderExecutionConditions ExecutionCondition = default);

        void PlaceNewOrder(IOrder? order);
        IOrder? ChangeOrder(IOrder? order, Decimal5 newPrice, long newSize);
        void CancelOrder(IOrder? order);
    }
}
