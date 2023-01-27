using Quik.Entities;

namespace Quik.EntityDataProviders.RequestContainers
{
    internal class SecurityBalanceRequestContainer : IRequestContainer<SecurityBalance>
    {
        public string? FirmId;
        public string? ClientCode;
        public string? Ticker;

        /// <summary>
        /// only returns true when both ClassCode and Ticker are provided
        /// </summary>
        public bool HasData
        {
            get
            {
               return !(string.IsNullOrEmpty(Ticker)
                || string.IsNullOrEmpty(FirmId)
                || string.IsNullOrEmpty(ClientCode));
            }
        }

        public bool IsMatching(SecurityBalance balance)
        {
            if (balance == null)
            {
                return false;
            }

            return balance.FirmId == FirmId
                && balance.AccountId == ClientCode
                && balance.Security.Ticker == Ticker;
        }

        public override bool Equals(object? obj)
        {
            return obj is SecurityBalanceRequestContainer other
                && FirmId == other.FirmId
                && ClientCode == other.ClientCode
                && Ticker == other.Ticker;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                return HashCode.Combine(FirmId, ClientCode, Ticker) * 345756; 
            }
        }
    }
}
