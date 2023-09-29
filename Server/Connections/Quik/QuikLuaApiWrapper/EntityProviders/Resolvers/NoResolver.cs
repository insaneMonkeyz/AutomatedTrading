using Quik.EntityProviders.RequestContainers;

namespace Quik.EntityProviders.Resolvers
{
    internal sealed class NoResolver<TRequest, TEntity> : EntityResolver<TRequest, TEntity>
        where TEntity : class
        where TRequest : struct, IRequestContainer<TEntity>
    {
        #region Singleton
        public static NoResolver<TRequest, TEntity> Instance { get; } = new();
        private NoResolver() : base(0, default) { }
        #endregion

        public override void CacheEntity(ref TRequest request, TEntity entity) { }
        public override TEntity? GetFromCache(ref TRequest request) => default;
        public override TEntity? Resolve(ref TRequest request) => default;
    }
}
