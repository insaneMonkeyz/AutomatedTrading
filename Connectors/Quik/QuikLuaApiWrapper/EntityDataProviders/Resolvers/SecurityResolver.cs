using System.Diagnostics;
using System.Runtime.CompilerServices;
using Quik.Entities;
using Quik.EntityProviders.RequestContainers;
using TradingConcepts;
using EntityFetcher = Quik.EntityProviders.Resolvers.ResolveEntityHandler<Quik.EntityProviders.RequestContainers.SecurityRequestContainer, Quik.Entities.Security>;

namespace Quik.EntityProviders.Resolvers
{
    internal class SecurityResolver : EntityResolver<SecurityRequestContainer, Security>
    {
        private readonly SecuritiesToClasscodesMap _classcodesMap;

        public SecurityResolver(EntityFetcher? fetchFromQuik, SecuritiesToClasscodesMap classcodesMap)
            : base(EstimateCacheSize(classcodesMap), fetchFromQuik)
        {
            _classcodesMap = classcodesMap;
        }

        public override Security? GetFromCache(ref SecurityRequestContainer request)
        {
            CompleteRequest(ref request);
            return base.GetFromCache(ref request);
        }

        public TSecurity? Resolve<TSecurity>(string ticker) where TSecurity : ISecurity
        {
            throw new NotImplementedException();
        }

        public override Security? Resolve(ref SecurityRequestContainer request)
        {
            CompleteRequest(ref request);
            return base.Resolve(ref request);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CompleteRequest(ref SecurityRequestContainer request)
        {
            if (!string.IsNullOrWhiteSpace(request.Ticker) && string.IsNullOrWhiteSpace(request.ClassCode))
            {
                request.ClassCode = _classcodesMap.GetClassCode(request.Ticker);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int EstimateCacheSize(SecuritiesToClasscodesMap map)
        {
            return Math.Max(0, map.TotalSecuritiesCount / 4);
        }
    }
}
