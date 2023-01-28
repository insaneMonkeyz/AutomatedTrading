using Quik.EntityDataProviders.RequestContainers;

namespace Quik.EntityDataProviders
{
    internal delegate TEntity? ResolveEntityHandler<TRequest, TEntity>(TRequest request);

    internal class EntityResolver<TRequest, TResult>
        where TResult : class
        where TRequest : IRequestContainer<TResult>
    {
        private readonly Dictionary<int, TResult> _pool;
        private readonly ResolveEntityHandler<TRequest, TResult>? _fetchFromQuik;

        public EntityResolver(int poolInitialSize, ResolveEntityHandler<TRequest, TResult>? fetcherFromQuik)
        {
            _pool = new(poolInitialSize);
            _fetchFromQuik = fetcherFromQuik;
        }

        public virtual TResult? GetEntity(TRequest request)
        {
            if (!request.HasData)
            {
                throw new ArgumentException("Trying to use an empty request");
            }

            if (_pool.TryGetValue(request.GetHashCode(), out TResult? entity))
            {
                if (request.IsMatching(entity))
                {
                    return (TResult?)entity;
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
