﻿using System;
using System.Collections.Concurrent;
using System.Threading;
using TradingConcepts;

namespace Quik.EntityProviders.Notification
{
    internal class EventSignalizer<TEntity> : IEntityEventSignalizer<TEntity>
        where TEntity : class, INotifiableEntity
    {
        protected readonly Queue<(EntityEventHandler<TEntity>, TEntity)> _entitiesToSend = new();
        private readonly ExecutionLoop _loop;
        private bool _enabled;
        private bool _disposed;

        public bool IsEnabled
        {
            get => _enabled;
            set
            {
                if (_enabled ^ value)
                {
                    if (value)
                    {
                        _loop.Execute += Signalize;
                    }
                    else
                    {
                        _loop.Execute -= Signalize;
                    }

                    _enabled = value;
                }
            }
        }

        public void QueueEntity(EntityEventHandler<TEntity> handler, TEntity entity)
        {
            _entitiesToSend.Enqueue((handler, entity));
        }

        protected void Signalize()
        {
            if (_entitiesToSend.TryDequeue(out (EntityEventHandler<TEntity> Handle, TEntity Entity) param))
            {
                param.Entity.NotifyUpdated();
                param.Handle(param.Entity);
            }
        }

        public EventSignalizer(ExecutionLoop loop)
        {
            _loop = loop;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            IsEnabled = false;
            _entitiesToSend.Clear();
        }
    }
}
