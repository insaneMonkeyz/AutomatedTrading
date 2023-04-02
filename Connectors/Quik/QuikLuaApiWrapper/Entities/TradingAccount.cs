using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingConcepts;

namespace Quik.Entities
{
    internal class DerivativesTradingAccount : ITradingAccount, IUniquelyIdentifiable, INotifyEntityUpdated
    {
        private string _code;
        private string _firmId;
        private int? _uniqueId;

        public bool IsMoneyAccount { get; init; }
        public long LimitType { get; init; }
        public string AccountCode
        {
            get => _code;
            init => _code = value ?? throw new ArgumentNullException(nameof(AccountCode));
        }
        public string FirmId
        {
            get => _firmId;
            init => _firmId = value ?? throw new ArgumentNullException(nameof(FirmId));
        }
        public string? Description { get; set; }
        public Decimal5 TotalFunds { get; set; }
        public Decimal5 UnusedFunds { get; set; }
        public Decimal5 FloatingIncome { get; set; }
        public Decimal5 CollateralMargin { get; set; }
        public Currencies AccountCurrency { get; init; }
        public string? MoexCurrCode { get; init; }
        public int UniqueId
        {
            get => _uniqueId ??=
                HashCode.Combine(nameof(DerivativesTradingAccount),
                    FirmId, AccountCode);
        }
        IEnumerable<ISecurityBalance> ITradingAccount.SecuritiesBalance => SecuritiesBalance;


        public List<ISecurityBalance> SecuritiesBalance { get; init; } = new(10);

        public event Action Updated = delegate { };

        public override string ToString()
        {
            return $"{AccountCode} Total funds: {AccountCurrency} {TotalFunds.ToString(2u, separateThousands: true)}; " +
                             $"Floating income: {AccountCurrency} {FloatingIncome.ToString(2u, separateThousands: true)};";
        }
    }
}
