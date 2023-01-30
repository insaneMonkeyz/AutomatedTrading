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

        public override Security? GetEntity(SecurityRequestContainer request)
        {
            if (string.IsNullOrWhiteSpace(request.Ticker))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(request.ClassCode))
            {
                request.ClassCode = _classcodesMap.GetClassCode(request.Ticker);
            }

            return base.GetEntity(request);
        }

        private static int EstimateCacheSize(SecuritiesToClasscodesMap map)
        {
            return Math.Max(0, map.TotalSecuritiesCount / 4);
        }
    }
}
