using TradingConcepts;

namespace Quik.EntityProviders.Notification
{
    internal interface IEntityEventSignalizer<TEntity> : IDisposable
        where TEntity : class, INotifiableEntity
    {
        bool IsEnabled { get; }
        void QueueEntity(EntityEventHandler<TEntity> handler, TEntity entity);
    }
}
