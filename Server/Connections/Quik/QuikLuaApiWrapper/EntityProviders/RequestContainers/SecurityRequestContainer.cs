using TradingConcepts;
using Quik.Entities;

namespace Quik.EntityProviders.RequestContainers
{
    internal struct SecurityRequestContainer : IRequestContainer<Security>, IEquatable<SecurityRequestContainer>
    {
        public string? ClassCode;
        public string? Ticker;

        /// <summary>
        /// only returns true when both ClassCode and Ticker are provided
        /// </summary>
        public bool HasData
        {
            get => !(string.IsNullOrEmpty(Ticker) || string.IsNullOrEmpty(ClassCode));
        }
        public bool IsMatching(Security? entity)
        {
            return entity != null 
                && entity.ClassCode == ClassCode
                && entity.Ticker == Ticker;
        }

        public static SecurityRequestContainer Create(string? classcode, string? ticker)
        {
            return new()
            {
                Ticker = ticker,
                ClassCode = classcode
            };
        }

        public bool Equals(SecurityRequestContainer other)
        {
            return other.ClassCode == ClassCode
                && other.Ticker == Ticker;
        }
        public override bool Equals(object? obj)
        {
            return obj is SecurityRequestContainer other
                && ClassCode == other.ClassCode
                && Ticker == other.Ticker;
        }

        public override string? ToString()
        {
            return $"Security Request: {{ClassCode: {ClassCode ?? "null"}, Ticker: {Ticker ?? "null"}}}";
        }
        public override int GetHashCode()
        {
            unchecked
            {
                return HashCode.Combine(ClassCode, Ticker) * 932576;
            }
        }
    }
}
