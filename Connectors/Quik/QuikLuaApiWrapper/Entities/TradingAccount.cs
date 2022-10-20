using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicConcepts;
using QuikLuaApi.Entities;

namespace QuikLuaApiWrapper.Entities
{
    internal class DerivativesTradingAccount : ITradingAccount
    {
        public string Code { get; init; }
        public string FirmId { get; init; }
        public string Description { get; set; }
        public Decimal5 TotalFunds { get; set; }
        public Decimal5 UnusedFunds { get; set; }
        public Decimal5 FloatingIncome { get; set; }
        public Currencies AccountCurrency { get; set; }
        public Decimal5 CollateralMargin { get; set; }
        IEnumerable<ISecurityBalance> ITradingAccount.SecuritiesBalance => SecuritiesBalance;


        public List<ISecurityBalance> SecuritiesBalance { get; init; } = new(10);

    }
}
