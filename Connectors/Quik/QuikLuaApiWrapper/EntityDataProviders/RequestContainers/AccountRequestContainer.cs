using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicConcepts;
using Quik.Entities;

namespace Quik.EntityDataProviders.RequestContainers
{
    internal class AccountRequestContainer : IRequestContainer<DerivativesTradingAccount>
    {
        public string? FirmId;
        public string? ClientCode;
        public bool IsMoneyAccount;

        public bool HasData
        {
            get => !(string.IsNullOrEmpty(FirmId) || string.IsNullOrEmpty(ClientCode));
        }
        public bool IsMatching(DerivativesTradingAccount entity)
        {
            return entity != null
                && entity.FirmId == FirmId
                && entity.AccountCode == ClientCode
                && entity.IsMoneyAccount == IsMoneyAccount;
        }

        public override bool Equals(object? obj)
        {
            return obj is AccountRequestContainer other
                && FirmId == other.FirmId
                && ClientCode == other.ClientCode
                && IsMoneyAccount == other.IsMoneyAccount;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                return HashCode.Combine(FirmId, ClientCode, IsMoneyAccount) * 10065; 
            }
        }
    }
}
