namespace Quik.EntityDataProviders
{
    internal interface IDataProvider
    {
        void SubscribeCallback(CallbackSubscriber subscriber);
    }
}
