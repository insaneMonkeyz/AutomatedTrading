using Quik.Entities;

namespace Quik.EntityProviders.RequestContainers
{
    internal abstract class SecurityBasedRequestContainer<T> : IRequestContainer<T>
        where T : class
    {
        public SecurityRequestContainer SecurityContainer;

        /// <summary>
        /// only returns true when both ClassCode and Ticker are provided
        /// </summary>
        public virtual bool HasData
        {
            get => !(string.IsNullOrEmpty(SecurityContainer.Ticker) || string.IsNullOrEmpty(SecurityContainer.ClassCode));
        }
        public abstract bool IsMatching(T? entity);

        protected bool SecuritiesMatch(Security security)
        {
            return security.ClassCode == SecurityContainer.ClassCode
                && security.Ticker == SecurityContainer.Ticker;
        }

        public override bool Equals(object? obj)
        {
            return obj is SecurityBasedRequestContainer<T> other
                && SecurityContainer.ClassCode == other.SecurityContainer.ClassCode
                && SecurityContainer.Ticker == other.SecurityContainer.Ticker;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                return HashCode.Combine(SecurityContainer.ClassCode, SecurityContainer.Ticker) * 932576;
            }
        }

    }
}
