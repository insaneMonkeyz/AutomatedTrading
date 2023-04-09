using Quik.Entities;

namespace Quik.EntityProviders.RequestContainers
{
    internal struct SecurityBalanceRequestContainer : IRequestContainer<SecurityBalance>, IEquatable<SecurityBalanceRequestContainer>
    {
        public string? FirmId;
        public string? Account;
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
                || string.IsNullOrEmpty(Account));
            }
        }

        public bool IsMatching(SecurityBalance? balance)
        {
            return balance != null
                && balance.FirmId == FirmId
                && balance.Account == Account
                && balance.Security.Ticker == Ticker;
        }

        public bool Equals(SecurityBalanceRequestContainer other)
        {
            return Ticker == other.Ticker
                && FirmId == other.FirmId
                && Account == other.Account;
        }

        public override bool Equals(object? obj)
        {
            return obj is SecurityBalanceRequestContainer other
                && FirmId == other.FirmId
                && Account == other.Account
                && Ticker == other.Ticker;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(FirmId, Account, Ticker);
        }
    }
}
