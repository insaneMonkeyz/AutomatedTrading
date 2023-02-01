using System.Collections.Concurrent;

namespace Quik.EntityProviders
{
    internal class EventSignalizer<TEntity>
        where TEntity : class
    {
        private readonly ConcurrentQueue<(EntityEventHandler<TEntity>, TEntity)> _entitiesToSend = new();
        private bool _running = false;
        private Thread? _thread;

        public void QueueEntity<THandler>(EntityEventHandler<TEntity> handler, TEntity entity)
        {
            _entitiesToSend.Enqueue((handler, entity));
        }
        public void Start()
        {
            if (!_running)
            {
                _running = true;
                _thread = new Thread(Loop);
                _thread.Start(); 
            }
        }
        public void Stop()
        {
            if (_running)
            {
                _running = false;
                _thread = null;
                _entitiesToSend.Clear();
            }
        }

        private void Loop()
        {
            while (_running)
            {
                while (_entitiesToSend.TryDequeue(out (EntityEventHandler<TEntity> Handle, TEntity Entity) param))
                {
                    try
                    {
                        param.Handle(param.Entity);
                    }
                    catch (Exception e)
                    {
                        e.ToString().DebugPrintWarning();
                    }
                }
            }
        }
    }
}
