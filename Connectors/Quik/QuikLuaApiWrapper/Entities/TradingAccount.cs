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
        private string _code;
        private string _firmId;

        public bool IsMoneyAccount { get; init; }
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
        IEnumerable<ISecurityBalance> ITradingAccount.SecuritiesBalance => SecuritiesBalance;


        public List<ISecurityBalance> SecuritiesBalance { get; init; } = new(10);

        public override string ToString()
        {
            return $"{AccountCode} Total funds: {AccountCurrency} {TotalFunds.ToString(2u)}; Floating income {AccountCurrency} {FloatingIncome.ToString(2u)};";
        }

        public Decimal5 GetBuyMarginRequirements(ISecurity security)
        {
            throw new NotImplementedException();
        }

        public Decimal5 GetSellMarginRequirements(ISecurity security)
        {
            throw new NotImplementedException();
        }
    }
}
