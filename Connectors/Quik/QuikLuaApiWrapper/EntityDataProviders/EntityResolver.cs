using Quik.EntityProviders.RequestContainers;

namespace Quik.EntityProviders
{
    internal delegate TEntity? ResolveEntityHandler<TRequest, TEntity>(TRequest request);

    internal class EntityResolver<TRequest, TResult>
        where TResult : class
        where TRequest : IRequestContainer<TResult>
    {
        private readonly Dictionary<int, TResult> _cache;
        private readonly ResolveEntityHandler<TRequest, TResult>? _fetchFromQuik;

        public EntityResolver(int initialCacheSize, ResolveEntityHandler<TRequest, TResult?> fetchFromQuik)
        {
            _cache = new(initialCacheSize);
            _fetchFromQuik = fetchFromQuik;
        }

        public virtual TResult? GetEntity(TRequest request)
        {
            if (!request.HasData)
            {
                throw new ArgumentException("Trying to use an empty request");
            }

            if (_cache.TryGetValue(request.GetHashCode(), out TResult? entity))
            {
                if (request.IsMatching(entity))
                {
                    return entity;
                }
                else
                {
                    throw new ApplicationException($"Investigate {typeof(TRequest)} hash collisions. " +
                        $"Request{request} hash {request.GetHashCode()}. Entity {entity}");
                }
            }
            
            return _fetchFromQuik?.Invoke(request);
        }
    }
}
