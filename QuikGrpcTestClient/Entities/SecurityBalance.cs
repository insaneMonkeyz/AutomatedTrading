using TradingConcepts;

namespace QuikGrpcTestClient.Entities
{
    internal class SecurityBalance : ISecurityBalance
    {
        private readonly Quik.Grpc.Entities.SecurityBalance _balance;
        private ISecurity? _security;

        internal SecurityBalance(Quik.Grpc.Entities.SecurityBalance balance)
        {
            _balance = balance;
        }

        ISecurity ISecurityBalance.Security
        {
            get => _security ??= new Security(_balance.Security);
        }

        long ISecurityBalance.Amount => _balance.Amount;
    }
}
