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
        public long LimitType;

        public bool HasData
        {
            get => !(string.IsNullOrEmpty(FirmId) || string.IsNullOrEmpty(Account));
        }
        public bool IsMatching(DerivativesTradingAccount? entity)
        {
            return entity != null
                && entity.FirmId == FirmId
                && entity.AccountCode == Account
                && entity.LimitType == LimitType;
        }

        public override bool Equals(object? obj)
        {
            return obj is AccountRequestContainer other
                && FirmId == other.FirmId
                && Account == other.Account
                && LimitType == other.LimitType;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                return HashCode.Combine(FirmId, Account, LimitType) * 10065; 
            }
        }
    }
}
