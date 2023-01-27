using Quik.Entities;

namespace Quik.EntityDataProviders.RequestContainers
{
    internal class SecurityRequestContainer : SecurityBasedRequestContainer<Security>
    {
        public override bool IsMatching(Security? entity)
        {
            return entity != null && SecuritiesMatch(entity);
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
    }
}
