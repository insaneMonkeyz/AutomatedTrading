namespace Quik.EntityProviders.Notification
{
    internal class DirectEntitySignalizer<TEntity> : IEntityEventSignalizer<TEntity>
    {
        public bool IsEnabled => true;

        public void QueueEntity(EntityEventHandler<TEntity> handler, TEntity entity)
        {
            handler(entity);
        }

        public void Dispose() { }
    }
}
