namespace Quik.EntityProviders
{
    internal interface IQuikDataSubscriber
    {
        void SubscribeCallback(LuaState state);
    }
}
