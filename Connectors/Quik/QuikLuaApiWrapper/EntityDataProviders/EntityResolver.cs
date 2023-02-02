using System.Runtime.CompilerServices;
using Quik.EntityProviders.RequestContainers;

namespace Quik.EntityProviders
{
    internal delegate TEntity? ResolveEntityHandler<TRequest, TEntity>(TRequest request);

    internal class EntityResolver<TRequest, TEntity>
        where TEntity : class
        where TRequest : IRequestContainer<TEntity>
    {
        private readonly Dictionary<int, TEntity> _cache;
        private readonly ResolveEntityHandler<TRequest, TEntity>? _fetchFromQuik;

        public EntityResolver(int initialCacheSize, ResolveEntityHandler<TRequest, TEntity?> fetchFromQuik)
        {
            _cache = new(initialCacheSize);
            _fetchFromQuik = fetchFromQuik;
        }

        public void CacheEntity(TRequest request, TEntity entity)
        {
            _cache[request.GetHashCode()] = entity;
        }
        public virtual TEntity? GetFromCache(TRequest request)
        {
            if (!request.HasData)
            {
                $"Can't resolve the entity. The request {request} is incomplete".DebugPrintWarning();
                return default;
            }

            return GetFromCacheInternal(request);
        }
        public virtual TEntity? Resolve(TRequest request)
        {
            if (!request.HasData)
            {
                $"Can't resolve the entity. The request {request} is incomplete".DebugPrintWarning();
                return default;
            }

            if (GetFromCacheInternal(request) is TEntity entity)
            {
                return entity;
            }

            entity = _fetchFromQuik?.Invoke(request);

            if(entity != null)
            {
                _cache.Add(request.GetHashCode(), entity);
            }

            return entity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TEntity? GetFromCacheInternal(TRequest request)
        {
            if (_cache.TryGetValue(request.GetHashCode(), out TEntity? entity))
            {
                if (!request.IsMatching(entity))
                {
                    throw new ApplicationException($"Investigate {typeof(TRequest)} hash collisions. " +
                        $"Request{request} hash {request.GetHashCode()}. Entity {entity}");
                }
            }

            return entity;
        }
    }
}
