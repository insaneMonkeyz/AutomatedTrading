namespace Quik.EntityDataProviders
{
    internal interface IQuikDataSubscriber
    {
        void SubscribeCallback(LuaState state);
    }
}
