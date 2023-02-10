namespace Quik.EntityProviders
{
    internal interface IEntityEventSignalizer<TEntity> : IDisposable
    {
        bool IsEnabled { get; }
        void QueueEntity(EntityEventHandler<TEntity> handler, TEntity entity);
    }
}
