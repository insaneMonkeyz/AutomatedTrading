using Quik.Lua;

namespace Quik.EntityProviders
{
    internal interface IQuikDataSubscriber
    {
        void SubscribeCallback(LuaWrap state);
    }
}
