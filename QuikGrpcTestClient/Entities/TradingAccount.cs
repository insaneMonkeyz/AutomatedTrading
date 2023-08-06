using TradingConcepts;

namespace QuikGrpcTestClient.Entities
{
    internal class TradingAccount : ITradingAccount
    {
        private readonly Quik.Grpc.Entities.TradingAccount _account;
        private IEnumerable<ISecurityBalance>? _securityBalances;

        internal TradingAccount(Quik.Grpc.Entities.TradingAccount account) 
        {
            _account = account;
        }

        string ITradingAccount.AccountCode => _account.AccountCode;

        string? ITradingAccount.Description => _account.HasDescription ? _account.Description : default;

        Decimal5 ITradingAccount.TotalFunds => Decimal5.FromMantissa(_account.TotalFunds.Mantissa);

        Decimal5 ITradingAccount.UnusedFunds => Decimal5.FromMantissa(_account.UnusedFunds.Mantissa);

        Decimal5 ITradingAccount.FloatingIncome => Decimal5.FromMantissa(_account.FloatingIncome.Mantissa);

        Currencies ITradingAccount.AccountCurrency => Enum.Parse<Currencies>(_account.AccountCurrency);

        Decimal5 ITradingAccount.CollateralMargin => Decimal5.FromMantissa(_account.CollateralMargin.Mantissa);

        IEnumerable<ISecurityBalance> ITradingAccount.SecuritiesBalance
        {
            get
            {
                return _securityBalances ??= _account.SecurityBalance.Select(sb => new SecurityBalance(sb)).ToArray();
            }
        }
    }
}
