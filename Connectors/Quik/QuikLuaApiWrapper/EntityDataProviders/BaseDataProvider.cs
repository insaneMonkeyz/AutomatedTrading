using Quik;
using Quik.EntityDataProviders.QuikApiWrappers;
using Quik.Entities;

namespace Quik.EntityDataProviders
{
    internal delegate void EntityStateChangedHandler<T>(T entity);
    internal delegate TEntity? GetChangingEntityHandler<TEntity, TDummy>(TDummy dummy);

    internal abstract class BaseDataProvider<TEntity,TDummy> where TDummy : new()
    {
        protected abstract string QuikCallbackMethod { get; }

        protected readonly TDummy _dummy = new();
        protected readonly object _callbackLock = new();
        protected readonly object _userRequestLock = new();

        public EntityStateChangedHandler<TEntity> EntityChanged = delegate { };
        public GetChangingEntityHandler<TEntity, TDummy> GetChangingEntity = delegate { return default; };

        public void SubscribeCallback(CallbackSubscriber subscribe)
        {
            subscribe(OnEntityChanged, QuikCallbackMethod);
        }
        public abstract void Update(TEntity entity);

        protected abstract void Update(TEntity entity, LuaState state);
        protected abstract void SetDummy(LuaState state);
        protected abstract TEntity? Create(LuaState state);

        private int OnEntityChanged(IntPtr state)
        {
            lock (_callbackLock)
            {
                SetDummy(state);

                if (GetChangingEntity(_dummy) is TEntity entity)
                {
                    Update(entity, state);

                    EntityChanged(entity);
                } 
            }

            return 1;
        }
    }
}
