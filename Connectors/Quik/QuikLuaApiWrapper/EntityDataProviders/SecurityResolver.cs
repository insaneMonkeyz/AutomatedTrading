using System.Runtime.CompilerServices;
using Quik.Entities;
using Quik.EntityProviders.RequestContainers;
using EntityFetcher = Quik.EntityProviders.ResolveEntityHandler<Quik.EntityProviders.RequestContainers.SecurityRequestContainer, Quik.Entities.Security>;

namespace Quik.EntityProviders
{
    internal class SecurityResolver : EntityResolver<SecurityRequestContainer, Security>
    {
        private readonly SecuritiesToClasscodesMap _classcodesMap;

        public SecurityResolver(EntityFetcher? fetchFromQuik, SecuritiesToClasscodesMap classcodesMap)
            : base(EstimateCacheSize(classcodesMap), fetchFromQuik)
        {
            _classcodesMap = classcodesMap;
        }

        public override Security? GetFromCache(SecurityRequestContainer request)
        {
            CompleteRequest(ref request);
            return base.GetFromCache(request);
        }

        public override Security? Resolve(SecurityRequestContainer request)
        {
            CompleteRequest(ref request);
            return base.Resolve(request);
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
