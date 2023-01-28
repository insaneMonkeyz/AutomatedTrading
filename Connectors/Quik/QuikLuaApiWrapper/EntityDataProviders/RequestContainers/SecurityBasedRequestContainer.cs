using Quik.Entities;

namespace Quik.EntityDataProviders.RequestContainers
{
    internal abstract class SecurityBasedRequestContainer<T> : IRequestContainer<T>
        where T : class
    {
        public string? ClassCode;
        public string? Ticker;

        /// <summary>
        /// only returns true when both ClassCode and Ticker are provided
        /// </summary>
        public virtual bool HasData
        {
            get => !(string.IsNullOrEmpty(Ticker) || string.IsNullOrEmpty(ClassCode));
        }
        public abstract bool IsMatching(T? entity);

        protected bool SecuritiesMatch(Security security)
        {
            return security.ClassCode == ClassCode
                && security.Ticker == Ticker;
        }

        public override bool Equals(object? obj)
        {
            return obj is SecurityBasedRequestContainer<T> other
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
