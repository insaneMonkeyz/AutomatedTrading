using QuikLuaApi;

namespace Quik.ApiWrapper
{
    internal abstract class BaseWrapper<T>
    {
        protected LuaState _state;

        internal T? CreateFromCallback()
        {
            return _state != default ? Create(_state) : default;
        }
        internal abstract List<T> GetAllEntities();
        internal abstract void Update(T account);
        internal abstract void Subscribe(CallbackSubscriber subscribe);

        protected abstract int OnEntityChanged(IntPtr state);
        protected abstract T Create(LuaState state);
    }
}
