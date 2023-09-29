using TradingConcepts;

namespace Quik.EntityProviders.Notification
{
    internal class DirectEntitySignalizer<TEntity> : IEntityEventSignalizer<TEntity>
        where TEntity : class, INotifiableEntity
    {
        public bool IsEnabled => true;

        public void QueueEntity(EntityEventHandler<TEntity> handler, TEntity entity)
        {
            entity.NotifyUpdated();
            handler(entity);
        }

        public void Dispose() { }
    }
}
