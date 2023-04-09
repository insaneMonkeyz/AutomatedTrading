﻿using System.Runtime.CompilerServices;
using Quik.EntityProviders.RequestContainers;

namespace Quik.EntityProviders.Resolvers
{
    internal delegate TEntity? ResolveEntityHandler<TRequest, TEntity>(ref TRequest request);

    internal class EntityResolver<TRequest, TEntity>
        where TEntity : class
        where TRequest : struct, IRequestContainer<TEntity>
    {
        private readonly object _resolveInProgress = new();
        private readonly Dictionary<int, TEntity> _cache;
        private readonly ResolveEntityHandler<TRequest, TEntity>? _fetchFromQuik;
        private readonly Log _log = LogManagement.GetLogger(typeof(EntityResolver<TRequest, TEntity>));

        public EntityResolver(int initialCacheSize, ResolveEntityHandler<TRequest, TEntity?> fetchFromQuik)
        {
            _cache = new(initialCacheSize);
            _fetchFromQuik = fetchFromQuik;
        }

        public virtual void CacheEntity(ref TRequest request, TEntity entity)
        {
            lock (_resolveInProgress)
            {
                _cache[request.GetHashCode()] = entity;
            }
        }
        public virtual TEntity? GetFromCache(ref TRequest request)
        {
            if (!request.HasData)
            {
                _log.Warn($"Can't resolve the entity. The request {request} is incomplete");
                return default;
            }

            lock (_resolveInProgress)
            {
                return GetFromCacheInternal(ref request);
            }
        }
        public virtual TEntity? Resolve(ref TRequest request)
        {
            if (!request.HasData)
            {
                _log.Warn($"Can't resolve the entity. The request {request} is incomplete");
                return default;
            }

            lock (_resolveInProgress)
            {
                if (GetFromCacheInternal(ref request) is TEntity entity)
                {
                    return entity;
                }

                entity = _fetchFromQuik?.Invoke(ref request);

                if (entity != null)
                {
                    _cache.Add(request.GetHashCode(), entity);
                }

                return entity;
            }
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