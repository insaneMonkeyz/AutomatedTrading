using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicConcepts;
using Quik.Entities;

namespace Quik.EntityProviders.RequestContainers
{
    internal class AccountRequestContainer : IRequestContainer<DerivativesTradingAccount>
    {
        public string? FirmId;
        public string? Account;
        public bool IsMoneyAccount;

        public bool HasData
        {
            get => !(string.IsNullOrEmpty(FirmId) || string.IsNullOrEmpty(Account));
        }
        public bool IsMatching(DerivativesTradingAccount entity)
        {
            return entity != null
                && entity.FirmId == FirmId
                && entity.AccountCode == Account
                && entity.IsMoneyAccount == IsMoneyAccount;
        }

        public override bool Equals(object? obj)
        {
            return obj is AccountRequestContainer other
                && FirmId == other.FirmId
                && Account == other.Account
                && IsMoneyAccount == other.IsMoneyAccount;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                return HashCode.Combine(FirmId, Account, IsMoneyAccount) * 10065; 
            }
        }
    }
}
