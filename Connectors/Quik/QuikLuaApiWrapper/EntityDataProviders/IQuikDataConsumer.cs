namespace Quik.EntityProviders
{
    internal interface IQuikDataConsumer : IDisposable
    {
        void Initialize(ExecutionLoop internalRoutinesThread);
        void SubscribeCallback();
        void UnsubscribeCallback();
    }
}
