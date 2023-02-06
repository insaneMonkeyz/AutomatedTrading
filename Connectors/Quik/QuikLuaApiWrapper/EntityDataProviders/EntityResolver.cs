using System.Runtime.CompilerServices;
using Quik.EntityProviders.RequestContainers;

namespace Quik.EntityProviders
{
    internal delegate TEntity? ResolveEntityHandler<TRequest, TEntity>(ref TRequest request);

    internal class EntityResolver<TRequest, TEntity>
        where TEntity : class
        where TRequest : struct, IRequestContainer<TEntity>
    {
        private readonly Dictionary<int, TEntity> _cache;
        private readonly ResolveEntityHandler<TRequest, TEntity>? _fetchFromQuik;

        public EntityResolver(int initialCacheSize, ResolveEntityHandler<TRequest, TEntity?> fetchFromQuik)
        {
            _cache = new(initialCacheSize);
            _fetchFromQuik = fetchFromQuik;
        }

        public void CacheEntity(ref TRequest request, TEntity entity)
        {
            _cache[request.GetHashCode()] = entity;
        }
        public virtual TEntity? GetFromCache(ref TRequest request)
        {
            if (!request.HasData)
            {
                $"Can't resolve the entity. The request {request} is incomplete".DebugPrintWarning();
                return default;
            }

            return GetFromCacheInternal(ref request);
        }
        public virtual TEntity? Resolve(ref TRequest request)
        {
            if (!request.HasData)
            {
                $"Can't resolve the entity. The request {request} is incomplete".DebugPrintWarning();
                return default;
            }

            if (GetFromCacheInternal(ref request) is TEntity entity)
            {
                return entity;
            }

            entity = _fetchFromQuik?.Invoke(ref request);

            if(entity != null)
            {
                _cache.Add(request.GetHashCode(), entity);
            }

            return entity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TEntity? GetFromCacheInternal(ref TRequest request)
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
