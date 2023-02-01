using BasicConcepts;
using Quik.Entities;

namespace Quik.EntityProviders.RequestContainers
{
    internal struct SecurityRequestContainer : IRequestContainer<Security>
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

        public override string? ToString()
        {
            if (string.IsNullOrEmpty(Ticker))
            {
                return "Empty Security Request";
            }

            return string.IsNullOrEmpty(ClassCode)
                    ? Ticker
                    : $"{ClassCode}:{Ticker}";
        }

        public override bool Equals(object? obj)
        {
            return obj is SecurityRequestContainer other
                && ClassCode == other.ClassCode
                && Ticker == other.Ticker;
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
