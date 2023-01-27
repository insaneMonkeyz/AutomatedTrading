using Quik.EntityDataProviders.RequestContainers;

namespace Quik.EntityDataProviders
{
    internal class EntityResolver<TRequest, TResult>
        where TResult : class
        where TRequest : IRequestContainer<TResult>
    {
        private readonly Dictionary<int, TResult> _pool;

        public EntityResolver(int poolInitialSize)
        {
            _pool = new(poolInitialSize);
        }

        public virtual TResult? GetEntity(TRequest request)
        {
            if (!request.HasData)
            {
                throw new ArgumentException("Trying to use an empty request");
            }

            return _pool.TryGetValue(request.GetHashCode(), out TResult? entity)
                && request.IsMatching(entity)
                ? entity
                : default;
        }
    }
}
